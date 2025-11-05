using System.ComponentModel.DataAnnotations;
using Moq;

namespace WareHouse.Product
{
    public class ProductUpdatedEventConsumerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<MassTransit.ConsumeContext<ProductUpdatedEvent>> _contextMock;
        private readonly ProductUpdatedEventConsumer _consumer;

        public ProductUpdatedEventConsumerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _contextMock = new Mock<MassTransit.ConsumeContext<ProductUpdatedEvent>>();
            _consumer = new ProductUpdatedEventConsumer(_productServiceMock.Object);
        }

        [Fact]
        public async Task Consume_WhenEventIsValid_ShouldUpdateProduct()
        {
            // Arrange
            var @event = new ProductUpdatedEvent(Guid.NewGuid(), 10);
            _contextMock.Setup(c => c.Message).Returns(@event);

            var product = new Product { Id = @event.Id, Code = "ABCDEFGH"};
            product.UpdateStock(@event.Stock);
            _productServiceMock
                .Setup(s => s.TryUpdateProduct(@event))
                .ReturnsAsync((product, []));

            // Act
            await _consumer.Consume(_contextMock.Object);

            // Assert
            _productServiceMock.Verify(s => s.TryUpdateProduct(@event), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenValidationFails_ShouldThrowProductValidationException()
        {
            // Arrange
            var @event = new ProductUpdatedEvent(Guid.NewGuid(), -1);
            var product = new Product { Id = @event.Id, Code = "ABCDEFGH"};
            product.UpdateStock(@event.Stock);
            var errors = new List<ValidationResult>
            {
                new("Stock cannot be negative.", ["Stock"])
            };
            _contextMock.Setup(c => c.Message).Returns(@event);
            _productServiceMock
                .Setup(s => s.TryUpdateProduct(@event))
                .ReturnsAsync((product, errors));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProductValidationException>(
                () => _consumer.Consume(_contextMock.Object)
            );

            Assert.Equal(errors, ex.Errors);
            Assert.Contains("Stock cannot be negative.", ex.Message);
            _productServiceMock.Verify(s => s.TryUpdateProduct(@event), Times.Once);
        }
    }
}
