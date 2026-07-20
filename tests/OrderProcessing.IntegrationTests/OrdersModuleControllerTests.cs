using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using OrderProcessing.Modules.Orders.Contracts;
using Shouldly;

namespace OrderProcessing.IntegrationTests;

public sealed class OrdersModuleControllerTests
{
    [Fact]
    public async Task GivenApiHost_WhenOrderIsRequested_ThenOrdersModuleControllerIsDiscovered()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging => logging.ClearProviders());
            });

        using var client = application.CreateClient();

        using var response = await client.GetAsync($"/orders/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
    }

    [Fact]
    public async Task GivenCreateOrderEndpoint_WhenCommandHandlerIsPlaceholder_ThenNotImplementedIsReturned()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging => logging.ClearProviders());
            });

        using var client = application.CreateClient();

        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            [new CreateOrderLineRequest(Guid.NewGuid(), 2)],
            new AddressDto(
                "1 Test Street",
                null,
                "Athens",
                "Attica",
                "10000",
                "GR"),
            new PaymentMethodReferenceDto("test-provider", "payment-token"));

        using var response = await client.PostAsJsonAsync("/orders", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
    }
}
