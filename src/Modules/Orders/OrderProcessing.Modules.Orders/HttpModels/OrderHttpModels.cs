namespace OrderProcessing.Modules.Orders.HttpModels;

internal sealed record CreateOrderRequest(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderLineRequest> Lines,
    AddressDto ShippingAddress,
    PaymentMethodReferenceDto PaymentMethod);

internal sealed record CreateOrderLineRequest(Guid ProductId, int Quantity);

internal sealed record AddressDto(
    string Line1,
    string? Line2,
    string City,
    string Region,
    string PostalCode,
    string CountryCode);

internal sealed record PaymentMethodReferenceDto(string Provider, string Token);

internal sealed record OrderResponse(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    DateTimeOffset CreatedAt);

internal sealed record OrderLifecycleEventResponse(
    Guid EventId,
    Guid OrderId,
    string Status,
    string Reason,
    DateTimeOffset OccurredAt);
