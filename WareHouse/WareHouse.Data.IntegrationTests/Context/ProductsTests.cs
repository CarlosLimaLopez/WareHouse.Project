namespace WareHouse.Context
{
    using Product;

    [Collection("DatabaseCollection")]
    public class ProductsTests
    {
        private DatabaseFixture Fixture { get; }

        public ProductsTests(DatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        #region IX_Products_Code
        [Fact]
        public async Task IX_Products_Code_WhenDuplicatedCode_ShouldReturnDuplicatedException()
        {
            // Arrange
            using var context = Fixture.CreateContext();

            string productCode = "12345678";

            var product1 = new Product { Id = Guid.NewGuid(), Code = productCode };
            var product2 = new Product { Id = Guid.NewGuid(), Code = productCode };

            context.Products.Add(product1);
            await context.SaveChangesAsync();

            // Act
            context.Products.Add(product2);
            Func<Task> act = async () => await context.SaveChangesAsync();

            //Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(act);

            Assert.Contains("Cannot insert duplicate key", exception.InnerException?.Message);
            Assert.Contains("IX_Products_Code", exception.InnerException?.Message);
        }
        #endregion
    }
}