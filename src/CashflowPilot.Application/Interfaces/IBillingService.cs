namespace CashflowPilot.Application.Interfaces;

public interface IBillingService
{
    Task<string> CreateCheckoutSessionAsync(int organizationId, string planTier, string successUrl, string cancelUrl);
    Task<string> CreateBillingPortalSessionAsync(int organizationId, string returnUrl);
    Task HandleWebhookAsync(string json, string stripeSignature);
}
