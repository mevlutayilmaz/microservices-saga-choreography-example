using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer(IPublishEndpoint publishEndpoint) : IConsumer<StockReservedEvent>
    {
        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new() { OrderId = context.Message.OrderId };
                await publishEndpoint.Publish(paymentCompletedEvent);
                Console.WriteLine("Ödeme başarılı");
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems,
                    Message = "Yetersiz bakiye"
                };
                await publishEndpoint.Publish(paymentFailedEvent);
                Console.WriteLine("Ödeme başarısız");
            }
        }
    }
}
