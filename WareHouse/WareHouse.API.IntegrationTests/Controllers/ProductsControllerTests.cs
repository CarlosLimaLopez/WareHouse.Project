using System.Net;
using System.Net.Http.Json;


namespace WareHouse.Product
{
    using System.ComponentModel.DataAnnotations;
    using API.IntegrationTests;

    [Collection("WareHouseApiTests")]
    public class ProductsControllerTests : IAsyncLifetime
    {
        private readonly WareHouseApiFixture _fixture;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _fixture.ClearContextAsync();
        }

        private class ProductResponse
        {
            public Guid Id { get; init; }

            public string Code { get; init; } = string.Empty;

            public int Stock { get; set; }
        }

        public ProductsControllerTests(WareHouseApiFixture fixture)
        {
            _fixture = fixture;
        }

        #region PostProduct (POST: api/products)
        [Fact]
        public async Task PostProduct_WhenValid_ShouldReturnCreated()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "ZXCVBNML" };
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/products", product);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(createdProduct);
            Assert.Equal(product.Id, createdProduct.Id);
            Assert.Equal(product.Code, createdProduct.Code);
        }

        [Fact]
        public async Task PostProduct_WhenInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "SHORT" };
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/products", product);

            // Assert
            Assert.Equal((HttpStatusCode)400, response.StatusCode);
        }

        [Fact]
        public async Task PostProduct_WhenDuplicatedProductCode_ShouldReturnUnprocessableEntity()
        {
            // Arrange
            var product1 = new Product { Id = Guid.NewGuid(), Code = "REPEAT01" };
            await _fixture.SeedContextAsync([product1]);

            var client = _fixture.Api.CreateClient();
            var product2 = new Product { Id = Guid.NewGuid(), Code = "REPEAT01" };

            // Act
            var response = await client.PostAsJsonAsync("/api/products", product2);

            // Assert
            Assert.Equal((HttpStatusCode)422, response.StatusCode);
        }
        #endregion

        #region DeleteProduct (DELETE: api/products/{id})
        [Fact]
        public async Task DeleteProduct_WhenExists_ShouldReturnNoContent()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "DELETE01" };
            await _fixture.SeedContextAsync([product]);
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/products/{product.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_WhenNotExists_ShouldReturnNotFound()
        {
            // Arrange
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region PostProductStock (POST: api/products/{id}/stock)
        [Fact]
        public async Task PostProductStock_WhenExists_ShouldReturnCreatedWithIncrementedStock()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "STOCKADD"};
            product.AddStock();
            await _fixture.SeedContextAsync([product]);
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.PostAsync($"/api/products/{product.Id}/stock", null);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var updatedProduct = await response.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(updatedProduct);
            Assert.Equal(product.Id, updatedProduct.Id);
            Assert.Equal(product.Stock + 1, updatedProduct.Stock);
        }

        [Fact]
        public async Task PostProductStock_WhenNotExists_ShouldReturnNotFound()
        {
            // Arrange
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.PostAsync($"/api/products/{Guid.NewGuid()}/stock", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region DeleteProductStock (DELETE: api/products/{id}/stock)
        [Fact]
        public async Task DeleteProductStock_WhenExistsAndStockPositive_ShouldReturnNoContent()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "STOCKDEL" };
            product.AddStock();
            await _fixture.SeedContextAsync([product]);
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/products/{product.Id}/stock");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProductStock_WhenNotExists_ShouldReturnNotFound()
        {
            // Arrange
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/products/{Guid.NewGuid()}/stock");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProductStock_WhenStockZero_ShouldReturnUnprocessableEntity()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "STOCKZER" };
            await _fixture.SeedContextAsync([product]);
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/products/{product.Id}/stock");

            // Assert
            Assert.Equal((HttpStatusCode)422, response.StatusCode);
        }
        #endregion
    }
}
