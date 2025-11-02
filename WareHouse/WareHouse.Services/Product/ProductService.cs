using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    using Repositories;
    using Context;

    public interface IProductService
    {
        /// <summary>
        /// Gets a product by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>The product if found; otherwise, null.</returns>
        Task<Product?> GetProduct(Guid id);

        /// <summary>
        /// Gets the list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        Task<List<Product>> GetProducts();

        /// <summary>
        /// Attempts to insert a new product after validating business rules.
        /// </summary>
        /// <param name="product">Product to insert.</param>
        /// <returns>
        /// Tuple containing the product and a collection of validation errors.
        /// If validation fails, errors will be populated.
        /// </returns>
        Task<(Product product, IEnumerable<ValidationResult> errors)> TryInsertProduct(Product product);

        /// <summary>
        /// Attempts to delete a product by its unique identifier after validating business rules.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>
        /// Tuple containing the product and a collection of validation errors.
        /// If the product does not exist, returns (null, empty).
        /// If validation fails, errors will be populated.
        /// </returns>
        Task<(Product? product, IEnumerable<ValidationResult> errors)> TryDeleteProduct(Guid id);

        /// <summary>
        /// Attempts to increment the stock of a product by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>
        /// Tuple containing the updated product and a collection of validation errors.
        /// If the product does not exist, returns (null, empty).
        /// </returns>
        Task<(Product? product, IEnumerable<ValidationResult> errors)> TryAddStock(Guid id);

        /// <summary>
        /// Attempts to decrement the stock of a product by its unique identifier after validating business rules.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>
        /// Tuple containing the updated product and a collection of validation errors.
        /// If the product does not exist, returns (null, empty).
        /// If validation fails (e.g., stock is zero), errors will be populated.
        /// </returns>
        Task<(Product? product, IEnumerable<ValidationResult> errors)> TryRemoveStock(Guid id);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductValidator _productValidator;
        private readonly IUnitOfWork<WareHouseContext> _unitOfWork;

        public ProductService(
            IProductRepository productRepository,
            ProductValidator productValidator,
            IUnitOfWork<WareHouseContext> unitOfWork
            )
        {
            _productRepository = productRepository;
            _productValidator = productValidator;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public Task<Product?> GetProduct(Guid id) => _productRepository.GetProductById(id);

        /// <inheritdoc/>
        public Task<List<Product>> GetProducts() => _productRepository.GetProducts();

        /// <inheritdoc/>
        public async Task<(Product product, IEnumerable<ValidationResult> errors)> TryInsertProduct(Product product)
        {
            var errors = await _productValidator.ValidateInsertAsync(product);
            if (errors.Any())
                return (product, errors);

            _productRepository.Add(product);

            await _unitOfWork.CompleteAsync();

            return (product, []);
        }

        /// <inheritdoc/>
        public async Task<(Product? product, IEnumerable<ValidationResult> errors)> TryDeleteProduct(Guid id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
                return (null, []);

            var validationResults = product.ValidateRemove();
            if (validationResults.Any())
                return (product, validationResults);

            _productRepository.Remove(product);

            await _unitOfWork.CompleteAsync();

            return (product, []);
        }

        /// <inheritdoc/>
        public async Task<(Product? product, IEnumerable<ValidationResult> errors)> TryAddStock(Guid id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
                return (null, []);

            product.AddStock();

            await _unitOfWork.CompleteAsync();

            return (product, []);
        }

        /// <inheritdoc/>
        public async Task<(Product? product, IEnumerable<ValidationResult> errors)> TryRemoveStock(Guid id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
                return (null, []);

            var validationResults = product.ValidateRemoveStock();
            if (validationResults.Any())
                return (product, validationResults);

            product.RemoveStock();

            await _unitOfWork.CompleteAsync();

            return (product, []);
        }
    }
}
