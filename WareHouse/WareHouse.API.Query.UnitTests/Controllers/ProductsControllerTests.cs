using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace WareHouse.Product
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly ProductsController _productsController;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _productsController = new ProductsController(_productServiceMock.Object);
        }

        #region Setups
        private Product SetupProduct()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Code = "ABCDEFGH"
            };
        }

        private List<ValidationResult> SetupErrors(string message, string field)
            => [ new(message, [field]) ];
        
        #endregion

        #region GetProducts (GET: api/Products)
        [Fact]
        public async Task GetProduct_ShouldReturnOkWithProducts()
        {
            //Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Code = "ABCDEFGH" },
                new Product { Id = Guid.NewGuid(), Code = "HGFEDCBA" }
            };
            _productServiceMock.Setup(s => s.GetProducts()).ReturnsAsync(products);

            //Act
            var result = await _productsController.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(2, returnedProducts.Count);
        }
        #endregion

        #region GetProduct (GET: api/Products/{id})
        [Fact]
        public async Task GetProduct_WhenExists_ShouldReturnOk()
        {
            //Arrange
            var product = SetupProduct();
            _productServiceMock.Setup(s => s.GetProduct(product.Id)).ReturnsAsync(product);

            //Act
            var result = await _productsController.GetProduct(product.Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(product.Id, returnedProduct.Id);
        }

        [Fact]
        public async Task GetProduct_WhenNotExists_ShouldReturnNotFound()
        {
            //Arrange
            _productServiceMock.Setup(s => s.GetProduct(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            //Act
            var result = await _productsController.GetProduct(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        #endregion
    }
}