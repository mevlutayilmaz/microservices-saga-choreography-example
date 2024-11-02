using MassTransit;
using Order.API.Contexts;
using Order.API.Enums;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer(OrderAPIDbContext _context) : IConsumer<StockNotReservedEvent>
    {
        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            Entities.Order order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order == null)
                throw new NullReferenceException();

            order.OrderStatu = OrderStatus.Fail;
            await _context.SaveChangesAsync();
        }
    }
}
