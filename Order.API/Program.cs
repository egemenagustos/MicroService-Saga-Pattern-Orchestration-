using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;

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



app.Run();
