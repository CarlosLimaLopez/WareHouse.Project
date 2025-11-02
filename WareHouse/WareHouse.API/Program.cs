using WareHouse.Context;
using Microsoft.EntityFrameworkCore;
using WareHouse.Product;
using WareHouse.Repositories;
using WareHouse.Data;

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    ContextSeeder.Initialize(scope.ServiceProvider);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
