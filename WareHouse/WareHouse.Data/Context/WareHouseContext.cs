using Microsoft.EntityFrameworkCore;

namespace WareHouse.Context
{
    using Product;

    public class WareHouseContext : DbContext
    {
        public WareHouseContext(DbContextOptions<WareHouseContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(product =>
            {
                product.HasIndex(e => e.Code).IsUnique();
            });
        }
    }
}