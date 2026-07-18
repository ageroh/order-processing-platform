namespace OrderProcessing.Modules.Orders.Contracts;

public sealed record OrderResponse(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    DateTimeOffset CreatedAt);
