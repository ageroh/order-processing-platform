namespace OrderProcessing.Modules.Orders.Application;

public interface IOrdersCommandHandler
{
    Task<CreateOrderResult> CreateOrderAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken);
}
