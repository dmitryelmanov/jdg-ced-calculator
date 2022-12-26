namespace CurrencyExchangeDifferenceCalc;
#nullable disable warnings

internal sealed class Input
{
    public string? InvoiceNumber { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateOnly SellingDate { get; set; }
    public DateOnly IncomeDate { get; set; }
}
