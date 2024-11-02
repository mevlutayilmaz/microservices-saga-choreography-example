namespace Order.API.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
