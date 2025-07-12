using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using StripePayment.Key;

namespace StripePayment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
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
        [HttpGet("payment-success")]
        public ContentResult PaymentSuccess()
        {
            var html = @"
<html>
<head>
    <meta charset='utf-8'>
    <title>Payment Success</title>
</head>
<body>
    <h1>Payment Success</h1>
    <p>You can close this tab and comeback to your application</p>
    <script>
        setTimeout(function() {
            window.close();
        }, 2000);
    </script>
</body>
</html>";
            return Content(html, "text/html");
        }



        public class CheckoutRequest
        {
            public string ItemName { get; set; }
            public decimal Price { get; set; }
        }

    }
}
