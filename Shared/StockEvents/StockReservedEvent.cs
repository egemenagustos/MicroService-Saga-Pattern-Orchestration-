using MassTransit;
using Shared.Messages;

namespace Shared.StockEvents
{
    public class StockReservedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; }

        public List<OrderItemMessage> OrderItems { get; set; }

        public StockReservedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
