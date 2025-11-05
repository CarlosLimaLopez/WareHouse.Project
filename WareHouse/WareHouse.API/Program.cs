using WareHouse.Context;
using Microsoft.EntityFrameworkCore;
using WareHouse.Product;
using WareHouse.Repositories;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Contexts
builder.Services.AddDbContext<WareHouseContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Database")) //For quick debug in execution
    //options.UseSqlServer(builder.Configuration.GetConnectionString("Database")) //For production use and integration tests
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
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, (ushort)rabbitPort, rabbitVHost, h =>
        {
            h.Username(rabbitUser!);
            h.Password(rabbitPass!);
        });
    });
});
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// For integration tests, this class is used to start the application.
public partial class Program { }
