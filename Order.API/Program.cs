using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Contexts;
using Order.API.Entities;
using Order.API.Enums;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using Shared.Events.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();
    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);
        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue, e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue, e => e.ConfigureConsumer<StockNotReservedEventConsumer>(context));
    });
});

builder.Services.AddDbContext<OrderAPIDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order", async (CreateOrderVM model, OrderAPIDbContext context, IPublishEndpoint publishEndpoint) =>
{
    Order.API.Entities.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
        CreatedDate = DateTime.Now,
        OrderItems = model.OrderItems.Select(oi => new OrderItem()
        {
            ProductId = Guid.TryParse(oi.ProductId, out Guid _productId) ? _productId : Guid.NewGuid(),
            Count = oi.Count,
            Price = oi.Price,
        }).ToList(),
        TotalPrice = model.OrderItems.Sum(oi => oi.Price * oi.Count),
        OrderStatu = OrderStatus.Suspend,
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new OrderItemMessage()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId,
        }).ToList()
    };
    await publishEndpoint.Publish(orderCreatedEvent);
});

app.Run();
