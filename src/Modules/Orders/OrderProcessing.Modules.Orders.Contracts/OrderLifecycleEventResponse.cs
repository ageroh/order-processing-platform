namespace OrderProcessing.Modules.Orders.Contracts;

public sealed record OrderLifecycleEventResponse(
    Guid EventId,
    Guid OrderId,
    string Status,
    string Reason,
    DateTimeOffset OccurredAt);
