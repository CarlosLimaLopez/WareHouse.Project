using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    public class ProductValidationExceptionTests
    {
        [Fact]
        public void Constructor_ShouldSetErrorsProperty()
        {
            // Arrange
            var errors = new List<ValidationResult>
            {
                new ValidationResult("Error 1"),
                new ValidationResult("Error 2")
            };

            // Act
            var exception = new ProductValidationException(errors);

            // Assert
            Assert.Equal(errors, exception.Errors);
        }

        [Fact]
        public void Constructor_ShouldSetMessageWithAllErrorMessages()
        {
            // Arrange
            var errors = new List<ValidationResult>
            {
                new ValidationResult("Error A"),
                new ValidationResult("Error B")
            };

            // Act
            var exception = new ProductValidationException(errors);

            // Assert
            Assert.Contains("Error A", exception.Message);
            Assert.Contains("Error B", exception.Message);
            Assert.Equal("Error A; Error B", exception.Message);
        }

        [Fact]
        public void Errors_ShouldBeEmpty_WhenNoErrorsProvided()
        {
            // Arrange
            var errors = Enumerable.Empty<ValidationResult>();

            // Act
            var exception = new ProductValidationException(errors);

            // Assert
            Assert.Empty(exception.Errors);
            Assert.Equal(string.Empty, exception.Message);
        }
    }
}
