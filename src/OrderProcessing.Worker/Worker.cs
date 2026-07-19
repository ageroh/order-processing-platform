namespace OrderProcessing.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Skeleton placeholder: replace this heartbeat with outbox dispatch, MassTransit
        // publishing, retries, failure visibility, and integration workflow processing.
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("OrderProcessing.Worker heartbeat at {Time}", DateTimeOffset.UtcNow);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
