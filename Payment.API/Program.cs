using MassTransit;
using Payment.API.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentStartedEventConsumer>();

    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration["RabbitMq"]);

        configure.ReceiveEndpoint(RabbitMqSettings.Payment_StartedEventQueue, e => e.ConfigureConsumer<PaymentStartedEventConsumer>(context));
    });
});

var app = builder.Build();

app.Run();
