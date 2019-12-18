using Microsoft.EntityFrameworkCore;
using MyCart.Models;

namespace MyCart.Data
{
    public class DataContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Cart> Carts { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
            //modelBuilder.Entity<SVT>().HasIndex(c => new { c.MACAddress }).IsUnique(true);


            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
