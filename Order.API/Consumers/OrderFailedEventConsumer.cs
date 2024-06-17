using MassTransit;
using Order.API.Contexts;
using Shared.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderFailedEventConsumer(OrderDbContext orderDbContext) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Models.Order order = await orderDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order is null)
                throw new Exception("Order is not found!");

            order.OrderStatus = Enums.OrderStatus.Fail;

            await orderDbContext.SaveChangesAsync();
        }
    }
}
