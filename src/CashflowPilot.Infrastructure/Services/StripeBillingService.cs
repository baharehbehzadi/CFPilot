using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace CashflowPilot.Infrastructure.Services;

public class StripeBillingService : IBillingService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<StripeBillingService> _logger;
    private readonly string _secretKey;
    private readonly string _webhookSecret;
    private readonly string _starterPriceId;
    private readonly string _professionalPriceId;

    public StripeBillingService(ApplicationDbContext db, IConfiguration configuration, ILogger<StripeBillingService> logger)
    {
        _db = db;
        _logger = logger;
        _secretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
        _starterPriceId = configuration["Stripe:StarterPriceId"] ?? string.Empty;
        _professionalPriceId = configuration["Stripe:ProfessionalPriceId"] ?? string.Empty;
        StripeConfiguration.ApiKey = _secretKey;
    }

    private string PriceIdFor(string planTier) =>
        planTier.Equals("Professional", StringComparison.OrdinalIgnoreCase) ? _professionalPriceId : _starterPriceId;

    public async Task<string> CreateCheckoutSessionAsync(int organizationId, string planTier, string successUrl, string cancelUrl)
    {
        var org = await _db.Organizations.FindAsync(organizationId)
            ?? throw new InvalidOperationException($"Organization {organizationId} not found");

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = PriceIdFor(planTier), Quantity = 1 }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = organizationId.ToString(),
        };

        if (!string.IsNullOrEmpty(org.StripeCustomerId))
            options.Customer = org.StripeCustomerId;

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreateBillingPortalSessionAsync(int organizationId, string returnUrl)
    {
        var org = await _db.Organizations.FindAsync(organizationId)
            ?? throw new InvalidOperationException($"Organization {organizationId} not found");

        if (string.IsNullOrEmpty(org.StripeCustomerId))
            throw new InvalidOperationException("This organization has no billing account yet. Subscribe to a plan first.");

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = org.StripeCustomerId,
            ReturnUrl = returnUrl
        });
        return session.Url;
    }

    public async Task HandleWebhookAsync(string json, string stripeSignature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutCompletedAsync(stripeEvent);
                break;
            case EventTypes.CustomerSubscriptionUpdated:
                await HandleSubscriptionUpdatedAsync(stripeEvent);
                break;
            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionDeletedAsync(stripeEvent);
                break;
            case EventTypes.InvoicePaymentFailed:
                await HandleInvoicePaymentFailedAsync(stripeEvent);
                break;
            default:
                _logger.LogInformation("Unhandled Stripe webhook event type: {Type}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleCheckoutCompletedAsync(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null || !int.TryParse(session.ClientReferenceId, out var orgId)) return;

        var org = await _db.Organizations.FindAsync(orgId);
        if (org == null) return;

        org.StripeCustomerId = session.CustomerId;
        org.StripeSubscriptionId = session.SubscriptionId;
        org.SubscriptionStatus = SubscriptionStatus.Active;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Organization {OrgId} subscription activated via checkout {SessionId}", orgId, session.Id);
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.StripeSubscriptionId == subscription.Id);
        if (org == null) return;

        org.SubscriptionStatus = MapStatus(subscription.Status);
        org.CurrentPeriodEnd = subscription.CurrentPeriodEnd;
        await _db.SaveChangesAsync();
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.StripeSubscriptionId == subscription.Id);
        if (org == null) return;

        org.SubscriptionStatus = SubscriptionStatus.Canceled;
        await _db.SaveChangesAsync();
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null || string.IsNullOrEmpty(invoice.CustomerId)) return;

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.StripeCustomerId == invoice.CustomerId);
        if (org == null) return;

        org.SubscriptionStatus = SubscriptionStatus.PastDue;
        await _db.SaveChangesAsync();
    }

    private static SubscriptionStatus MapStatus(string stripeStatus) => stripeStatus switch
    {
        "active" or "trialing" => SubscriptionStatus.Active,
        "past_due" or "unpaid" or "incomplete" => SubscriptionStatus.PastDue,
        "canceled" or "incomplete_expired" => SubscriptionStatus.Canceled,
        _ => SubscriptionStatus.PastDue
    };
}
