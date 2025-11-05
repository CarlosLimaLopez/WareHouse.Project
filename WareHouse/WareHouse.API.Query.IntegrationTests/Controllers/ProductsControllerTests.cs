using System.Net;
using System.Net.Http.Json;
using WareHouse.API.Query.IntegrationTests;

namespace WareHouse.Product
{
    [Collection("WareHouseApiQueryTests")]
    public class ProductsControllerTests : IAsyncLifetime
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

        public ProductsControllerTests(WareHouseApiQueryFixture fixture)
        {
            _fixture = fixture;
        }

        #region GetProduct (GET: api/Products/{id})
        [Fact]
        public async Task GetProduct_WhenProductNotExist_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/products/{productId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProduct_WhenProductExist_ReturnsOKAndProduct()
        {
            // Arrange
            var product = new Product() { Id = Guid.NewGuid(), Code = "12345678" };
            var client = _fixture.Api.CreateClient();
            await _fixture.SeedContextAsync([product]);

            // Act
            var response = await client.GetAsync($"/api/products/{product.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var returnedProduct = await response.Content.ReadFromJsonAsync<Product>();
            Assert.NotNull(returnedProduct);
            Assert.Equal(product.Id, returnedProduct.Id);
            Assert.Equal(product.Code, returnedProduct.Code);
        }
        #endregion

        #region GetProducts (GET: api/Products)
        [Fact]
        public async Task GetProducts_WhenNoProductsExist_ReturnsEmptyList()
        {
            // Arrange
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.GetAsync("/api/products");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            Assert.NotNull(products);
            Assert.Empty(products);
        }

        [Fact]
        public async Task GetProducts_WhenProductsExist_ReturnsListWithProducts()
        {
            // Arrange
            var product1 = new Product { Id = Guid.NewGuid(), Code = "11111111" };
            var product2 = new Product { Id = Guid.NewGuid(), Code = "22222222" };
            await _fixture.SeedContextAsync([product1, product2]);
            var client = _fixture.Api.CreateClient();

            // Act
            var response = await client.GetAsync("/api/products");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            Assert.NotNull(products);
            Assert.Contains(products, p => p.Id == product1.Id && p.Code == product1.Code);
            Assert.Contains(products, p => p.Id == product2.Id && p.Code == product2.Code);
        }
        #endregion
    }
}
