using CashflowPilot.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashflowPilot.Web.Controllers;

[Route("webhooks/stripe")]
[AllowAnonymous]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly IBillingService _billing;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IBillingService billing, ILogger<StripeWebhookController> logger)
    {
        _billing = billing;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        string json;
        using (var reader = new StreamReader(Request.Body))
        {
            json = await reader.ReadToEndAsync();
        }

        if (!Request.Headers.TryGetValue("Stripe-Signature", out var signature))
        {
            return BadRequest();
        }

        try
        {
            await _billing.HandleWebhookAsync(json, signature.ToString());
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stripe webhook processing failed");
            return BadRequest();
        }
    }
}
