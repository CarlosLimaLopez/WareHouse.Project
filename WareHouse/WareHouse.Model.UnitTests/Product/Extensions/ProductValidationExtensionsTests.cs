namespace WareHouse.Product
{
    public class ProductValidationExtensionsTests
    {
        [Fact]
        public void ValidateAttributes_ShouldReturnNoErrors_ForValidProduct()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = "ABCDEFGH" // 8 caracteres válidos
            };

            // Act
            var errors = product.ValidateAttributes();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateAttributes_ShouldReturnError_WhenCodeIsNull()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = null!
            };

            // Act
            var errors = product.ValidateAttributes();

            // Assert
            var error = Assert.Single(errors);
            Assert.Equal("The Code field is required.", error.ErrorMessage);
            Assert.Contains("Code", error.MemberNames);
        }

        [Fact]
        public void ValidateAttributes_ShouldReturnError_WhenCodeIsTooShort()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = "ABC" // Menos de 8 caracteres
            };

            // Act
            var errors = product.ValidateAttributes();

            // Assert
            var error = Assert.Single(errors);
            Assert.Equal("Code must be exactly 8 characters.", error.ErrorMessage);
            Assert.Contains("Code", error.MemberNames);
        }

        [Fact]
        public void ValidateAttributes_ShouldReturnError_WhenCodeIsTooLong()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = "ABCDEFGHIJK" // Más de 8 caracteres
            };

            // Act
            var errors = product.ValidateAttributes();

            // Assert
            var error = Assert.Single(errors);
            Assert.Equal("Code must be exactly 8 characters.", error.ErrorMessage);
            Assert.Contains("Code", error.MemberNames);
        }
    }
}
