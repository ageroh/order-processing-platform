using OrderProcessing.Modules.Orders.Domain;

namespace OrderProcessing.Modules.Orders.Tests;

public sealed class OrderStatusTests
{
    [Fact]
    public void Initial_Order_Statuses_Are_Deliberately_Small()
    {
        var statuses = Enum.GetNames<OrderStatus>();

        Assert.Equal(["Pending", "Accepted", "Cancelled", "Rejected"], statuses);
    }
}
