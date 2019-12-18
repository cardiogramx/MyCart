using System;
using MyCart.Data;

namespace MyCart.Models
{
    public static class AppleSeed
    {
        /// <summary>
        /// Initialize the inmemory database
        /// </summary>
        /// <param name="ctx"></param>
        public static void Initialize(DataContext ctx)
        {
            ctx.Customers.AddRange(
                new Customer
                {
                    Id = default,
                    Email = "james@cole.com",
                    Name = "James Cole",
                    Phone = "2347039197369"
                },
                new Customer
                {
                    Id = default,
                    Email = "jane@doe.com",
                    Name = "Jane Doe",
                    Phone = "2347039197369"
                });

            ctx.Carts.Add(new Cart
            {
                Id = default,
                CustomerId = 1,
                DateTimeAdded = DateTime.UtcNow,
                IsAbandoned = false,
                IsCheckedOut = false,
                IsDeleted = false,
                IsReminded = false,
                Products = new System.Collections.Generic.List<Product>
                {
                    new Product
                    {
                        Id = default,
                        Price = 499.99,
                        Name = "Tesla Cybertruck",
                        Quantity = 1
                    },
                    new Product
                    {   Id = default,
                        Price = 249.99,
                        Name = "Tesla Model X",
                        Quantity = 1
                    },
                    new Product
                    {
                        Id = default,
                        Price = 1999.99,
                        Name = "Toyota Camry",
                        Quantity = 1
                    }
                }
            });

            ctx.SaveChanges();
        }
    }
}
