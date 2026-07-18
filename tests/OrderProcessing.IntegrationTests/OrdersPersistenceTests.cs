using Microsoft.EntityFrameworkCore;
using OrderProcessing.Modules.Orders.Domain;
using OrderProcessing.Modules.Orders.Outbox;
using OrderProcessing.Modules.Orders.Persistence;
using Shouldly;
using Testcontainers.PostgreSql;

namespace OrderProcessing.IntegrationTests;

public sealed class OrdersPersistenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 07, 18, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GivenPostgreSqlTestcontainer_WhenOrderIsPersisted_ThenAggregateRoundTrips()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_TESTCONTAINERS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var postgreSql = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("order_processing_tests")
            .WithUsername("order_processing")
            .WithPassword("order_processing")
            .Build();

        await postgreSql.StartAsync();

        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(postgreSql.GetConnectionString())
            .Options;

        var orderId = Guid.NewGuid();

        await using (var context = new OrdersDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var order = Order.CreatePending(
                orderId,
                Guid.NewGuid(),
                [new OrderLine(Guid.NewGuid(), 2)],
                Now);

            order.Accept(
                new OrderPricing(
                    new Money(100m, "EUR"),
                    new Money(24m, "EUR"),
                    new Money(6m, "EUR")),
                Now.AddMinutes(1));

            context.Orders.Add(order);
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "orders.order-accepted.v1",
                Payload = $$"""{"orderId":"{{order.Id}}"}""",
                OccurredAt = Now.AddMinutes(1)
            });

            await context.SaveChangesAsync();
        }

        await using (var context = new OrdersDbContext(options))
        {
            var saved = await context.Orders
                .Include(order => order.Lines)
                .Include(order => order.Lifecycle)
                .SingleAsync(order => order.Id == orderId);

            saved.Status.ShouldBe(OrderStatus.Accepted);
            saved.Lines.ShouldHaveSingleItem().Quantity.ShouldBe(2);
            saved.Lifecycle.Count.ShouldBe(2);

            var pricing = saved.Pricing.ShouldNotBeNull();
            pricing.Total.ShouldBe(new Money(130m, "EUR"));

            var outboxCount = await context.OutboxMessages.CountAsync();
            outboxCount.ShouldBe(1);
        }
    }
}
