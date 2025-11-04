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

        #region PostProduct (POST: api/Products)
        [Fact]
        public async Task PostProduct_WhenValidationFails_ShouldReturnUnprocessableEntity()
        {
            //Arrange
            var product = SetupProduct();
            var errors = SetupErrors("Duplicate code", nameof(product.Code));
            _productServiceMock.Setup(s => s.TryInsertProduct(product)).ReturnsAsync((product, errors));

            //Act
            var result = await _productsController.PostProduct(product);

            //Assert
            var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<ValidationResult>>(unprocessable.Value);
            Assert.Single(returnedErrors);
        }

        [Fact]
        public async Task PostProduct_WhenValidationPasses_ShouldReturnCreated()
        {
            //Arrange
            var product = SetupProduct();
            _productServiceMock.Setup(s => s.TryInsertProduct(product)).ReturnsAsync((product, []));

            //Act
            var result = await _productsController.PostProduct(product);

            //Assert
            var created = Assert.IsType<CreatedResult>(result.Result);
            var returned = Assert.IsAssignableFrom<Product>(created.Value);
            Assert.Equal(product.Id, returned.Id);
        }
        #endregion

        #region DeleteProduct (DELETE: api/Products/{id})
        [Fact]
        public async Task DeleteProduct_WhenNotExists_ShouldReturnNotFound()
        {
            //Arrange
            _productServiceMock.Setup(s => s.TryDeleteProduct(It.IsAny<Guid>())).ReturnsAsync((null, new List<ValidationResult>()));

            //Act
            var result = await _productsController.DeleteProduct(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteProduct_WhenValidationFails_ShouldReturnUnprocessableEntity()
        {
            //Arrange
            var product = SetupProduct();
            var errors = SetupErrors("Cannot delete", nameof(product.Code));
                
            _productServiceMock.Setup(s => s.TryDeleteProduct(product.Id)).ReturnsAsync((product, errors));

            //Act
            var result = await _productsController.DeleteProduct(product.Id);

            //Assert
            var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<ValidationResult>>(unprocessable.Value);
            Assert.Single(returnedErrors);
        }

        [Fact]
        public async Task DeleteProduct_WhenValidationPasses_ShouldReturnNoContent()
        {
            //Arrange
            var product = SetupProduct();
            _productServiceMock.Setup(s => s.TryDeleteProduct(product.Id)).ReturnsAsync((product, []));

            //Act
            var result = await _productsController.DeleteProduct(product.Id);

            //Assert
            Assert.IsType<NoContentResult>(result.Result);
        }
        #endregion

        #region PostProductStock (POST: api/Products/{id}/stock)
        [Fact]
        public async Task PostProductStock_WhenNotExists_ShouldReturnNotFound()
        {
            //Arrange
            _productServiceMock.Setup(s => s.TryAddStock(It.IsAny<Guid>())).ReturnsAsync((null, []));

            //Act
            var result = await _productsController.PostProductStock(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostProductStock_WhenExists_ShouldReturnCreated()
        {
            //Arrange
            var product = SetupProduct();
            _productServiceMock.Setup(s => s.TryAddStock(product.Id)).ReturnsAsync((product, []));

            //Act
            var result = await _productsController.PostProductStock(product.Id);

            //Assert
            var created = Assert.IsType<CreatedResult>(result.Result);
            var returned = Assert.IsAssignableFrom<Product>(created.Value);
            Assert.Equal(product.Id, returned.Id);
        }
        #endregion

        #region DeleteProductStock (DELETE: api/Products/{id}/stock)
        [Fact]
        public async Task DeleteProductStock_WhenNotExists_ShouldReturnNotFound()
        {
            //Arrange
            _productServiceMock.Setup(s => s.TryRemoveStock(It.IsAny<Guid>())).ReturnsAsync((null, []));

            //Act
            var result = await _productsController.DeleteProductStock(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteProductStock_WhenValidationFails_ShouldReturnUnprocessableEntity()
        {
            //Arrange
            var product = SetupProduct();
            var errors = SetupErrors("Stock is zero", nameof(product.Stock));
                
            _productServiceMock.Setup(s => s.TryRemoveStock(product.Id)).ReturnsAsync((product, errors));

            //Act
            var result = await _productsController.DeleteProductStock(product.Id);

            //Assert
            var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<ValidationResult>>(unprocessable.Value);
            Assert.Single(returnedErrors);
        }

        [Fact]
        public async Task DeleteProductStock_WhenValidationPasses_ShouldReturnNoContent()
        {
            var product = SetupProduct();
            _productServiceMock.Setup(s => s.TryRemoveStock(product.Id)).ReturnsAsync((product, []));

            var result = await _productsController.DeleteProductStock(product.Id);

            Assert.IsType<NoContentResult>(result.Result);
        }
        #endregion
    }
}