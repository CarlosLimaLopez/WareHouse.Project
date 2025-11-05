using MassTransit;

namespace WareHouse.Product
{
    public class ProductDeletedEventConsumer : IConsumer<ProductDeletedEvent>
    {
        private readonly IProductService _productService;

        public ProductDeletedEventConsumer(IProductService productService)
        {
            _productService = productService;
        }

        public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
        {
            var message = context.Message;

            var (product, errors) = await _productService.TryDeleteProduct(message);

            if (errors.Any())
                throw new ProductValidationException(errors);
        }
    }
}
