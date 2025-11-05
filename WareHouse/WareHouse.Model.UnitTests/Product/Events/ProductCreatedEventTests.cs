namespace WareHouse.Product
{
    public class ProductCreatedEventTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var code = "ABCDEFGH";

            // Act
            var evt = new ProductCreatedEvent(id, code);

            // Assert
            Assert.Equal(id, evt.Id);
            Assert.Equal(code, evt.Code);
        }

        [Fact]
        public void ToProduct_ShouldReturnProductWithSameProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var code = "ABCDEFGH";
            var evt = new ProductCreatedEvent(id, code);

            // Act
            var product = evt.ToProduct();

            // Assert
            Assert.Equal(id, product.Id);
            Assert.Equal(code, product.Code);
            Assert.Equal(0, product.Stock);
        }

        [Fact]
        public void ToProduct_ShouldReturnNewInstanceEachTime()
        {
            // Arrange
            var id = Guid.NewGuid();
            var code = "ABCDEFGH";
            var evt = new ProductCreatedEvent(id, code);

            // Act
            var product1 = evt.ToProduct();
            var product2 = evt.ToProduct();

            // Assert
            Assert.NotSame(product1, product2);
        }
    }
}
