using Microsoft.EntityFrameworkCore;

namespace WareHouse.Repositories
{
    using Context;
    using Product;

    public interface IProductRepository
    {
        public void Add(Product product);
        public Task<Product?> GetProductById(Guid id);
        public Task<Product?> GetProductByIdAsNoTracking(Guid id);
        public Task<Product?> GetProductByCode(string code);
        public Task<List<Product>> GetProducts();
        public Task<List<Product>> GetProductsAsNoTracking();
        public void Remove(Product product);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly WareHouseContext _wareHouseContext;

        public ProductRepository(WareHouseContext wareHouseContext)
        {
            _wareHouseContext = wareHouseContext;
        }

        public void Add(Product product) 
            => _wareHouseContext.Products.Add(product);

        public Task<Product?> GetProductById(Guid id)
            => _wareHouseContext.Products.FirstOrDefaultAsync(p => p.Id == id);

        public Task<Product?> GetProductByIdAsNoTracking(Guid id)
            => _wareHouseContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        public Task<Product?> GetProductByCode(string code)
            => _wareHouseContext.Products.FirstOrDefaultAsync(p => p.Code == code);

        public Task<List<Product>> GetProducts()
            => _wareHouseContext.Products.ToListAsync();

        public Task<List<Product>> GetProductsAsNoTracking()
            => _wareHouseContext.Products.AsNoTracking().ToListAsync();

        public void Remove(Product Product)
            => _wareHouseContext.Products.Remove(Product);
    }
}
