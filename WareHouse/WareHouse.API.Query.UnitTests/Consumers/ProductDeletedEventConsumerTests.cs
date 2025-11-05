using System.ComponentModel.DataAnnotations;
using Moq;

namespace WareHouse.Product
{
    public class ProductDeletedEventConsumerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<MassTransit.ConsumeContext<ProductDeletedEvent>> _contextMock;
        private readonly ProductDeletedEventConsumer _consumer;

        public ProductDeletedEventConsumerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _contextMock = new Mock<MassTransit.ConsumeContext<ProductDeletedEvent>>();
            _consumer = new ProductDeletedEventConsumer(_productServiceMock.Object);
        }

        [Fact]
        public async Task Consume_WhenEventIsValid_ShouldDeleteProduct()
        {
            // Arrange
            var @event = new ProductDeletedEvent(Guid.NewGuid());
            _contextMock.Setup(c => c.Message).Returns(@event);

            var product = new Product { Id = @event.Id, Code = "ABCDEFGH" };
            _productServiceMock
                .Setup(s => s.TryDeleteProduct(@event))
                .ReturnsAsync((product, []));

            // Act
            await _consumer.Consume(_contextMock.Object);

            // Assert
            _productServiceMock.Verify(s => s.TryDeleteProduct(@event), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenValidationFails_ShouldThrowProductValidationException()
        {
            // Arrange
            var @event = new ProductDeletedEvent(Guid.NewGuid());
            var product = new Product { Id = @event.Id, Code = "ABCDEFGH" };
            var errors = new List<ValidationResult>
            {
                new("Cannot remove a product when stock level is zero.", ["Stock"])
            };
            _contextMock.Setup(c => c.Message).Returns(@event);
            _productServiceMock
                .Setup(s => s.TryDeleteProduct(@event))
                .ReturnsAsync((product, errors));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProductValidationException>(
                () => _consumer.Consume(_contextMock.Object)
            );

            Assert.Equal(errors, ex.Errors);
            Assert.Contains("Cannot remove a product when stock level is zero.", ex.Message);
            _productServiceMock.Verify(s => s.TryDeleteProduct(@event), Times.Once);
        }
    }
}
