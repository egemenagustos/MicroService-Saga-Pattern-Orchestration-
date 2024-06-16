using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using Order.API.ViewModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration["RabbitMq"]);
    });
});

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["MsSql"]);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrder request, OrderDbContext context) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = request.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = Order.API.Enums.OrderStatus.Suspend,
        TotalPrice = request.OrderItems.Sum(x => x.Price * x.Count),
        OrderItems = request.OrderItems.Select(x => new Order.API.Models.OrderItem
        {
            Count = x.Count,
            Price = x.Price,
            ProductId = x.ProductId,
        }).ToList()
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();
});



app.Run();
