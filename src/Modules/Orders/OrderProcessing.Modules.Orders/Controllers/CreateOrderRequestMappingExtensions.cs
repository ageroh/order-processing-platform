using OrderProcessing.Modules.Orders.Application;
using OrderProcessing.Modules.Orders.Contracts;

namespace OrderProcessing.Modules.Orders.Controllers;

internal static class CreateOrderRequestMappingExtensions
{
    public static CreateOrderCommand ToCommand(
        this CreateOrderRequest request,
        string? idempotencyKey)
    {
        return new CreateOrderCommand(
            request.CustomerId,
            request.Lines
                .Select(line => new CreateOrderLineCommand(line.ProductId, line.Quantity))
                .ToArray(),
            new ShippingAddressCommand(
                request.ShippingAddress.Line1,
                request.ShippingAddress.Line2,
                request.ShippingAddress.City,
                request.ShippingAddress.Region,
                request.ShippingAddress.PostalCode,
                request.ShippingAddress.CountryCode),
            new PaymentMethodCommand(
                request.PaymentMethod.Provider,
                request.PaymentMethod.Token),
            idempotencyKey);
    }
}
