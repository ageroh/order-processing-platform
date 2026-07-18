using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace OrderProcessing.IntegrationTests;

public sealed class ApiHealthTests
{
    [Fact]
    public async Task GivenApiHost_WhenHealthEndpointIsCalled_ThenResponseIsSuccess()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging => logging.ClearProviders());
            });

        using var client = application.CreateClient();

        using var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}
