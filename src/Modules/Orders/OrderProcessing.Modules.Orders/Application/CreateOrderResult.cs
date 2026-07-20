namespace OrderProcessing.Modules.Orders.Application;

public sealed record CreateOrderResult(
    Guid OrderId,
    string Status,
    string? RejectionReason);
