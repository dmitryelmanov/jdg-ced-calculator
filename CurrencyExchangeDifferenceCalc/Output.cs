namespace CurrencyExchangeDifferenceCalc;
#nullable disable warnings

internal sealed class Output
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    public string? Invoice { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public Row Selling { get; set; }
    public Row Income { get; set; }
    public decimal Difference { get; set; }


    internal sealed class Row
    {
        public DateOnly Date { get; set; }
        public string Table { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
