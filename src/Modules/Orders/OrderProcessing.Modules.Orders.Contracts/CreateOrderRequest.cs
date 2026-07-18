namespace OrderProcessing.Modules.Orders.Contracts;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderLineRequest> Lines,
    AddressDto ShippingAddress,
    PaymentMethodReferenceDto PaymentMethod);

public sealed record CreateOrderLineRequest(Guid ProductId, int Quantity);

public sealed record AddressDto(
    string Line1,
    string? Line2,
    string City,
    string Region,
    string PostalCode,
    string CountryCode);

public sealed record PaymentMethodReferenceDto(string Provider, string Token);
