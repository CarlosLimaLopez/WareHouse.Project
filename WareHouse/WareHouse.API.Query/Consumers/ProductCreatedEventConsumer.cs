using MassTransit;

namespace WareHouse.Product
{
    public class ProductCreatedEventConsumer : IConsumer<ProductCreatedEvent>
    {
        private readonly IProductService _productService;

        public ProductCreatedEventConsumer(IProductService productService)
        {
            _productService = productService;
        }

        public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            var message = context.Message;

            var (product, errors) = await _productService.TryInsertProduct(message);

            if (errors.Any())
                throw new ProductValidationException(errors);
        }
    }
}
