// See https://aka.ms/new-console-template for more information
using CurrencyExchangeDifferenceCalc;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

Console.WriteLine("Hello, World!");

var confguration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("input.json", true, false)
    .AddCommandLine(args)
    .Build();

Input? input = null;
try
{
    input = confguration.Get<Input>();
}
catch (Exception ex)
{
    while (ex.InnerException is not null)
    {
        ex = ex.InnerException;
    }

    switch (ex)
    {
        case FormatException fex:
            LogInputErrors(new[] { fex.Message });
            return;
        default:
            throw;
    };
}

var validatonResult = new InputValidator().Validate(input);
if (!validatonResult.IsValid)
{
    LogInputErrors(validatonResult.Errors.Select(x => x.ErrorMessage));
    return;
}

var nbpClient = new NbpClient();

var currencies = await nbpClient.GetSupportedCurrenciesAsync(input!.IncomeDate);
if (!currencies.Any(x => x.Code.ToUpper() == input!.Currency.ToUpper()))
{
    LogInputErrors(new[] { $"Currency '{input!.Currency}' is not supported" });
    Console.WriteLine("Supported currencies:");
    foreach (var currency in currencies.OrderBy(x => x.Code))
    {
        Console.WriteLine($"{currency.Code} - {currency.Name}");
    }
    return;
}

var sellingRate = await nbpClient.GetExchangeRateAsync(input!.SellingDate, input.Currency);
var incomeRate = await nbpClient.GetExchangeRateAsync(input!.IncomeDate, input.Currency);

var output = new Output
{
    Amount = input.Amount,
    Currency = input.Currency,
    Invoice = input.InvoiceNumber,
    Selling = new Output.Row
    {
        Amount = sellingRate.Rate * input.Amount,
        Date = sellingRate.EffectiveDate,
        Table = sellingRate.TableNumber,
        Rate = sellingRate.Rate,
    },
    Income = new Output.Row
    {
        Amount = incomeRate.Rate * input.Amount,
        Date = incomeRate.EffectiveDate,
        Table = incomeRate.TableNumber,
        Rate = incomeRate.Rate,
    },
    Difference = incomeRate.Rate * input.Amount - sellingRate.Rate * input.Amount,
};

Console.WriteLine($"Exchange rate difference is {output.Difference.ToString("F4")} PLN");

Console.WriteLine("Generating PDF...");
var template = await File.ReadAllTextAsync("template.html");
var html = OutputGenerator.Generate(output, template);
var outputFile = "plik.pdf";

await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true
});
using (var page = await browser.NewPageAsync())
{
    await page.SetContentAsync(html);
    var result = await page.GetContentAsync();
    await page.PdfAsync(outputFile);
}


void LogInputErrors(IEnumerable<string> errors)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Input validation failed");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
    Console.ResetColor();
}
