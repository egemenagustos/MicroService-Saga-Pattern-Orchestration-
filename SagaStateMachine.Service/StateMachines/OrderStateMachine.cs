using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.StockEvents;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        // Gelebilecek eventleri bu şekilde property olarak tanımlayacağız. Böylece state machine'de temsil edeceğiz.
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }

        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }

        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        public OrderStateMachine()
        {
            // State machine yapılacak durum bilgilendirilmesi currenstate'de tutulacak.
            InstanceState(instance => instance.CurrentState);

            // Gelen event OrderStartedEvent ise => CorrelateBy metodu ile veritabanında tutulan order state instance'da ki
            // event orderId'yi kıyaslıyoruz. Bu kıyas sayesinde iligli instance varsa gelenin yeni bir sipariş olmadığını
            // anlıyoruz ve kaydetmiyoruz.
            Event(
                () => OrderStartedEvent, 
                orderStateInstance => orderStateInstance.CorrelateBy<int>(db => db.OrderId, @event => @event.Message.OrderId)
                .SelectId(x => Guid.NewGuid())
                );

            Event(
               () => StockReservedEvent,
               orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(
               () => StockNotReservedEvent,
               orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(
               () => PaymentCompletedEvent,
               orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(
             () => PaymentFailedEvent,
             orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
        }
    }
}
