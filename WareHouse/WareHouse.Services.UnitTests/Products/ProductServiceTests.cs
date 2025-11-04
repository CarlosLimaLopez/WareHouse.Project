using Moq;

namespace WareHouse.Product
{
    using Context;
    using Repositories;

    public class ProductServiceTests
    {
        #region Private dependencies
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly ProductValidator _productValidator;
        private readonly Mock<IUnitOfWork<WareHouseContext>> _unitOfWorkMock;
        private readonly ProductService _productService;
        #endregion

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _productValidator = new ProductValidator(_productRepositoryMock.Object);
            _unitOfWorkMock = new Mock<IUnitOfWork<WareHouseContext>>();
            _productService = new ProductService(_productRepositoryMock.Object, _productValidator, _unitOfWorkMock.Object);
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
        #endregion

        #region GetProduct
        [Fact]
        public async Task GetProduct_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            var product = SetupProduct();
            _productRepositoryMock.Setup(r => r.GetProductByIdAsNoTracking(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProduct(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Code, result.Code);
        }

        [Fact]
        public async Task GetProduct_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _productRepositoryMock.Setup(r => r.GetProductById(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProduct(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetProducts
        [Fact]
        public async Task GetProducts_WhenProductsExist_ShouldReturnList()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Code = "ABCDEFGH" },
                new Product { Id = Guid.NewGuid(), Code = "HGFEDCBA" }
            };
            _productRepositoryMock.Setup(r => r.GetProductsAsNoTracking()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Code == "ABCDEFGH");
            Assert.Contains(result, p => p.Code == "HGFEDCBA");
        }

        [Fact]
        public async Task GetProducts_WhenNoProductsExist_ShouldReturnEmptyList()
        {
            // Arrange
            _productRepositoryMock.Setup(r => r.GetProductsAsNoTracking()).ReturnsAsync(new List<Product>());

            // Act
            var result = await _productService.GetProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        #endregion

        #region TryInsertProduct
        [Fact]
        public async Task TryInsertProduct_WhenValidationFails_ShouldReturnErrors()
        {
            // Arrange
            var product = SetupProduct();
            var expectedErrorMessage = $"A product with code '{product.Code}' already exists.";
            var expectedMemberName = nameof(product.Code);

            _productRepositoryMock.Setup(v => v.GetProductByCode(product.Code)).ReturnsAsync(product);

            // Act
            var result = await _productService.TryInsertProduct(product);

            // Assert
            Assert.Equal(product, result.product);
            var error = Assert.Single(result.errors);
            Assert.Equal(expectedErrorMessage, error.ErrorMessage);
            Assert.Contains(expectedMemberName, error.MemberNames);
            _productRepositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TryInsertProduct_WhenValidationPasses_ShouldInsertProduct()
        {
            // Arrange
            var product = SetupProduct();
            _productRepositoryMock.Setup(v => v.GetProductByCode(product.Code)).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.TryInsertProduct(product);

            // Assert
            Assert.Equal(product, result.product);
            Assert.Empty(result.errors);
            _productRepositoryMock.Verify(r => r.Add(product), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
        #endregion

        #region TryDeleteProduct
        [Fact]
        public async Task TryDeleteProduct_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _productRepositoryMock.Setup(r => r.GetProductById(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.TryDeleteProduct(Guid.NewGuid());

            // Assert
            Assert.Null(result.product);
            Assert.Empty(result.errors);
            _productRepositoryMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TryDeleteProduct_WhenValidationFails_ShouldReturnErrors()
        {
            // Arrange
            var product = SetupProduct();
            product.AddStock();
            var expectedErrorMessage = "Cannot remove a product when stock level is zero.";
            var expectedMemberName = nameof(product.Stock);

            _productRepositoryMock.Setup(r => r.GetProductById(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _productService.TryDeleteProduct(product.Id);

            // Assert
            Assert.Equal(product, result.product);
            var error = Assert.Single(result.errors);
            Assert.Equal(expectedErrorMessage, error.ErrorMessage);
            Assert.Contains(expectedMemberName, error.MemberNames);
            _productRepositoryMock.Verify(r => r.Remove(product), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TryDeleteProduct_WhenValidationPasses_ShouldDeleteProduct()
        {
            // Arrange
            var product = SetupProduct();
            _productRepositoryMock.Setup(r => r.GetProductById(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _productService.TryDeleteProduct(product.Id);

            // Assert
            Assert.Equal(product, result.product);
            Assert.Empty(result.errors);
            _productRepositoryMock.Verify(r => r.Remove(product), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
        #endregion

        #region TryAddStock
        [Fact]
        public async Task TryAddStock_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _productRepositoryMock.Setup(r => r.GetProductById(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.TryAddStock(Guid.NewGuid());

            // Assert
            Assert.Null(result.product);
            Assert.Empty(result.errors);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TryAddStock_WhenProductExists_ShouldIncrementStock()
        {
            // Arrange
            var product = SetupProduct(); 
            _productRepositoryMock.Setup(r => r.GetProductById(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _productService.TryAddStock(product.Id);

            // Assert
            Assert.Equal(product, result.product);
            Assert.Empty(result.errors);
            Assert.Equal(1, product.Stock);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
        #endregion

        #region TryRemoveStock
        [Fact]
        public async Task TryRemoveStock_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _productRepositoryMock.Setup(r => r.GetProductById(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.TryRemoveStock(Guid.NewGuid());

            // Assert
            Assert.Null(result.product);
            Assert.Empty(result.errors);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TryRemoveStock_WhenValidationFails_ShouldReturnErrors()
        {
            // Arrange
            var product = SetupProduct();
            var expectedErrorMessage = "Cannot remove stock when stock level is zero.";
            var expectedMemberName = nameof(product.Stock);

            _productRepositoryMock.Setup(r => r.GetProductById(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _productService.TryRemoveStock(product.Id);

            // Assert
            Assert.Equal(product, result.product);
            var error = Assert.Single(result.errors);
            Assert.Equal(expectedErrorMessage, error.ErrorMessage);
            Assert.Contains(expectedMemberName, error.MemberNames);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task TryRemoveStock_WhenValidationPasses_ShouldDecrementStock()
        {
            // Arrange
            var product = SetupProduct();
            product.AddStock(); // Ensure stock > 0
            _productRepositoryMock.Setup(r => r.GetProductById(product.Id)).ReturnsAsync(product);

            // Act
            var result = await _productService.TryRemoveStock(product.Id);

            // Assert
            Assert.Equal(product, result.product);
            Assert.Empty(result.errors);
            Assert.Equal(0, product.Stock);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
        #endregion
    }
}
