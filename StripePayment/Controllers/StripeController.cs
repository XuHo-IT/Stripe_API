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
                SuccessUrl = "https://yourapp.com/success",
                CancelUrl = "https://yourapp.com/cancel",
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { sessionUrl = session.Url });
        }
    }

    public class CheckoutRequest
    {
        public string ItemName { get; set; }
        public decimal Price { get; set; }
    }

}
