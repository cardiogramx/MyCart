using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using MyCart.Data;
using MyCart.Models;

namespace MyCart.Services
{
    public interface ICartService
    {
        Task CreateAsync(Cart cart, CancellationToken cancellationToken = default);

        Task AddProductAsync(Product product, CancellationToken cancellationToken = default);

        Task CheckoutAsync(int cartId, CancellationToken cancellationToken = default);

        Task<Cart> GetAsync(int cartId, CancellationToken cancellationToken = default);

        Task DeleteAsync(int cartId, CancellationToken cancellationToken = default);

        Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<List<Cart>> ListAsync(CancellationToken cancellationToken = default);

        Task<List<Cart>> AbandonedCartsBeforeCheckout(CancellationToken cancellationToken = default);

        Task<List<Cart>> AbandonedCartsDuringCheckout(CancellationToken cancellationToken = default);

        Task<Cart> GetAbandonedCartAsync(int customerId, CancellationToken cancellationToken = default);

    }

    public class CartService : ICartService
    {
        private readonly DataContext ctx;
        private readonly ICacheService redis;
        private readonly IEmailService emailService;


        public CartService(DataContext dataContext, ICacheService cacheService, IEmailService emailService)
        {
            ctx = dataContext;
            this.redis = cacheService;
            this.emailService = emailService;

            //listens to when a cartid in redis is expiring
            this.redis.OnExpiring += Redis_OnExpiring;
        }

        //executes each time a cartid is expiring in redis
        private void Redis_OnExpiring(object sender, EventArgs e)
        {
            //get cart detail from db including customer data
            var cart = ctx.Carts.Include(c => c.Customer)
              .SingleOrDefault(c => c.Id == (int)sender && c.IsAbandoned);
            
            //send notification email to customer
            var isEmailSent = emailService.SendAsync(new SendGridEmailMessage
            {
                Address = cart.Customer.Email,
                Subject = "Don't Break My Cart",
                PlainTextContent = $"You have uncompleted orders"
            }).Result;

            if (isEmailSent)
            {
                cart.IsReminded = true;
                cart.DateTimeReminded = DateTime.UtcNow;
                ctx.SaveChanges();
            }
          
        }


        public async Task CreateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            var loggedInCustomerId = 1; //get this from session, claims etc.

            //get the cart for the logged in customer
            var data = await ctx.Carts
                    .SingleOrDefaultAsync(c => c.CustomerId == loggedInCustomerId
                    && !c.IsDeleted && !c.IsCheckedOut, cancellationToken);

            if (data != null)
            {
                return;
            }

            cart.CustomerId = loggedInCustomerId;
            cart.IsAbandoned = true;
            cart.DateTimeAbandoned = null;

            cart.IsCheckedOut = false;
            cart.DateTimeCheckedOut = null;

            cart.IsDeleted = false;
            cart.DateTimeDeleted = null;

            cart.IsReminded = false;
            cart.DateTimeReminded = null;

            await ctx.Carts.AddAsync(cart, cancellationToken);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            var loggedInCustomerId = 1; //get this from session, claims etc.

            //get the cart for the logged in customer
            var cart = await ctx.Carts.Include(c => c.Products)
                    .SingleOrDefaultAsync(c => c.CustomerId == loggedInCustomerId
                    && !c.IsDeleted && !c.IsCheckedOut, cancellationToken);

            //if cart has items
            if (cart.Products.Any())
            {
                ////if item already in cart, increment the quantity
                //if (cart.Products.Select(c => c.Id ).Contains(product.Id))
                //{
                //    cart.Products.SingleOrDefault(c => c.Id == product.Id).Quantity += 1;
                //}
            }

            cart.Products.Add(product);

            cart.IsAbandoned = true;
            cart.DateTimeAbandoned = DateTime.UtcNow;

            //save the changes to db
            ctx.Entry(cart).State = EntityState.Modified;
            await ctx.SaveChangesAsync(cancellationToken);

            //log cart to redis to monitor expiry
            await redis.SetAsync(cart.Id.ToString(), cart.CustomerId, cancellationToken);
        }


        public async Task CheckoutAsync(int cartId, CancellationToken cancellationToken = default)
        {
            //in production, get cart using the loggedin customerId instead of cartId for security reasons
            var cart = await ctx.Carts.Include(c => c.Products)
                   .SingleOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted, cancellationToken);

            //if no abandoned cart, abort checkout process
            if (cart == null)
            {
                return;
            }

            //update the checkout status and date
            cart.IsCheckedOut = true;
            cart.DateTimeCheckedOut = DateTime.UtcNow;
            cart.IsAbandoned = false;
            cart.DateTimeAbandoned = null;

            //save the changes to db
            ctx.Entry(cart).State = EntityState.Modified;
            await ctx.SaveChangesAsync(cancellationToken);

            //log cart to redis to monitor expiry
            await redis.SetAsync(cart.Id.ToString(), cart.CustomerId, cancellationToken);
        }


        public async Task<Cart> GetAsync(int cartId, CancellationToken cancellationToken = default)
        {
            var cart = await ctx.Carts.SingleOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted, cancellationToken);

            cart.LastVist = DateTime.UtcNow;
            cart.IsAbandoned = true;
            cart.DateTimeAbandoned = cart.LastVist;

            //save update to db
            ctx.Entry(cart).State = EntityState.Modified;
            await ctx.SaveChangesAsync(cancellationToken);

            //return cart
            return cart;
        }


        public async Task DeleteAsync(int cartId, CancellationToken cancellationToken = default)
        {
            //in production, get cart using the loggedin customerId instead of cartId for security reasons
            var cart = await ctx.Carts.Include(c => c.Products)
                   .SingleOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            cart.IsDeleted = true;
            cart.DateTimeDeleted = DateTime.UtcNow;

            //save the changes to db
            ctx.Entry(cart).State = EntityState.Modified;
            await ctx.SaveChangesAsync(cancellationToken);

            //remove cart entry from redis
            await redis.RemoveAsync(cart.Id.ToString(), cancellationToken);
        }


        public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            //fetch uncompleted cart from the db
            var data = await ctx.Carts.SingleOrDefaultAsync(c => c.Id == cart.Id && c.IsAbandoned, cancellationToken);

            //update modified products
            foreach (var dbItem in data.Products)
            {
                foreach (var product in cart.Products)
                {
                    if (dbItem.Id == product.Id)
                    {
                        dbItem.Price = (Math.Abs(dbItem.Price - product.Price) > double.Epsilon) ? product.Price : dbItem.Price;
                        dbItem.Quantity = (dbItem.Quantity != product.Quantity) ? product.Quantity : dbItem.Quantity;
                    }
                }
            }

            //add new products
            foreach (var product in cart.Products)
            {
                if (!data.Products.Contains(product))
                {
                    data.Products.Add(product);
                }
            }

            //remove deleted products
            foreach (var dbItem in data.Products)
            {
                if (!cart.Products.Contains(dbItem))
                {
                    data.Products.Remove(dbItem);
                }
            }

            ctx.Entry(data).State = EntityState.Modified;
            await ctx.SaveChangesAsync(cancellationToken);

            //log cart to redis to monitor expiry
            await redis.SetAsync(cart.Id.ToString(), cart.CustomerId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Cart>> AbandonedCartsBeforeCheckout(CancellationToken cancellationToken = default)
        {
            var data = await ctx.Carts
                .Where(c => c.IsAbandoned == true && c.DateTimeAbandoned.HasValue && c.IsCheckedOut == false)
                .Include(c => c.Customer)
                .ToListAsync(cancellationToken);

            return data;
        }

        public async Task<List<Cart>> AbandonedCartsDuringCheckout(CancellationToken cancellationToken = default)
        {
            var data = await ctx.Carts
                .Where(c => c.IsAbandoned == true && c.DateTimeAbandoned.HasValue && c.IsCheckedOut == true)
                .Include(c => c.Customer)
                .ToListAsync(cancellationToken);

            return data;
        }

        public async Task<List<Cart>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await ctx.Carts
                .Include(c => c.Customer)
                .Include(c => c.Products)
                .ToListAsync(cancellationToken);
        }

        public async Task<Cart> GetAbandonedCartAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var cart = await ctx.Carts.SingleOrDefaultAsync(c => c.CustomerId == customerId && !c.IsDeleted && c.IsAbandoned, cancellationToken);

            cart.LastVist = DateTime.UtcNow;

            //save update to db
            ctx.Entry(cart).State = EntityState.Modified;
            await ctx.SaveChangesAsync(cancellationToken);

            //return cart
            return cart;
        }
    }
}
