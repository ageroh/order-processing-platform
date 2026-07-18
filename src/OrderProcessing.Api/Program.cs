using MassTransit;
using OrderProcessing.Api.Observability;
using OrderProcessing.Modules.Orders;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();
    configurator.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
});

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OrderProcessing.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("OrderProcessing")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("OrderProcessing")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

builder.Services.AddHealthChecks();
builder.Services.AddOrdersModule(builder.Configuration, mvcBuilder);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;
