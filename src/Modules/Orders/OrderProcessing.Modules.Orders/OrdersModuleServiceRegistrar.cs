using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessing.Modules.Orders.Application;
using OrderProcessing.Modules.Orders.Application.Ports;
using OrderProcessing.Modules.Orders.Infrastructure;
using OrderProcessing.Modules.Orders.Persistence;

namespace OrderProcessing.Modules.Orders;

public static class OrdersModuleServiceRegistrar
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IMvcBuilder? mvcBuilder = null)
    {
        var connectionString = configuration.GetConnectionString("Orders")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=order_processing;Username=order_processing;Password=order_processing";

        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrdersCommandHandler, OrdersCommandHandler>();
        services.AddScoped<IInventoryReservationPort, InventoryReservationPort>();
        services.AddScoped<IPricingPort, PricingPort>();
        services.AddScoped<IPaymentAuthorizationPort, PaymentAuthorizationPort>();

        if (mvcBuilder is not null)
        {
            mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(typeof(OrdersModuleServiceRegistrar).Assembly));
        }

        return services;
    }
}
