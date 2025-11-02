using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    using Repositories;

    public class ProductValidator
    {
        private readonly IProductRepository _productRepository;

        public ProductValidator(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ValidationResult>> ValidateInsertAsync(Product product)
        {
            var errors = new List<ValidationResult>();

            var existing = await _productRepository.GetProductByCode(product.Code);
            if (existing != null)
            {
                errors.Add(new ValidationResult(
                    $"A product with code '{product.Code}' already exists.",
                    new[] { nameof(product.Code) }
                ));
            }

            return errors;
        }
    }
}
