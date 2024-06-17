using MassTransit;
using Shared.Messages;

namespace Shared.OrderEvents
{
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; }

        public List<OrderItemMessage> OrderItems { get; set; }


        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
