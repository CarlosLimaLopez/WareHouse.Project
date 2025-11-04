using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;

namespace WareHouse.API.IntegrationTests
{
    using Context;
    using Product;

    [CollectionDefinition("WareHouseApiTests", DisableParallelization = true)]
    public class WareHouseApiCollection : ICollectionFixture<WareHouseApiFixture>
    {
    }

    public class WareHouseApiFixture : IAsyncLifetime
    {
        public WareHouseApi Api { get; } = new();

        public async Task InitializeAsync()
        {
            await Api.InitializeAsync();
        }

        public async Task DisposeAsync()
        {
            await Api.DisposeAsync();
        }

        public async Task SeedContextAsync(List<Product> products)
        {
            using var scope = Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();

            context.Products.AddRange(
                products
            );

            await context.SaveChangesAsync();
        }

        public async Task ClearContextAsync()
        {
            using var scope = Api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();

            await context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(WareHouseContext.Products)}");
        }
    }
    
    public class WareHouseApi : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private MsSqlContainer? SqlInstance;
        private readonly string SqlPassword = "Your!Passw0rd";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(SqlInstance?.GetConnectionString() ?? string.Empty)
                {
                    InitialCatalog = "WareHouseCommandDatabaseTests"
                };

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WareHouseContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<WareHouseContext>(options =>
                {
                    options.UseSqlServer(connectionStringBuilder.ConnectionString);
                });

            });
        }

        public async Task InitializeAsync()
        {
            SqlInstance = new MsSqlBuilder()
                .WithPassword(SqlPassword)
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                    .UntilPortIsAvailable(1433)
                )
            .Build();

            if (SqlInstance != null)
                await SqlInstance.StartAsync();

            using var scope = Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();

            context.Database.EnsureCreated();

            await context.SaveChangesAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();

            await context.Database.EnsureDeletedAsync();

            if (SqlInstance != null)
                await SqlInstance.DisposeAsync();
        }
    }
}
