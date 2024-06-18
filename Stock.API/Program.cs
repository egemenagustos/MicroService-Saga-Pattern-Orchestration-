using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollBackMessageConsumer>();

    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration["RabbitMq"]);

        configure.ReceiveEndpoint(RabbitMqSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        configure.ReceiveEndpoint(RabbitMqSettings.Stock_RollbackMessageQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

using var scope = builder.Services.BuildServiceProvider().CreateAsyncScope();

var mongoService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

if(!await(await mongoService.GetCollection<Stock.API.Models.Stock>().FindAsync(x=> true)).AnyAsync())
{
    mongoService.GetCollection<Stock.API.Models.Stock>()
        .InsertOne(new()
        {
            ProductId = 1,
            Count = 200
        });

    mongoService.GetCollection<Stock.API.Models.Stock>()
       .InsertOne(new()
       {
           ProductId = 2,
           Count = 300
       });

    mongoService.GetCollection<Stock.API.Models.Stock>()
       .InsertOne(new()
       {
           ProductId = 3,
           Count = 50
       });
}

app.Run();
