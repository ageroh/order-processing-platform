namespace OrderProcessing.Modules.Orders.Domain;

internal sealed class OrderLifecycleEntry
{
    private OrderLifecycleEntry()
    {
        Description = string.Empty;
    }

    public OrderLifecycleEntry(OrderStatus status, string description, DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Lifecycle description is required.", nameof(description));
        }

        Id = Guid.NewGuid();
        Status = status;
        Description = description.Trim();
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public OrderStatus Status { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
