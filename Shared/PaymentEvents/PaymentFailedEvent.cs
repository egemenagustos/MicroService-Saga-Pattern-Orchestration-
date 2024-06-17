﻿using MassTransit;
using Shared.Messages;

namespace Shared.PaymentEvents
{
    public class PaymentFailedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; }

        public string Message { get; set; }

        public List<OrderItemMessage> OrderItems { get; set; }

        public PaymentFailedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
