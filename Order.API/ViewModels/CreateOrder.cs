namespace Order.API.ViewModels
{
    public class CreateOrder
    {
        public int BuyerId { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
