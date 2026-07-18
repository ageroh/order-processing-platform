using OrderProcessing.Modules.Orders.Domain;
using Shouldly;

namespace OrderProcessing.Modules.Orders.Tests;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 07, 18, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreatePending_Requires_At_Least_One_Line()
    {
        var act = () => Order.CreatePending(Guid.NewGuid(), Guid.NewGuid(), [], Now);

        var exception = Should.Throw<InvalidOperationException>(act);

        exception.Message.ShouldBe("An order must contain at least one product.");
    }

    [Fact]
    public void CreatePending_Records_Initial_Lifecycle_And_Domain_Event()
    {
        var order = CreatePendingOrder();

        order.Status.ShouldBe(OrderStatus.Pending);
        order.Lines.ShouldHaveSingleItem();

        var lifecycleEntry = order.Lifecycle.ShouldHaveSingleItem();
        lifecycleEntry.Status.ShouldBe(OrderStatus.Pending);
        lifecycleEntry.Description.ShouldBe("Order created.");
        lifecycleEntry.OccurredAt.ShouldBe(Now);

        order.DomainEvents.ShouldContain(domainEvent => domainEvent is OrderCreatedDomainEvent);
    }

    [Fact]
    public void Accept_Moves_Pending_Order_To_Accepted_And_Stores_Pricing()
    {
        var order = CreatePendingOrder();
        var pricing = new OrderPricing(
            new Money(100m, "EUR"),
            new Money(24m, "EUR"),
            new Money(6m, "EUR"));

        order.Accept(pricing, Now.AddMinutes(1));

        order.Status.ShouldBe(OrderStatus.Accepted);

        var pricingSnapshot = order.Pricing.ShouldNotBeNull();
        pricingSnapshot.Total.ShouldBe(new Money(130m, "EUR"));

        order.AcceptedAt.ShouldBe(Now.AddMinutes(1));
        order.Lifecycle.ShouldContain(entry => entry.Status == OrderStatus.Accepted);
        order.DomainEvents.ShouldContain(domainEvent => domainEvent is OrderAcceptedDomainEvent);
    }

    [Fact]
    public void Reject_Moves_Pending_Order_To_Rejected_With_Reason()
    {
        var order = CreatePendingOrder();

        order.Reject("Inventory unavailable.", Now.AddMinutes(1));

        order.Status.ShouldBe(OrderStatus.Rejected);
        order.RejectionReason.ShouldBe("Inventory unavailable.");
        order.Lifecycle.ShouldContain(entry =>
            entry.Status == OrderStatus.Rejected && entry.Description == "Inventory unavailable.");
        order.DomainEvents.ShouldContain(domainEvent => domainEvent is OrderRejectedDomainEvent);
    }

    [Fact]
    public void Cancel_Allows_Full_Cancellation_When_Order_Is_Pending()
    {
        var order = CreatePendingOrder();

        order.Cancel("Customer requested cancellation.", Now.AddMinutes(1));

        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.CancellationReason.ShouldBe("Customer requested cancellation.");
        order.DomainEvents.ShouldContain(domainEvent => domainEvent is OrderCancelledDomainEvent);
    }

    [Fact]
    public void Cancel_Allows_Full_Cancellation_When_Order_Is_Accepted()
    {
        var order = CreateAcceptedOrder();

        order.Cancel("Customer requested cancellation.", Now.AddMinutes(2));

        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.CancelledAt.ShouldBe(Now.AddMinutes(2));
    }

    [Fact]
    public void Cancel_Does_Not_Allow_Cancelling_Rejected_Order()
    {
        var order = CreatePendingOrder();
        order.Reject("Payment authorization failed.", Now.AddMinutes(1));

        var act = () => order.Cancel("Customer requested cancellation.", Now.AddMinutes(2));

        var exception = Should.Throw<InvalidOperationException>(act);

        exception.Message.ShouldBe("Only pending or accepted orders can be cancelled.");
    }

    [Fact]
    public void OrderLine_Requires_Positive_Quantity()
    {
        var act = () => new OrderLine(Guid.NewGuid(), 0);

        var exception = Should.Throw<ArgumentOutOfRangeException>(act);

        exception.ParamName.ShouldBe("quantity");
    }

    [Fact]
    public void OrderPricing_Requires_One_Currency()
    {
        var act = () => new OrderPricing(
                new Money(100m, "EUR"),
                new Money(24m, "USD"),
                new Money(6m, "EUR"));

        var exception = Should.Throw<InvalidOperationException>(act);

        exception.Message.ShouldBe("Order pricing values must use one currency.");
    }

    private static Order CreatePendingOrder()
    {
        return Order.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new OrderLine(Guid.NewGuid(), 2)],
            Now);
    }

    private static Order CreateAcceptedOrder()
    {
        var order = CreatePendingOrder();
        order.Accept(
            new OrderPricing(new Money(100m, "EUR"), new Money(24m, "EUR"), new Money(6m, "EUR")),
            Now.AddMinutes(1));

        return order;
    }
}
