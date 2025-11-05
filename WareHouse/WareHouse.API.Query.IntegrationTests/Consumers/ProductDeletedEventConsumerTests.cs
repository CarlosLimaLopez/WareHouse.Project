using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.API.Query.IntegrationTests;

namespace WareHouse.Product
{
    using System.Linq;
    using Context;

    [Collection("WareHouseApiQueryTests")]
    public class ProductDeletedEventConsumerTests : IAsyncLifetime
    {
        private readonly WareHouseApiQueryFixture _fixture;

        public Task InitializeAsync() => Task.CompletedTask;
        public async Task DisposeAsync() => await _fixture.ClearContextAsync();

        public ProductDeletedEventConsumerTests(WareHouseApiQueryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Consume_WhenEventIsValid_ShouldDeleteProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productCode = "ABCDEFGH";
            var product = new Product { Id = productId, Code = productCode};
            await _fixture.SeedContextAsync([product]);

            var eventMessage = new ProductDeletedEvent(productId);

            var harness = _fixture.Api.Services.GetRequiredService<ITestHarness>();

            // Act
            await harness.Bus.Publish(eventMessage);

            // Assert
            while (!harness.Consumed.Select<ProductDeletedEvent>().Any(e => e.Context.Message.Id == productId))
                await Task.Delay(100);

            using var scope = _fixture.Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();
            var deletedProduct = context.Products.FirstOrDefault(p => p.Id == productId);

            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task Consume_WhenProductHasStock_ShouldNotDeleteProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productCode = "ABCDEFGH";
            var product = new Product { Id = productId, Code = productCode };
            product.UpdateStock(10);
            await _fixture.SeedContextAsync([product]);

            var eventMessage = new ProductDeletedEvent(productId);

            var harness = _fixture.Api.Services.GetRequiredService<ITestHarness>();

            // Act
            await harness.Bus.Publish(eventMessage);

            // Assert
            while (!harness.Consumed.Select<ProductDeletedEvent>().Any(e => e.Context.Message.Id == productId))
                await Task.Delay(100);

            using var scope = _fixture.Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();
            var deletedProduct = context.Products.FirstOrDefault(p => p.Id == productId);

            Assert.NotNull(deletedProduct);
        }
    }
}
