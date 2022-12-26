// See https://aka.ms/new-console-template for more information
using CurrencyExchangeDifferenceCalc;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

Console.WriteLine("Currency exchange rate difference calculator");

var confguration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, false)
    .AddCommandLine(args)
    .Build();

AppSettings? settings = null;
try
{
    settings = confguration.Get<AppSettings>();
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
            Console.WriteLine("\r\nPress any key to exit...");
            Console.ReadKey();
            return;
        default:
            throw;
    };
}

settings ??= new AppSettings();
confguration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, false)
    .AddJsonFile(settings.InputFileName, true, false)
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
            Console.WriteLine("\r\nPress any key to exit...");
            Console.ReadKey();
            return;
        default:
            throw;
    };
}

var validatonResult = new InputValidator().Validate(input);
if (!validatonResult.IsValid)
{
    LogInputErrors(validatonResult.Errors.Select(x => x.ErrorMessage));
    Console.WriteLine("\r\nPress any key to exit...");
    Console.ReadKey();
    return;
}

var nbpClient = new NbpClient(settings.NbpApiUrl);

var currencies = await nbpClient.GetSupportedCurrenciesAsync(input!.IncomeDate);
if (!currencies.Any(x => x.Code.ToUpper() == input!.Currency.ToUpper()))
{
    LogInputErrors(new[] { $"Currency '{input!.Currency}' is not supported" });
    Console.WriteLine("Supported currencies:");
    foreach (var currency in currencies.OrderBy(x => x.Code))
    {
        Console.WriteLine($"{currency.Code} - {currency.Name}");
    }

    Console.WriteLine("\r\nPress any key to exit...");
    Console.ReadKey();
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
var template = await File.ReadAllTextAsync(settings.TemplateFileName);
var html = OutputGenerator.Generate(output, template);

await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true
});
using var page = await browser.NewPageAsync();
await page.SetContentAsync(html);
var result = await page.GetContentAsync();
await page.PdfAsync(settings.OutputFileName);

Console.WriteLine("\r\nPress any key to exit...");
Console.ReadKey();

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
