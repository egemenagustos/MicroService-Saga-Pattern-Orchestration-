using MassTransit;
using MongoDB.Driver;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration["RabbitMq"]);
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
