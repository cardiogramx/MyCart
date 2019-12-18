using System;
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
    public class CustomersController : Controller
    {
        private readonly ICustomerService customerService;
        private readonly ICartService cartService;
        private readonly IEmailService emailService;

        public CustomersController(ICustomerService customerService, ICartService cartService, IEmailService emailService)
        {
            this.customerService = customerService;
            this.cartService = cartService;
            this.emailService = emailService;
        }


        [HttpGet("Get")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int customerId, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var customer = await customerService.GetAsync(customerId, cancellationToken);

                return Ok(customer);
            }

            return BadRequest(customerId);
        }

        [HttpPost("Add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Add(Customer customer, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                await customerService.AddAsync(customer, cancellationToken);

                return Ok();
            }

            return BadRequest(customer);
        }

        [HttpPost("Update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Customer customer, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                await customerService.UpdateAsync(customer, cancellationToken);

                return Ok();
            }

            return BadRequest(customer);
        }

        [HttpGet("List")]
        [ProducesResponseType(typeof(List<Customer>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var customers = await customerService.ListAsync(cancellationToken);

                return Ok(customers);
            }

            return BadRequest();
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(Customer customer, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                //assuming email and password matches the ones on the db
                if (true)
                {
                    var abandonedCart = await cartService.GetAbandonedCartAsync(customer.Id, cancellationToken);

                    //if user visit is less than 21 days
                    if (abandonedCart != null && (DateTime.UtcNow - abandonedCart.LastVist).Value.Days < 21)
                    {
                        await emailService.SendAsync(new SendGridEmailMessage
                        {
                            Address = abandonedCart.Customer.Email,
                            Subject = "Don't Break My Cart",
                            PlainTextContent = $"You have uncompleted orders"
                        });
                    }

                    return Ok();
                }
            }

            return BadRequest(customer);
        }
    }
}
