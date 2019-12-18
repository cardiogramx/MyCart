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

        [HttpPost("Add")]
        public async Task<IActionResult> Add(Product product, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid && product != null)
            {
                try
                {
                    await cartService.AddAsync(product, cancellationToken);

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
                var cart = await cartService.GetAsync(cartId, cancellationToken);
                cart.LastVist = DateTime.UtcNow;
                await cartService.SaveChangesAsync(cancellationToken);
                return Ok(cart);
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
                var cart = await cartService.GetAsync(cartId, cancellationToken);
                cart.LastVist = DateTime.UtcNow;
                cart.IsDeleted = true;
                await cartService.SaveChangesAsync(cancellationToken);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }
        }
    }
}
