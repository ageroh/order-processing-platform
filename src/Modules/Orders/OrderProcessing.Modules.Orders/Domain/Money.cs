namespace OrderProcessing.Modules.Orders.Domain;

internal readonly record struct Money
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Money amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money operator +(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Cannot add money values with different currencies.");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }
}
