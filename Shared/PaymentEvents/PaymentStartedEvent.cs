using MassTransit;
using Shared.Messages;

namespace Shared.PaymentEvents
{
    public class PaymentStartedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; }

        public decimal TotalPrice { get; set; }

        public List<OrderItemMessage> OrderItems { get; set; }


        public PaymentStartedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
