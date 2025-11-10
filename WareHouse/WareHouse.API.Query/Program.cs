using MassTransit;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using WareHouse.Context;
using WareHouse.Product;
using WareHouse.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Contexts
builder.Services.AddDbContext<WareHouseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"))
);
#endregion

#region Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductValidator>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUnitOfWork<WareHouseContext>, UnitOfWork<WareHouseContext>>();
#endregion

#region MassTransit
var rabbitConfig = builder.Configuration.GetSection("RabbitMq");
var rabbitHost = rabbitConfig.GetValue<string>("Host");
var rabbitPort = rabbitConfig.GetValue<int>("Port");
var rabbitUser = rabbitConfig.GetValue<string>("Username");
var rabbitPass = rabbitConfig.GetValue<string>("Password");
var rabbitVHost = rabbitConfig.GetValue<string>("VirtualHost");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedEventConsumer>();
    x.AddConsumer<ProductUpdatedEventConsumer>();
    x.AddConsumer<ProductDeletedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, (ushort)rabbitPort, rabbitVHost, h =>
        {
            h.Username(rabbitUser!);
            h.Password(rabbitPass!);
        });

        cfg.ReceiveEndpoint("product-created-events", e =>
            e.ConfigureConsumer<ProductCreatedEventConsumer>(context));
        cfg.ReceiveEndpoint("product-updated-events", e =>
            e.ConfigureConsumer<ProductUpdatedEventConsumer>(context));
        cfg.ReceiveEndpoint("product-deleted-events", e =>
            e.ConfigureConsumer<ProductDeletedEventConsumer>(context));
    });
});
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Urls.Add("http://0.0.0.0:80");

app.UseMetricServer();

app.UseHttpMetrics();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


// For integration tests, this class is used to start the application.
public partial class Program { }
