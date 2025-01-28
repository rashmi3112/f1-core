using System.Net;
using API.Extensions;
using API.SignalR;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Stripe;

namespace API.Controllers;

public class PaymentsController(IPaymentService paymentService,
    IUnitOfWork unit, ILogger<PaymentsController> logger,
    IConfiguration config, IHubContext<NotificationHub> hubContext) : BaseApiController
{
    private readonly string _whSecret = config["StripeSettings:WhSecret"]!;

    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);

        if (cart == null) return BadRequest("Problem occured with your cart");

        return Ok(cart);
    }

    [HttpGet("delivery-methods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethod()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        logger.LogInformation("Stripe webhok payload received: {json}", json);

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            // logger.LogInformation("Stripe-Signature: {StripeSignature}", stripeSignature);
            logger.LogInformation($"Stripe-Signature: {stripeSignature}");

            var stripeEvent = ConstructStripeEvent(json);
            logger.LogInformation("Stripe Event Type: {EventType}", stripeEvent.Type);

            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                logger.LogWarning("Stripe event data is not a PaymentIntent");
                return BadRequest("Invalid event data");
            }

            await HandlePaymentSucceeded(intent);

            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook error: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during webhook procesing");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    private async Task HandlePaymentSucceeded(PaymentIntent intent)
    {
        if (intent.Status != "succeeded") return;

        logger.LogInformation("Handling PaymnetIntent with ID: {PaymentIntentId}", intent.Id);

        var spec = new OrderSpecification(intent.Id, true);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null)
        {
            logger.LogError("Order not found PaymentIntentId: {PaymentIntentId}", intent.Id);
            throw new Exception("Order not found");
        }

        logger.LogInformation("Order found for PaymentIntentId: {PaymentIntentId}, orderId: {OrderId}", intent.Id, order.Id);

        if ((long)order.GetTotal() * 100 != intent.Amount)
        {
            logger.LogWarning("Payment amount mismatch expected: {Expected}, Actual: {Actual}",
                (long)order.GetTotal() * 100, intent.Amount);
            order.Status = OrderStatus.PaymentMismatched;
        }
        else
        {
            order.Status = OrderStatus.PaymentReceived;
        }

        await unit.Complete();

        var connectionId = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);

        if (!string.IsNullOrEmpty(connectionId))
        {
            // logger.LogInformation("Sending notification to user with connectionId: {ConnectionId}", connectionId);
            // await hubContext.Clients.Client(connectionId).SendAsync("OrderCompleteNotification", order.ToDto());

            try
            {
                await hubContext.Clients.Client(connectionId)
                    .SendAsync("OrderCompleteNotification", order.ToDto());
                logger.LogInformation("Notification sent successfully to ConnectionId: {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification via SignalR for ConnectionId: {ConnectionId}", connectionId);
            }

        }
        else
        {
            logger.LogWarning("No connection found for user: {BuyerEmail}", order.BuyerEmail);
        }

        // if (intent.Status == "succeeded")
        // {
        //     var spec = new OrderSpecification(intent.Id, true);

        //     var order = await unit.Repository<Order>().GetEntityWithSpec(spec)
        //         ?? throw new Exception("Order not found");

        //     if ((long)order.GetTotal() * 100 != intent.Amount)
        //     {
        //         order.Status = OrderStatus.PaymentMismatched;
        //     }
        //     else 
        //     {
        //         order.Status = OrderStatus.PaymentReceived;
        //     }

        //     await unit.Complete();

        //     //SignalR
        //     var connectionId = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);

        //     if (!string.IsNullOrEmpty(connectionId))
        //     {
        //         await hubContext.Clients.Client(connectionId)
        //             .SendAsync("OrderCompleteNotification", order.ToDto());
        //     }
        // }
    }

    private Event ConstructStripeEvent(string json)
    {
        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            logger.LogInformation($"Stripe-Signature: {stripeSignature}");
            logger.LogInformation("Attempting to construct stripe event. secret: {Secret}", _whSecret);

            // TEMPORARY: Skip signature validation in development
            // if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            // {
            //     logger.LogWarning("Skipping Stripe signature validation for development environment.");
            //     return JsonConvert.DeserializeObject<Event>(json);
            // }

            //validate signature in other environments
            if (string.IsNullOrEmpty(stripeSignature))
            {
                logger.LogError("Stripe-Signature header is missing");
                throw new StripeException("Stripe-Signature header");
            }
            logger.LogInformation($"Stripe-Signature header: {stripeSignature}");

            return EventUtility.ConstructEvent(json, stripeSignature,
                _whSecret, tolerance: 600);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Failed to construct stripe event. Verify the webhook signature and payload.");
            throw;
            //throw new StripeException("Invalid signature");
        }
    }
}
