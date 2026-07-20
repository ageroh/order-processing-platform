using OrderProcessing.Modules.Orders.Domain;

namespace OrderProcessing.Modules.Orders.Application.Ports;

internal interface IPricingPort
{
    Task<PricingResult> PriceAsync(
        PricingRequest request,
        CancellationToken cancellationToken);
}

internal sealed record PricingRequest(
    Guid CustomerId,
    IReadOnlyCollection<PricingLine> Lines,
    ShippingAddressCommand ShippingAddress);

internal sealed record PricingLine(Guid ProductId, int Quantity);

internal sealed record PricingResult(
    OrderPricing Pricing,
    IReadOnlyCollection<PricedOrderLine> Lines);
