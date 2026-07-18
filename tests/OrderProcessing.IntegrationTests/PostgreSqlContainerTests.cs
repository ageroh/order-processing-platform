using Shouldly;
using Testcontainers.PostgreSql;

namespace OrderProcessing.IntegrationTests;

public sealed class PostgreSqlContainerTests
{
    [Fact]
    public async Task GivenTestcontainersEnabled_WhenPostgreSqlStarts_ThenConnectionStringIsAvailable()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_TESTCONTAINERS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var postgreSql = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("order_processing_tests")
            .WithUsername("order_processing")
            .WithPassword("order_processing")
            .Build();

        await postgreSql.StartAsync();

        postgreSql.GetConnectionString().ShouldNotBeNullOrWhiteSpace();
    }
}
