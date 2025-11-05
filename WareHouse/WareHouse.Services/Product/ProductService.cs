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
        Task<(Product product, IEnumerable<ValidationResult> errors)> TryInsertProduct(Product product, bool publishEvent = true);

        /// <summary>
        /// Attempts to insert a new product from a ProductCreatedEvent after validating business rules.
        /// </summary>
        /// <param name="productCreatedEvent">Event containing product data to be inserted.</param>
        /// <returns>
        /// Tuple containing the product and a collection of validation errors.
        /// If validation fails, errors will be populated.
        /// </returns>
        Task<(Product product, IEnumerable<ValidationResult> errors)> TryInsertProduct(ProductCreatedEvent productCreatedEvent);

        /// <summary>
        /// Attempts to delete a product by its unique identifier after validating business rules.
        /// </summary>
        /// <param name="id">Unique identifier of the product.</param>
        /// <returns>
        /// Tuple containing the product and a collection of validation errors.
        /// If the product does not exist, returns (null, empty).
        /// If validation fails, errors will be populated.
        /// </returns>
        Task<(Product? product, IEnumerable<ValidationResult> errors)> TryDeleteProduct(Guid id, bool publishEvent = true);

        /// <summary>
        /// Attempts to delete a product from a ProductDeletedEvent after validating business rules.
        /// </summary>
        /// <param name="productDeletedEvent">Event containing product data to be deleted</param>
        /// <returns>
        /// Tuple containing the product and a collection of validation errors.
        /// If the product does not exist, returns (null, empty).
        /// If validation fails, errors will be populated.
        /// </returns>
        Task<(Product? product, IEnumerable<ValidationResult> errors)> TryDeleteProduct(ProductDeletedEvent productDeletedEvent);

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

        /// <summary>
        /// Attempts to update a product's stock from a ProductUpdatedEvent.
        /// </summary>
        /// <param name="productUpdatedEvent">  
        /// Event containing product data to be updated
        /// </param>
        /// <returns>
        /// Tuple containing the updated product and a collection of validation errors.
        /// </returns>
        Task<(Product? product, IEnumerable<ValidationResult> errors)> TryUpdateProduct(ProductUpdatedEvent productUpdatedEvent);
    }

    public class ProductService : IProductService
    {
        private readonly MassTransit.IPublishEndpoint _publishEndpoint;
        private readonly IProductRepository _productRepository;
        private readonly ProductValidator _productValidator;
        private readonly IUnitOfWork<WareHouseContext> _unitOfWork;
        
        public ProductService(
            MassTransit.IPublishEndpoint publishEndpoint,
            IProductRepository productRepository,
            ProductValidator productValidator,
            IUnitOfWork<WareHouseContext> unitOfWork
            )
        {
            _publishEndpoint = publishEndpoint;
            _productRepository = productRepository;
            _productValidator = productValidator;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public Task<Product?> GetProduct(Guid id) => _productRepository.GetProductByIdAsNoTracking(id);

        /// <inheritdoc/>
        public Task<List<Product>> GetProducts() => _productRepository.GetProductsAsNoTracking();

        /// <inheritdoc/>
        public async Task<(Product product, IEnumerable<ValidationResult> errors)> TryInsertProduct(Product product, bool publishEvent = true)
        {
            var errors = await _productValidator.ValidateInsertAsync(product);
            if (errors.Any())
                return (product, errors);

            _productRepository.Add(product);

            await _unitOfWork.CompleteAsync();

            if (publishEvent)
            {
                await _publishEndpoint.Publish(
                    new ProductCreatedEvent(product.Id, product.Code)
                );
            }

            return (product, []);
        }

        /// <inheritdoc/>
        public async Task<(Product product, IEnumerable<ValidationResult> errors)> TryInsertProduct(ProductCreatedEvent productCreatedEvent)
        {
            var product = productCreatedEvent.ToProduct();

            var errors = product.ValidateAttributes();
            if (errors.Any())
                return (product, errors);

            return await TryInsertProduct(product, publishEvent: false);
        }

        /// <inheritdoc/>
        public async Task<(Product? product, IEnumerable<ValidationResult> errors)> TryDeleteProduct(Guid id, bool publishEvent = true)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
                return (null, []);

            var validationResults = product.ValidateRemove();
            if (validationResults.Any())
                return (product, validationResults);

            _productRepository.Remove(product);

            await _unitOfWork.CompleteAsync();

            if (publishEvent)
            {
                await _publishEndpoint.Publish(
                    new ProductDeletedEvent(product.Id)
                );
            }

            return (product, []);
        }

        /// <inheritdoc/>
        public Task<(Product? product, IEnumerable<ValidationResult> errors)> TryDeleteProduct(ProductDeletedEvent productDeletedEvent)
            => TryDeleteProduct(productDeletedEvent.Id, publishEvent: false);
        
        /// <inheritdoc/>
        public async Task<(Product? product, IEnumerable<ValidationResult> errors)> TryAddStock(Guid id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
                return (null, []);

            product.AddStock();

            await _unitOfWork.CompleteAsync();

            await _publishEndpoint.Publish(
                new ProductUpdatedEvent(product.Id, product.Stock)
            );

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

            await _publishEndpoint.Publish(
                new ProductUpdatedEvent(product.Id, product.Stock)
            );

            return (product, []);
        }

        /// <inheritdoc/>
        public async Task<(Product? product, IEnumerable<ValidationResult> errors)> TryUpdateProduct(ProductUpdatedEvent productUpdatedEvent)
        {
            var product = await _productRepository.GetProductById(productUpdatedEvent.Id);
            if (product == null)
                return (null, []);

            var validationResults = product.ValidateUpdateStock(productUpdatedEvent.Stock);
            if (validationResults.Any())
                return (product, validationResults);

            product.UpdateStock(productUpdatedEvent.Stock);

            await _unitOfWork.CompleteAsync();

            return (product, []);
        }
    }
}
