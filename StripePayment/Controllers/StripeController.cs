using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using StripePayment.Key;

namespace StripePayment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly FakeOrderService _orderService = new FakeOrderService();

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] CheckoutRequest request)
        {
            StripeConfiguration.ApiKey = SecretKeyManager.GetKey("StripeSecretKey");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(request.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = request.ItemName,
                    },
                },
                Quantity = 1,
            },
        },
                Mode = "payment",
                SuccessUrl = "https://localhost:7155/api/stripe/payment-success",
                CancelUrl = "https://localhost:7155/api/stripe/payment-cancel",
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new
            {
                sessionUrl = session.Url,
                sessionId = session.Id
            });
        }


        [HttpGet("check-payment-status/{sessionId}")]
        public IActionResult CheckPaymentStatus(string sessionId)
        {
            StripeConfiguration.ApiKey = SecretKeyManager.GetKey("StripeSecretKey");

            var service = new SessionService();
            var session = service.Get(sessionId);

            if (session.PaymentStatus == "paid")
            {
                return Ok("paid");
            }

            return Ok("pending");
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    SecretKeyManager.GetKey("StripeWebhookSecret")
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session != null)
                    {
                        Console.WriteLine($"✅ Payment success for session {session.Id}, amount: {session.AmountTotal / 100.0}");

                        // 🔁 Call your own callback endpoint
                        using var client = new HttpClient();
                        var callback = new OrderCallbackModel
                        {
                            OrderId = session.Id,
                            Amount = (double)(session.AmountTotal / 100.0),
                            PaymentStatus = "success"
                        };

                        var jsonData = System.Text.Json.JsonSerializer.Serialize(callback);
                        var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                        await client.PostAsync("https://localhost:7155/api/stripe/stripe-callback", content);
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine($"❌ Webhook error: {e.Message}");
                return BadRequest();
            }
        }


        [HttpPost("stripe-callback")]
        public IActionResult StripeCallback([FromBody] OrderCallbackModel model)
        {
            // Example model:
            // { "orderId": "abc", "amount": 50, "paymentStatus": "success" }

            // ✅ Insert order into your database
            _orderService.CreateOrder(model.OrderId, model.Amount, model.PaymentStatus);

            Console.WriteLine($"✅ Order {model.OrderId} saved after Stripe payment.");
            return Ok(new { message = "Order recorded successfully" });
        }



        public class CheckoutRequest
        {
            public string ItemName { get; set; }
            public decimal Price { get; set; }
        }
        
        public class OrderCallbackModel
        {
            public string OrderId { get; set; }
            public double Amount { get; set; }
            public string PaymentStatus { get; set; }
        }
        public class FakeOrderService
        {
            public void CreateOrder(string orderId, double amount, string paymentStatus)
            {
                Console.WriteLine($"💾 [FakeOrderService] Order saved: {orderId}, ${amount}, Status: {paymentStatus}");
            }
        }

    }


}
