using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCart.Models;
using MyCart.Services;

namespace MyCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartService cartService;

        public CartsController(ICartService cartService)
        {
            this.cartService = cartService;
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(Cart cart, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid && cart != null)
            {
                try
                {
                    await cartService.CreateAsync(cart, cancellationToken);

                    return Ok();
                }
                catch (Exception ex)
                {
                    return new UnprocessableEntityObjectResult(ex);
                }
            }

            return BadRequest(ModelState);
        }


        [HttpPost("AddProduct")]
        public async Task<IActionResult> Add(Product product, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid && product != null)
            {
                try
                {
                    await cartService.AddProductAsync(product, cancellationToken);

                    return Ok();
                }
                catch (Exception ex)
                {
                    return new UnprocessableEntityObjectResult(ex);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get(int cartId, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await cartService.GetAsync(cartId, cancellationToken));
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update(Cart cart, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid && cart != null)
            {
                try
                {
                    await cartService.UpdateAsync(cart, cancellationToken);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return new UnprocessableEntityObjectResult(ex);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> Delete(int cartId, CancellationToken cancellationToken)
        {
            try
            {               
                await cartService.DeleteAsync(cartId, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }
        }


        [HttpGet("List")]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await cartService.ListAsync(cancellationToken));
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }
        }


        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout(int cartId, CancellationToken cancellationToken)
        {
            try
            {
                await cartService.CheckoutAsync(cartId, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }
        }
    }
}
