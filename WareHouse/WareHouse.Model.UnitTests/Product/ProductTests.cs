using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    public class ProductTests
    {
        [Fact]
        public void Constructor_Always_ShouldInitializeWithZeroStock()
        {
            // Act
            var product = new Product { Id = Guid.NewGuid(), Code = "12345678901234567890" };

            // Assert
            Assert.Equal(0, product.Stock);
        }

        [Theory]
        [InlineData("")]    // empty
        public void Validate_WhenEmptyCode_ShouldFailRequiredValidation(string invalidCode)
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = invalidCode };
            var validationContext = new ValidationContext(product);
            var results = new List<ValidationResult>();

            // Act
            Validator.TryValidateObject(product, validationContext, results, true);

            // Assert
            Assert.Single(results);
            Assert.Equal("The Code field is required.", results.First().ErrorMessage);
        }

        [Theory]
        [InlineData("1234567")]   // 7 chars
        [InlineData("123456789")] // 9 chars
        public void Validate_WhenInvalidCodeLength_ShouldFailCodeLengthValidation(string invalidCode)
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = invalidCode };
            var validationContext = new ValidationContext(product);
            var results = new List<ValidationResult>();

            // Act
            Validator.TryValidateObject(product, validationContext, results, true);

            // Assert
            Assert.Single(results);
            Assert.Equal("Code must be exactly 8 characters.", results.First().ErrorMessage);
        }

        [Fact]
        public void AddStock_Always_IncrementStockByOne()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "12345678901234567890" };

            // Act
            product.AddStock();

            // Assert
            Assert.Equal(1, product.Stock);
        }

        [Fact]
        public void RemoveStock_WhenStockGreaterThanZero_ShouldDecrementStockByOne()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "12345678901234567890" };
            product.AddStock();

            // Act
            product.RemoveStock();

            // Assert
            Assert.Equal(0, product.Stock);
        }

        [Fact]
        public void RemoveStock_WhenStockIsZero_ShouldThrowStockIsZeroException()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "12345678901234567890" };

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => product.RemoveStock());

            // Assert
            Assert.Equal("Cannot remove stock when stock level is zero.", ex.Message);
        }

        [Fact]
        public void ValidateRemoveStock_WhenStockIsZero_ShouldFailStockIsZeroValidation()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "12345678901234567890" };
            var context = new ValidationContext(product);

            // Act
            var results = product.ValidateRemoveStock().ToList();

            // Assert
            Assert.Single(results);
            Assert.Equal("Cannot remove stock when stock level is zero.", results.First().ErrorMessage);
        }

        [Fact]
        public void ValidateRemoveStock_WhenStockGreaterThanZero_ShouldReturnEmpty()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Code = "12345678901234567890" };
            product.AddStock();
            var context = new ValidationContext(product);

            // Act
            var results = product.ValidateRemoveStock().ToList();

            // Assert
            Assert.Empty(results);
        }
    }
}
