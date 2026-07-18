using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Modules.Orders.HttpModels;

namespace OrderProcessing.Modules.Orders.Controllers;

[ApiController]
[Route("orders")]
internal sealed class OrdersController : ControllerBase
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = CorrelationIdHeader)] string? correlationId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails
        {
            Title = "Create order is not implemented yet.",
            Detail = "Slice 1 exposes the module controller contract. The command handler arrives in the application slice.",
            Status = StatusCodes.Status501NotImplemented
        });
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetOrder(
        Guid orderId,
        [FromHeader(Name = CorrelationIdHeader)] string? correlationId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails
        {
            Title = "Get order is not implemented yet.",
            Detail = "Slice 1 exposes the module controller contract. The query handler arrives in the application slice.",
            Status = StatusCodes.Status501NotImplemented
        });
    }

    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult CancelOrder(
        Guid orderId,
        [FromHeader(Name = CorrelationIdHeader)] string? correlationId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails
        {
            Title = "Cancel order is not implemented yet.",
            Detail = "Slice 1 exposes the module controller contract. The cancellation policy arrives in the domain slice.",
            Status = StatusCodes.Status501NotImplemented
        });
    }

    [HttpGet("{orderId:guid}/lifecycle")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderLifecycleEventResponse>), StatusCodes.Status200OK)]
    public IActionResult GetLifecycle(
        Guid orderId,
        [FromHeader(Name = CorrelationIdHeader)] string? correlationId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails
        {
            Title = "Get order lifecycle is not implemented yet.",
            Detail = "Slice 1 exposes the module controller contract. Lifecycle persistence arrives in the persistence slice.",
            Status = StatusCodes.Status501NotImplemented
        });
    }
}
