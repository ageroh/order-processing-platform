namespace OrderProcessing.Modules.Orders.Domain;

internal sealed class OrderLine
{
    private OrderLine()
    {
    }

    public OrderLine(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }
}
