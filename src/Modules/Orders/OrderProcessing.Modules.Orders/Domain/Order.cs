namespace OrderProcessing.Modules.Orders.Domain;

internal sealed class Order
{
    private readonly List<OrderDomainEvent> _domainEvents = [];
    private readonly List<OrderLifecycleEntry> _lifecycle = [];
    private readonly List<OrderLine> _lines = [];

    private Order(Guid id, Guid customerId, IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(id));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        Id = id;
        CustomerId = customerId;
        CreatedAt = createdAt;
        Status = OrderStatus.Pending;

        _lines.AddRange(lines);

        if (_lines.Count == 0)
        {
            throw new InvalidOperationException("An order must contain at least one product.");
        }

        AddLifecycle(OrderStatus.Pending, "Order created.", createdAt);
        AddDomainEvent(new OrderCreatedDomainEvent(Id, createdAt));
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? RejectedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? RejectionReason { get; private set; }

    public string? CancellationReason { get; private set; }

    public OrderPricing? Pricing { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public IReadOnlyCollection<OrderLifecycleEntry> Lifecycle => _lifecycle.AsReadOnly();

    public IReadOnlyCollection<OrderDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Order CreatePending(Guid id, Guid customerId, IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(lines);

        return new Order(id, customerId, lines, createdAt);
    }

    public void Accept(OrderPricing pricing, DateTimeOffset acceptedAt)
    {
        ArgumentNullException.ThrowIfNull(pricing);
        EnsureStatus(OrderStatus.Pending, "Only pending orders can be accepted.");

        Pricing = pricing;
        Status = OrderStatus.Accepted;
        AcceptedAt = acceptedAt;

        AddLifecycle(OrderStatus.Accepted, "Order accepted.", acceptedAt);
        AddDomainEvent(new OrderAcceptedDomainEvent(Id, acceptedAt));
    }

    public void Reject(string reason, DateTimeOffset rejectedAt)
    {
        EnsureStatus(OrderStatus.Pending, "Only pending orders can be rejected.");

        RejectionReason = RequireReason(reason, nameof(reason));
        Status = OrderStatus.Rejected;
        RejectedAt = rejectedAt;

        AddLifecycle(OrderStatus.Rejected, RejectionReason, rejectedAt);
        AddDomainEvent(new OrderRejectedDomainEvent(Id, RejectionReason, rejectedAt));
    }

    public void Cancel(string reason, DateTimeOffset cancelledAt)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Accepted))
        {
            throw new InvalidOperationException("Only pending or accepted orders can be cancelled.");
        }

        CancellationReason = RequireReason(reason, nameof(reason));
        Status = OrderStatus.Cancelled;
        CancelledAt = cancelledAt;

        AddLifecycle(OrderStatus.Cancelled, CancellationReason, cancelledAt);
        AddDomainEvent(new OrderCancelledDomainEvent(Id, CancellationReason, cancelledAt));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private static string RequireReason(string reason, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required.", parameterName);
        }

        return reason.Trim();
    }

    private void EnsureStatus(OrderStatus status, string message)
    {
        if (Status != status)
        {
            throw new InvalidOperationException(message);
        }
    }

    private void AddLifecycle(OrderStatus status, string description, DateTimeOffset occurredAt)
    {
        _lifecycle.Add(new OrderLifecycleEntry(status, description, occurredAt));
    }

    private void AddDomainEvent(OrderDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
