using OrderProcessing.Modules.Orders.Domain;

namespace OrderProcessing.Modules.Orders.Application;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderLineCommand> Lines,
    ShippingAddressCommand ShippingAddress,
    PaymentMethodCommand PaymentMethod,
    string? IdempotencyKey);

public sealed record CreateOrderLineCommand(Guid ProductId, int Quantity);

public sealed record ShippingAddressCommand(
    string Line1,
    string? Line2,
    string City,
    string Region,
    string PostalCode,
    string CountryCode);

public sealed record PaymentMethodCommand(string Provider, string Token);

internal sealed record PricedOrderLine(Guid ProductId, int Quantity, Money UnitPrice);
