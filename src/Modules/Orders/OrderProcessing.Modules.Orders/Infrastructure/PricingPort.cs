using OrderProcessing.Modules.Orders.Application.Ports;

namespace OrderProcessing.Modules.Orders.Infrastructure;

internal sealed class PricingPort : IPricingPort
{
    public Task<PricingResult> PriceAsync(
        PricingRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Pricing adapter is implemented in the create-order slice.");
    }
}
