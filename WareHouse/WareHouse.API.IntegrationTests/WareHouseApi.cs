using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using MassTransit;

namespace WareHouse.API.IntegrationTests
{
    using Context;
    using MassTransit.SqlTransport;
    using MassTransit.Testing;
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
        #region Sql Container Setup
        private MsSqlContainer? SqlInstance;
        private readonly string SqlPassword = "Your!Passw0rd";
        private int SqlPort => 1433;

        private void ConfigureWareHouseContext(IServiceCollection services)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(SqlInstance?.GetConnectionString() ?? string.Empty)
            {
                InitialCatalog = "WareHouseCommandDatabaseTests"
            };

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WareHouseContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<WareHouseContext>(options =>
                options.UseSqlServer(connectionStringBuilder.ConnectionString));
        }
        #endregion

        #region RabbitMQ Container Setup
        private RabbitMqContainer? RabbitMqInstance;
        private string RabbitMqHost => RabbitMqInstance?.Hostname ?? "localhost";
        private int RabbitMqHostPort => 5674;
        private int RabbitMqDefaultPort => 5672;
        private string RabbitMqUser => "user";
        private string RabbitMqPass => "password";

        private void ConfigureMassTransit(IServiceCollection services)
        {
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("MassTransit"))
                .ToList();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransit(x =>
            {
                x.AddConsumer<ProductCreatedEventTestConsumer>();
                x.AddConsumer<ProductUpdatedEventTestConsumer>();
                x.AddConsumer<ProductDeletedEventTestConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(RabbitMqHost, (ushort)RabbitMqHostPort, "/", h =>
                    {
                        h.Username(RabbitMqUser);
                        h.Password(RabbitMqPass);
                    });

                    cfg.ReceiveEndpoint("test-product-created-events", e =>
                        e.ConfigureConsumer<ProductCreatedEventTestConsumer>(ctx));
                    cfg.ReceiveEndpoint("test-product-updated-events", e =>
                        e.ConfigureConsumer<ProductUpdatedEventTestConsumer>(ctx));
                    cfg.ReceiveEndpoint("test-product-deleted-events", e =>
                        e.ConfigureConsumer<ProductDeletedEventTestConsumer>(ctx));
                });
            });
        }
        #endregion

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                ConfigureWareHouseContext(services);
                ConfigureMassTransit(services);
            });
        }

        public async Task InitializeAsync()
        {
            #region Start Sql & RabbitMQ Containers
            SqlInstance = new MsSqlBuilder()
                .WithPassword(SqlPassword)
            .WithWaitStrategy(
                    Wait.ForUnixContainer()
                    .UntilPortIsAvailable(SqlPort)
                )
                .Build();

            RabbitMqInstance = new RabbitMqBuilder()
                .WithUsername(RabbitMqUser)
                .WithPassword(RabbitMqPass)
                .WithPortBinding(RabbitMqHostPort, RabbitMqDefaultPort)
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                    .UntilPortIsAvailable(RabbitMqDefaultPort)
                )
                .Build();

            var startTasks = new List<Task>();
            if (SqlInstance != null)
                startTasks.Add(SqlInstance.StartAsync());
            if (RabbitMqInstance != null)
                startTasks.Add(RabbitMqInstance.StartAsync());

            await Task.WhenAll(startTasks);
            #endregion

            #region Apply Migrations
            using var scope = Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();

            context.Database.EnsureCreated();

            await context.SaveChangesAsync();
            #endregion

            #region Wait for Bus to be Healthy      
            var bus = scope.ServiceProvider.GetRequiredService<IBusControl>();
            while (bus.CheckHealth().Status != BusHealthStatus.Healthy)
                await Task.Delay(100);
            #endregion
        }

        public override async ValueTask DisposeAsync()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WareHouseContext>();

            await context.Database.EnsureDeletedAsync();

            if (SqlInstance != null)
                await SqlInstance.DisposeAsync();

            if (RabbitMqInstance != null)
                await RabbitMqInstance.DisposeAsync();
        }
    }
}
