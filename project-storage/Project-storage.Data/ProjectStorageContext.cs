using Microsoft.EntityFrameworkCore;
using Project_storage.Data.Models;
using System;

namespace Project_storage.Data
{
    public class ProjectStorageContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionProduct> TransactionProducts { get; set; }

        public ProjectStorageContext(DbContextOptions<ProjectStorageContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
