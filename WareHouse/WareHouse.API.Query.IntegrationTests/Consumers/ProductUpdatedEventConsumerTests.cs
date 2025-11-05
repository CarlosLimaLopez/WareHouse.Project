using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.API.Query.IntegrationTests;

namespace WareHouse.Product
{
    using System.Linq;
    using Context;

    [Collection("WareHouseApiQueryTests")]
    public class ProductUpdatedEventConsumerTests : IAsyncLifetime
    {
        private readonly WareHouseApiQueryFixture _fixture;

        public Task InitializeAsync() => Task.CompletedTask;
        public async Task DisposeAsync() => await _fixture.ClearContextAsync();

        public ProductUpdatedEventConsumerTests(WareHouseApiQueryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Consume_WhenEventIsValid_ShouldUpdateProduct()
        {
            //Arrange
            var originalStock = 10;
            var newStock = 20;
            var originalProduct = new Product { Id = Guid.NewGuid(), Code = "ABCDEFGH" };
            originalProduct.UpdateStock(originalStock);
            await _fixture.SeedContextAsync([originalProduct]);

            var eventMessage = new ProductUpdatedEvent(originalProduct.Id, newStock);

            var harness = _fixture.Api.Services.GetRequiredService<ITestHarness>();

            // Act
            await harness.Bus.Publish(eventMessage);

            // Assert
            while (!harness.Consumed.Select<ProductUpdatedEvent>().Any(e => e.Context.Message.Id == originalProduct.Id))
                await Task.Delay(100);

            using var scope = _fixture.Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();
            var product = context.Products.FirstOrDefault(p => p.Id == originalProduct.Id);

            Assert.NotNull(product);
            Assert.Equal(newStock, product.Stock);
        }

        [Fact]
        public async Task Consume_WhenEventIsInvalid_ShouldNotUpdateProduct()
        {
            // Arrange
            var originalStock = 10;
            var newStock = -20;
            var originalProduct = new Product { Id = Guid.NewGuid(), Code = "aserthnj"};
            originalProduct.UpdateStock(originalStock);
            await _fixture.SeedContextAsync([originalProduct]);

            var eventMessage = new ProductUpdatedEvent(originalProduct.Id, newStock);

            var harness = _fixture.Api.Services.GetRequiredService<ITestHarness>();

            // Act
            await harness.Bus.Publish(eventMessage);

            // Assert
            while (!harness.Consumed.Select<ProductUpdatedEvent>().Any(e => e.Context.Message.Id == originalProduct.Id))
                await Task.Delay(100);

            using var scope = _fixture.Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();
            var product = context.Products.FirstOrDefault(p => p.Id == originalProduct.Id);

            Assert.NotNull(product);
            Assert.Equal(originalStock, product.Stock); 
        }
    }
}
