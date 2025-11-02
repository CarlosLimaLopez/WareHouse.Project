using Moq;

namespace WareHouse.Product
{
    using Repositories;

    public class ProductValidatorTests
    {
        [Fact]
        public async Task ValidateInsertAsync_WhenCodeDoesNotExist_ShouldReturnNoErrors()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "ABCDEFGH" };
            var productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(r => r.GetProductByCode(product.Code)).ReturnsAsync((Product?)null);

            var validator = new ProductValidator(productRepositoryMock.Object);

            // Act
            var results = await validator.ValidateInsertAsync(product);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task ValidateInsertAsync_WhenCodeExists_ShouldReturnValidationError()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "ABCDEFGH" };
            var existingProduct = new Product { Id = Guid.NewGuid(), Code = "ABCDEFGH" };
            var repoMock = new Mock<IProductRepository>();
            repoMock.Setup(r => r.GetProductByCode(product.Code)).ReturnsAsync(existingProduct);

            var validator = new ProductValidator(repoMock.Object);

            // Act
            var results = await validator.ValidateInsertAsync(product);

            // Assert
            var error = Assert.Single(results);
            Assert.Equal($"A product with code '{product.Code}' already exists.", error.ErrorMessage);
            Assert.Contains(nameof(product.Code), error.MemberNames);
        }
    }
}
