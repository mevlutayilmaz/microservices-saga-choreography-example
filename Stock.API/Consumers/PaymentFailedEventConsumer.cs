using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer(MongoDBService mongoDBService) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            IMongoCollection<Entities.Stock> collection = mongoDBService.GetCollection<Entities.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                Entities.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId.ToString())).FirstOrDefaultAsync();

                if(stock is not null)
                {
                    stock.Count += orderItem.Count;
                    await collection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId.ToString(), stock);
                }
            }
        }
    }
}
