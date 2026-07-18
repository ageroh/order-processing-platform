namespace OrderProcessing.Modules.Orders.Domain;

internal sealed record OrderPricing
{
    public OrderPricing(Money subtotal, Money tax, Money additionalCharges)
    {
        EnsureSameCurrency(subtotal, tax, additionalCharges);

        Subtotal = subtotal;
        Tax = tax;
        AdditionalCharges = additionalCharges;
        Total = subtotal + tax + additionalCharges;
    }

    public Money Subtotal { get; }

    public Money Tax { get; }

    public Money AdditionalCharges { get; }

    public Money Total { get; }

    private static void EnsureSameCurrency(params Money[] values)
    {
        var currency = values[0].Currency;

        if (values.Any(value => !string.Equals(value.Currency, currency, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Order pricing values must use one currency.");
        }
    }
}
