using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;

namespace WareHouse.Context
{
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }

    public class DatabaseFixture : IAsyncLifetime
    {
        #region Constructor
        private MsSqlContainer SqlInstance { get; init; }
        private const string MsSqlPassword = "Your!Passw0rd";
        public DatabaseFixture()
        {
            SqlInstance = new MsSqlBuilder()
               .WithPassword(MsSqlPassword)
               .WithWaitStrategy(
                   Wait.ForUnixContainer()
                   .UntilPortIsAvailable(1433)
               )
            .Build();
        }
        #endregion

        async Task IAsyncLifetime.DisposeAsync()
        {
            if (SqlInstance != null) await SqlInstance.DisposeAsync();
        }

        async Task IAsyncLifetime.InitializeAsync()
        {
            await SqlInstance.StartAsync();

            using var context = CreateContext(false);

            await context.Database.EnsureCreatedAsync();
        }

        public WareHouseContext CreateContext(bool transaction = true)
        {
            var options = new DbContextOptionsBuilder<WareHouseContext>()
                .UseSqlServer(SqlInstance.GetConnectionString())
                .Options;

            var context = new WareHouseContext(options);

            if (transaction)
                context.Database.BeginTransaction();
            
            return context;
        }
    }
}