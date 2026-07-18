using OrderProcessing.Modules.Orders.Domain;
using Shouldly;

namespace OrderProcessing.Modules.Orders.Tests;

public sealed class OrderStatusTests
{
    [Fact]
    public void GivenOrderStatus_WhenEnumIsRead_ThenInitialLifecycleIsDeliberatelySmall()
    {
        var statuses = Enum.GetNames<OrderStatus>();

        statuses.ShouldBe(["Pending", "Accepted", "Cancelled", "Rejected"]);
    }
}
