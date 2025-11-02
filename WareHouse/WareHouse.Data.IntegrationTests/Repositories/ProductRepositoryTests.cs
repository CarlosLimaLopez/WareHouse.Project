namespace WareHouse.Repositories
{
    using Context;
    using Product;

    [Collection("DatabaseCollection")]
    public class ProductRepositoryTests
    {
        private DatabaseFixture Fixture { get; }

        public ProductRepositoryTests(DatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        #region Add
        [Fact]
        public async Task Add_WhenNotDuplicatedCode_ShouldInsertNewProduct()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            var productId = Guid.NewGuid();
            var productCode = "12345678";
            var product = new Product { Id = productId, Code = productCode };

            // Act
            sut.Add(product);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var addedProduct = context.Products.FirstOrDefault(t => t.Id == productId);
            Assert.NotNull(addedProduct);
            Assert.Equal(product.Code, addedProduct.Code);
        }

        [Fact]
        public async Task Add_WhenDuplicatedCode_ShouldThrowDbUpdateExceptionWithExpectedMessage()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            var productId1 = Guid.NewGuid();
            var productCode = "87654321";
            var product1 = new Product { Id = productId1, Code = productCode };
            sut.Add(product1);
            await context.SaveChangesAsync();

            var productId2 = Guid.NewGuid();
            var product2 = new Product { Id = productId2, Code = productCode };
            sut.Add(product2);

            // Act
            Func<Task> act = async () => await context.SaveChangesAsync();

            // Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(act);
            Assert.Contains("Cannot insert duplicate key", exception.InnerException?.Message);
            Assert.Contains("IX_Products_Code", exception.InnerException?.Message);
        }
        #endregion

        #region GetProductById
        [Fact]
        public async Task GetProductById_WhenNoneExist_ShouldReturnNull()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            // Act
            var result = await sut.GetProductById(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductById_WhenExist_ShouldReturnProduct()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            var productId = Guid.NewGuid();
            var productCode = "ABCDEFGH";
            var product = new Product { Id = productId, Code = productCode };
            sut.Add(product);
            await context.SaveChangesAsync();

            // Act
            var result = await sut.GetProductById(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
        }
        #endregion

        #region GetProductByCode
        [Fact]
        public async Task GetProductByCode_WhenNoneExist_ShouldReturnNull()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            // Act
            var result = await sut.GetProductByCode("NONEXIST1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductByCode_WhenExist_ShouldReturnProduct()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            var productId = Guid.NewGuid();
            var productCode = "ZXCVBNML";
            var product = new Product { Id = productId, Code = productCode };
            sut.Add(product);
            await context.SaveChangesAsync();

            // Act
            var result = await sut.GetProductByCode(productCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productCode, result.Code);
        }
        #endregion

        #region GetProducts
        [Fact]
        public async Task GetProducts_WhenExistProducts_ShouldReturnNotEmptyList()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            var productId = Guid.NewGuid();
            var productCode = "QWERTYUI";
            var product = new Product { Id = productId, Code = productCode };
            sut.Add(product);
            await context.SaveChangesAsync();

            // Act
            var result = await sut.GetProducts();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, p => p.Id == productId);
        }
        #endregion

        #region Remove
        [Fact]
        public async Task Remove_WhenProductExists_ShouldDeleteProduct()
        {
            // Arrange
            using var context = Fixture.CreateContext();
            var sut = new ProductRepository(context);

            var productId = Guid.NewGuid();
            var productCode = "REMOVEEE";
            var product = new Product { Id = productId, Code = productCode };
            sut.Add(product);
            await context.SaveChangesAsync();

            // Act
            sut.Remove(product);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var deletedProduct = context.Products.FirstOrDefault(t => t.Id == productId);
            Assert.Null(deletedProduct);
        }
        #endregion
    }
}
