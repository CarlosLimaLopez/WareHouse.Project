using MassTransit;

namespace WareHouse.Product
{
    public class ProductUpdatedEventConsumer : IConsumer<ProductUpdatedEvent>
    {
        private readonly IProductService _productService;

        public ProductUpdatedEventConsumer(IProductService productService)
        {
            _productService = productService;
        }

        public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            var message = context.Message;

            var (product, errors) = await _productService.TryUpdateProduct(message);

            if (errors.Any())
                throw new ProductValidationException(errors);
        }
    }
}
