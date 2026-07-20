namespace OrderProcessing.Modules.Orders.Application.Ports;

internal interface IInventoryReservationPort
{
    Task<InventoryReservationResult> ReserveAsync(
        InventoryReservationRequest request,
        CancellationToken cancellationToken);

    Task ReleaseAsync(
        Guid reservationId,
        CancellationToken cancellationToken);
}

internal sealed record InventoryReservationRequest(
    Guid OrderId,
    IReadOnlyCollection<InventoryReservationLine> Lines);

internal sealed record InventoryReservationLine(Guid ProductId, int Quantity);

internal sealed record InventoryReservationResult(
    bool Succeeded,
    Guid? ReservationId,
    string? FailureReason);
