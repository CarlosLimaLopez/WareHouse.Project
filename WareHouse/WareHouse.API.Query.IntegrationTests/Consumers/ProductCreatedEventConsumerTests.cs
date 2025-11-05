using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.API.Query.IntegrationTests;

namespace WareHouse.Product
{
    using System.Linq;
    using Context;

    [Collection("WareHouseApiQueryTests")]
    public class ProductCreatedEventConsumerTests : IAsyncLifetime
    {
        private readonly WareHouseApiQueryFixture _fixture;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _fixture.ClearContextAsync();
        }

        public ProductCreatedEventConsumerTests(WareHouseApiQueryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Consume_WhenEventIsValid_ShouldCreateProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productCode = "ABCDEFGH";
            var eventMessage = new ProductCreatedEvent(productId, productCode);

            var bus = _fixture.Api.Services.GetRequiredService<IBus>();
            var harness = _fixture.Api.Services.GetRequiredService<ITestHarness>();

            // Act
            await harness.Bus.Publish(eventMessage);

            // Assert
            while (!harness.Consumed.Select<ProductCreatedEvent>().Any(e => e.Context.Message.Id == productId))
                await Task.Delay(100);

            using var scope = _fixture.Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();
            var product = context.Products.FirstOrDefault(p => p.Id == productId);
                        
            Assert.NotNull(product);
            Assert.Equal(productCode, product.Code);
        }

        [Fact]
        public async Task Consume_WhenEventIsInvalid_ShouldNotCreateProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var invalidCode = "123";
            var eventMessage = new ProductCreatedEvent(productId, invalidCode);

            var harness = _fixture.Api.Services.GetRequiredService<ITestHarness>();
            
            // Act
            await harness.Bus.Publish(eventMessage);

            // Assert
            while (!harness.Consumed.Select<ProductCreatedEvent>().Any(e => e.Context.Message.Id == productId))
                await Task.Delay(100);

            using var scope = _fixture.Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();
            var product = context.Products.FirstOrDefault(p => p.Id == productId);

            Assert.Null(product);
        }
    }
}
