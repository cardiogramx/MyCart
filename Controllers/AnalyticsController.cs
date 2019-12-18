using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCart.Models;
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
        [ProducesResponseType(typeof(List<Customer>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(List<Customer>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(List<Cart>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AbandonedCartsBeforeCheckout(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var carts = await cartService.AbandonedCartsBeforeCheckout(cancellationToken);

                return Ok(carts);
            }

            return BadRequest();
        }


        [HttpGet("AbandonedCartsDuringCheckout")]
        [ProducesResponseType(typeof(List<Cart>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AbandonedCartsDuringCheckout(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var carts = await cartService.AbandonedCartsDuringCheckout(cancellationToken);

                return Ok(carts);
            }

            return BadRequest();
        }
    }
}
