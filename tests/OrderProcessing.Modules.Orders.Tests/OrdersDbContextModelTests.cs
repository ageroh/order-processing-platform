using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderProcessing.Modules.Orders.Domain;
using OrderProcessing.Modules.Orders.Outbox;
using OrderProcessing.Modules.Orders.Persistence;
using Shouldly;

namespace OrderProcessing.Modules.Orders.Tests;

public sealed class OrdersDbContextModelTests
{
    [Fact]
    public void GivenOrdersModel_WhenMetadataIsBuilt_ThenModuleOwnedTablesUseOrdersSchema()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql("Host=localhost;Database=orders_model;Username=orders;Password=orders")
            .Options;

        using var context = new OrdersDbContext(options);

        var orderEntity = context.Model.FindEntityType(typeof(Order)).ShouldNotBeNull();
        orderEntity.GetSchema().ShouldBe("orders");
        orderEntity.GetTableName().ShouldBe("orders");
        orderEntity.FindProperty(nameof(Order.Version)).ShouldNotBeNull().IsConcurrencyToken.ShouldBeTrue();
        orderEntity.FindNavigation(nameof(Order.DomainEvents)).ShouldBeNull();

        var lineEntity = context.Model.GetEntityTypes()
            .Single(entity => entity.ClrType == typeof(OrderLine));
        lineEntity.GetSchema().ShouldBe("orders");
        lineEntity.GetTableName().ShouldBe("order_lines");

        var lifecycleEntity = context.Model.GetEntityTypes()
            .Single(entity => entity.ClrType == typeof(OrderLifecycleEntry));
        lifecycleEntity.GetSchema().ShouldBe("orders");
        lifecycleEntity.GetTableName().ShouldBe("order_lifecycle_events");

        var outboxEntity = context.Model.FindEntityType(typeof(OutboxMessage)).ShouldNotBeNull();
        outboxEntity.GetSchema().ShouldBe("orders");
        outboxEntity.GetTableName().ShouldBe("outbox_messages");
    }
}
