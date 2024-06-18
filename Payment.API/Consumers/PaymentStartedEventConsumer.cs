using MassTransit;
using Shared.PaymentEvents;
using Shared.Settings;

namespace Payment.API.Consumers
{
    public class PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider) : IConsumer<PaymentStartedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));

            if (true)
            {
                PaymentCompletedEvent payment = new(context.Message.CorrelationId)
                {                  
                };

                await sendEndpoint.Send(payment)c
            }

            else
            {

                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Yetersiz bakiye",
                    OrderItems = context.Message.OrderItems
                };

                await sendEndpoint.Send(paymentFailedEvent);
            }
        }
    }
}
