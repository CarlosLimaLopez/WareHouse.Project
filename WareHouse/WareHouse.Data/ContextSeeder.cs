using Microsoft.Extensions.DependencyInjection;

namespace WareHouse.Data
{
    using Context;
    using Product;

    public class ContextSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<WareHouseContext>();

            if (context.Products.Any())
                return;
            
            var products = new List<Product>
                {
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Code = "12345678"
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Code = "ABCDEFGH"
                    }
                };

            context.Products.AddRange(products);

            context.SaveChanges();
        }
    }
}
