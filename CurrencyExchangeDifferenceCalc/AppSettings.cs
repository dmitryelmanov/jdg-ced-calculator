namespace CurrencyExchangeDifferenceCalc;

internal sealed class AppSettings
{
    public string InputFileName { get; set; } = "input.json";
    public string TemplateFileName { get; set; } = "template.html";
    public string OutputFileName { get; set; } = "plik.pdf";
    public string NbpApiUrl { get; set; } = "https://api.nbp.pl";
}
