using System.Net.Http.Json;
using System.Text.Json.Serialization;
using static CurrencyExchangeDifferenceCalc.NbpClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyExchangeDifferenceCalc;

internal sealed class NbpClient
{
    private static readonly string PathTemplate = "api/exchangerates/rates/a/{0}/{1}?format=json";
    private static readonly string CurrenciesQueryTemplate = "api/exchangerates/tables/a/{0}?format=json";
    private readonly HttpClient _httpClient;

    public NbpClient()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.nbp.pl");
    }

    public async Task<ExchangeRate> GetExchangeRateAsync(DateOnly date, string currency)
    {
        var query = string.Format(PathTemplate, currency, date.ToString("yyyy-MM-dd"));
        var response = await _httpClient.GetFromJsonAsync<CurrencyRateResponse>(query);
        return response!.Rates.First();
    }

    public async Task<Currency[]> GetSupportedCurrenciesAsync(DateOnly date)
    {
        var query = string.Format(CurrenciesQueryTemplate, date.ToString("yyyy-MM-dd"));
        var response = await _httpClient.GetFromJsonAsync<TableResponse[]>(query);
        return response!.First().Rates;
    }


#nullable disable warnings
    public sealed class Currency
    {
        [JsonPropertyName("currency")]
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public sealed class ExchangeRate
    {
        [JsonPropertyName("no")]
        public string TableNumber { get; set; }
        public DateOnly EffectiveDate { get; set; }
        [JsonPropertyName("mid")]
        public decimal Rate { get; set; }
    }

    internal sealed class CurrencyRateResponse
    {
        public string Table { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public ExchangeRate[] Rates { get; set; }
    }

    internal sealed class TableResponse
    {
        public string Table { get; set; }
        [JsonPropertyName("no")]
        public string TableNumber { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public Currency[] Rates { get; set; }
    }
}
