using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace OrderProcessing.IntegrationTests;

public sealed class OrdersModuleControllerTests
{
    [Fact]
    public async Task Orders_Module_Controller_Is_Discovered_By_The_Api_Host()
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
}
