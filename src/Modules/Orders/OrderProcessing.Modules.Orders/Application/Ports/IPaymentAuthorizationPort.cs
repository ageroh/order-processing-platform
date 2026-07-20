using OrderProcessing.Modules.Orders.Domain;

namespace OrderProcessing.Modules.Orders.Application.Ports;

internal interface IPaymentAuthorizationPort
{
    Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken);

    Task VoidAsync(
        string authorizationId,
        CancellationToken cancellationToken);
}

internal sealed record PaymentAuthorizationRequest(
    Guid OrderId,
    Guid CustomerId,
    Money Amount,
    string PaymentProvider,
    string PaymentToken);

internal sealed record PaymentAuthorizationResult(
    bool Succeeded,
    string? AuthorizationId,
    string? FailureReason);
