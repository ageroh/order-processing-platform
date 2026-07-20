using OrderProcessing.Modules.Orders.Application.Ports;

namespace OrderProcessing.Modules.Orders.Infrastructure;

internal sealed class PaymentAuthorizationPort : IPaymentAuthorizationPort
{
    public Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Payment authorization adapter is implemented in the create-order slice.");
    }

    public Task VoidAsync(
        string authorizationId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Payment authorization voiding is implemented with cancellation behavior.");
    }
}
