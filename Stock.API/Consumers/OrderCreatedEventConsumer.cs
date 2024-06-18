using MassTransit;
using MongoDB.Driver;
using Shared.OrderEvents;
using Shared.Settings;
using Shared.StockEvents;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResults = new();

            var stockCollection = mongoDbService.GetCollection<Models.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                stockResults.Add(await(await stockCollection
                    .FindAsync(
                    x => x.ProductId == orderItem.ProductId
                    &&
                    x.Count >= (long)orderItem.Count)).AnyAsync());
            }

            var sendEnpoint = await sendEndpointProvider
                .GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));

            if (stockResults.TrueForAll(x => x.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await (await stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId))
                        .FirstOrDefaultAsync();

                    stock.Count -= orderItem.Count;

                   await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };

                await sendEnpoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stok yetersiz !"
                };

                await sendEnpoint.Send(stockNotReservedEvent);
            }
        }
    }
}
