using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
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

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }

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

            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.UtcNow;
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEventQueue}"), context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems
                }));

            
            During(OrderCreated, When(StockReservedEvent).TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMqSettings.Payment_StartedEventQueue}"),
                context => new PaymentStartedEvent(context.Instance.CorrelationId)
            {
                TotalPrice = context.Instance.TotalPrice,
                OrderItems = context.Data.OrderItems
            }), When(StockNotReservedEvent).TransitionTo(StockNotReserved)
            .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderFailedEventQueue}"), context => new OrderFailedEvent
            {
                OrderId = context.Instance.OrderId,
                Message = context.Data.Message
            }));

            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue: {RabbitMqSettings.Order_OrderCompletedEventQueue}"), context => new OrderCompletedEvent
                {
                    OrderId = context.Instance.OrderId, 
                }).Finalize(),
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue: {RabbitMqSettings.Order_OrderFailedEventQueue}"), context => new OrderFailedEvent()
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                })
                .Send(new Uri($"queue: {RabbitMqSettings.Stock_RollbackMessageQueue}"), context => new StockRollbackMessage()
                {
                    OrderItems = context.Data.OrderItems
                }));

            SetCompletedWhenFinalized();
        }
    }
}
