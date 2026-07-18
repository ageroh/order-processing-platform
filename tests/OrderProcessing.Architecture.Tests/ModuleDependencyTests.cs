using OrderProcessing.Modules.Orders;

namespace OrderProcessing.Architecture.Tests;

public sealed class ModuleDependencyTests
{
    [Fact]
    public void Orders_Module_Should_Expose_Only_The_Service_Registrar_Publicly()
    {
        var publicTypes = typeof(OrdersModuleServiceRegistrar).Assembly
            .GetExportedTypes()
            .Where(type => type.Namespace?.StartsWith("OrderProcessing.Modules.Orders", StringComparison.Ordinal) == true)
            .Select(type => type.FullName!)
            .ToArray();

        var publicType = Assert.Single(publicTypes);
        Assert.Equal(typeof(OrdersModuleServiceRegistrar).FullName, publicType);
    }
}
