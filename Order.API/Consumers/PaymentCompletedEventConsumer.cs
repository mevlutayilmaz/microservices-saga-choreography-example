using MassTransit;
using Order.API.Contexts;
using Order.API.Enums;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer(OrderAPIDbContext _context) : IConsumer<PaymentCompletedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            Entities.Order order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order == null)
                throw new NullReferenceException();

            order.OrderStatu = OrderStatus.Completed;
            await _context.SaveChangesAsync();
        }
    }
}
