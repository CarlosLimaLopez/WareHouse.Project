using System.ComponentModel.DataAnnotations;
using Moq;

namespace WareHouse.Product
{
    public class ProductCreatedEventConsumerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<MassTransit.ConsumeContext<ProductCreatedEvent>> _contextMock;

        private readonly ProductCreatedEventConsumer _consumer;

        public ProductCreatedEventConsumerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _contextMock = new Mock<MassTransit.ConsumeContext<ProductCreatedEvent>>();

            _consumer = new ProductCreatedEventConsumer(_productServiceMock.Object);
        }

        [Fact]
        public async Task Consume_WhenEventIsValid_ShouldInsertProduct()
        {
            // Arrange
            var @event = new ProductCreatedEvent(Guid.NewGuid(), "ABCDEFGH");
            _contextMock.Setup(c => c.Message).Returns(@event);

            var product = new Product { Id = @event.Id, Code = @event.Code };
            _productServiceMock
                .Setup(s => s.TryInsertProduct(@event))
                .ReturnsAsync((product, []));

            // Act
            await _consumer.Consume(_contextMock.Object);

            // Assert
            _productServiceMock.Verify(s => s.TryInsertProduct(@event), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenValidationFails_ShouldThrowProductValidationException()
        {
            // Arrange
            var @event = new ProductCreatedEvent(Guid.NewGuid(), "INVALID");
            var product = new Product { Id = @event.Id, Code = @event.Code };
            var errors = new List<ValidationResult>
            {
                new("Code must be exactly 8 characters.", ["Code"])
            };
            _contextMock.Setup(c => c.Message).Returns(@event);
            _productServiceMock
                .Setup(s => s.TryInsertProduct(@event))
                .ReturnsAsync((product, errors));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProductValidationException>(
                () => _consumer.Consume(_contextMock.Object)
            );

            Assert.Equal(errors, ex.Errors);
            Assert.Contains("Code must be exactly 8 characters.", ex.Message);
            _productServiceMock.Verify(s => s.TryInsertProduct(@event), Times.Once);
        }
    }
}