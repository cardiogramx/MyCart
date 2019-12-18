using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

using System.Threading;
using MyCart.Models;
using MyCart.Data;

namespace MyCart.Services
{
    public interface ICustomerService
    {
        Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);

        Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

        Task<Customer> GetAsync(string customerId, CancellationToken cancellationToken = default);

        Task<List<Customer>> ListAsync(CancellationToken cancellationToken = default);

        Task<List<Customer>> VisitedCartAfterReminder(CancellationToken cancellationToken = default);

        Task<List<Customer>> DeletedCartAfterReminder(CancellationToken cancellationToken = default);

    }

    public class CustomerService : ICustomerService
    {
        private readonly DataContext ctx;

        public CustomerService(DataContext dataContext)
        {
            this.ctx = dataContext;
        }

        public async Task<Customer> GetAsync(string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await ctx.Customers
                .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

                return data;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            try
            {
                await ctx.Customers.AddAsync(customer);
                await ctx.SaveChangesAsync(cancellationToken);

                return customer;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await ctx.Customers.SingleOrDefaultAsync(c => c.Id == customer.Id);

                data.Email = !string.IsNullOrWhiteSpace(customer.Email) ? customer.Email : data.Email;
                data.Name = !string.IsNullOrWhiteSpace(customer.Name) ? customer.Name : data.Name;
                data.Phone = !string.IsNullOrWhiteSpace(customer.Phone) ? customer.Phone : data.Phone;

                ctx.Entry(data).State = EntityState.Modified;
                await ctx.SaveChangesAsync(cancellationToken);

                return data;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Customer>> ListAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await ctx.Customers.ToListAsync(cancellationToken);
                return data;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Customer>> VisitedCartAfterReminder(CancellationToken cancellationToken = default)
        {
            var data = await ctx.Carts
                .Where(c => c.IsReminded && c.LastVist >= c.DateTimeReminded)
                .Select(c => c.Customer).ToListAsync(cancellationToken);

            return data;
        }

        public async Task<List<Customer>> DeletedCartAfterReminder(CancellationToken cancellationToken = default)
        {
            var data = await ctx.Carts
                .Where(c => c.IsReminded && c.IsDeleted && c.DateTimeDeleted >= c.DateTimeReminded)
                .Select(c => c.Customer).ToListAsync(cancellationToken);

            return data;
        }
    }
}