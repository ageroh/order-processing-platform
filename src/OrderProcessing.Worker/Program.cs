using MassTransit;
using OrderProcessing.Worker;
using OrderProcessing.Modules.Orders;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();
    configurator.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
});

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OrderProcessing.Worker"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("OrderProcessing")
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("OrderProcessing")
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

builder.Services.AddOrdersModule(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
