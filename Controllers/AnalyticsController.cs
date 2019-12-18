using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using MyCart.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyCart.Controllers
{
    [Route("api/[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly ICustomerService customerService;
        private readonly ICartService cartService;

        public AnalyticsController(ICustomerService customerService, ICartService cartService)
        {
            this.customerService = customerService;
            this.cartService = cartService;
        }

        [HttpGet("DeletedCartAfterReminder")]
        public async Task<IActionResult> DeletedCartAfterReminder(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var customers = await customerService.DeletedCartAfterReminder(cancellationToken);

                return Ok(customers);
            }

            return BadRequest();
        }

        [HttpGet("VisitedCartAfterReminder")]
        public async Task<IActionResult> VisitedCartAfterReminder(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var customers = await customerService.VisitedCartAfterReminder(cancellationToken);

                return Ok(customers);
            }

            return BadRequest();
        }

        [HttpGet("AbandonedCartsBeforeCheckout")]
        public async Task<IActionResult> AbandonedCartsBeforeCheckout(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var customers = await cartService.AbandonedCartsBeforeCheckout(cancellationToken);

                return Ok(customers);
            }

            return BadRequest();
        }

        [HttpGet("AbandonedCartsDuringCheckout")]
        public async Task<IActionResult> AbandonedCartsDuringCheckout(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var customers = await cartService.AbandonedCartsDuringCheckout(cancellationToken);

                return Ok(customers);
            }

            return BadRequest();
        }
    }
}
