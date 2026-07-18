namespace OrderProcessing.Modules.Orders.Domain;

internal interface OrderDomainEvent
{
    Guid OrderId { get; }

    DateTimeOffset OccurredAt { get; }
}

internal sealed record OrderCreatedDomainEvent(Guid OrderId, DateTimeOffset OccurredAt) : OrderDomainEvent;

internal sealed record OrderAcceptedDomainEvent(Guid OrderId, DateTimeOffset OccurredAt) : OrderDomainEvent;

internal sealed record OrderRejectedDomainEvent(Guid OrderId, string Reason, DateTimeOffset OccurredAt) : OrderDomainEvent;

internal sealed record OrderCancelledDomainEvent(Guid OrderId, string Reason, DateTimeOffset OccurredAt) : OrderDomainEvent;
