using OrderProcessing.Modules.Orders.Application.Ports;

namespace OrderProcessing.Modules.Orders.Infrastructure;

internal sealed class InventoryReservationPort : IInventoryReservationPort
{
    public Task<InventoryReservationResult> ReserveAsync(
        InventoryReservationRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Inventory reservation adapter is implemented in the create-order slice.");
    }

    public Task ReleaseAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Inventory reservation release is implemented with cancellation behavior.");
    }
}
