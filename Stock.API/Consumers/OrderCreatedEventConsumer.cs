using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(MongoDBService mongoDBService, ISendEndpointProvider sendEndpointProvider) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            IMongoCollection<Entities.Stock> collection = mongoDBService.GetCollection<Entities.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                stockResult.Add(await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId.ToString() && s.Count >= orderItem.Count)).AnyAsync());
            }

            if(stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    Entities.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId.ToString())).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;

                    await collection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId.ToString(), stock);
                }

                ISendEndpoint sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue }"));

                StockReservedEvent stockReservedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    BuyerId = context.Message.BuyerId,
                    TotalPrice = context.Message.TotalPrice,
                    OrderItems = context.Message.OrderItems,
                };
                await sendEndpoint.Send(stockReservedEvent);
                Console.WriteLine("Stock başarılı");
            }
            else
            {
                ISendEndpoint sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Order_StockNotReservedEventQueue}"));

                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    BuyerId = context.Message.BuyerId,
                    Message = "Stok miktarı yetersiz"
                };
                await sendEndpoint.Send(stockNotReservedEvent);
                Console.WriteLine("Stock başarısız");
            }
        }
    }
}
