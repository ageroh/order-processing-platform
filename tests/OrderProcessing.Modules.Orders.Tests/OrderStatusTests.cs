using OrderProcessing.Modules.Orders.Domain;
using Shouldly;

namespace OrderProcessing.Modules.Orders.Tests;

public sealed class OrderStatusTests
{
    [Fact]
    public void Initial_Order_Statuses_Are_Deliberately_Small()
    {
        var statuses = Enum.GetNames<OrderStatus>();

        statuses.ShouldBe(["Pending", "Accepted", "Cancelled", "Rejected"]);
    }
}
