using OrderProcessing.Modules.Orders.Application.Ports;
using OrderProcessing.Modules.Orders.Persistence;

namespace OrderProcessing.Modules.Orders.Application;

internal sealed class OrdersCommandHandler(
    OrdersDbContext dbContext,
    IInventoryReservationPort inventory,
    IPricingPort pricing,
    IPaymentAuthorizationPort payments) : IOrdersCommandHandler
{
    public Task<CreateOrderResult> CreateOrderAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        _ = dbContext;
        _ = inventory;
        _ = pricing;
        _ = payments;

        throw new NotImplementedException("Create order command handling is the first delivery slice.");
    }
}
