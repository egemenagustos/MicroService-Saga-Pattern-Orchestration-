using MassTransit;
using SagaStateMachine.Service.StateInstances;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public OrderStateMachine()
        {
            //State machine yapılacak durum bilgilendirilmesi currenstate'de tutulacak.
            InstanceState(instance => instance.CurrentState);
        }
    }
}
