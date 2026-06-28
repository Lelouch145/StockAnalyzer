namespace StockAnalyzer.Api.Services;

using System.ComponentModel;
using System.Globalization;
using System.Net.Quic;
using System.Text.Json;
using StockAnalyzer.Api.Models;



public class StockService
{
    // Di för config och http
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    // contuctor för DI
    public StockService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    // olika typer av parsing för att undivka göra koden mer repetetiv
    private decimal ParseDecimal(string value)
    {
        decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal result);
        return result;
    }
    private long ParseLong(string value)
    {
        long.TryParse(value, out long result);
        return result;
    }
    private DateOnly ParsedDate(string value)
    {
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, out DateOnly result);
        return result;
    }
    // Hämtar Stock historik från extärn api. Ticker är aktire förkortning
    public async Task<StockHistoryResult> GetStockHistoryAsync(string ticker)
    {
        
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return new StockHistoryResult
            {
                ErrorType = StockApiError.InvalidTicker,
                Message = "Ticker cannot be empty"  
            };
        }
        // _configuration läser apikey som står i AlphaVantage
        string? apiKey = _configuration["AlphaVantage:ApiKey"];
        // Skickar ApiKey och ticker till extärna api för aktie data
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={ticker}&apikey={apiKey}";
        // Väntar på response från extärna api
        var response = await _httpClient.GetAsync(url);
        // Om extärna api inte ger en success status kod
        if (!response.IsSuccessStatusCode)
        {
            // returnerar StockHistoryResult med Enum och en meddelande
            return new StockHistoryResult
            {
                ErrorType = StockApiError.ExternalApiError,
                Message = "External api error"
            };
        }
        // väntar på response så den innehåll skas läsas som sträng
        var json = await response.Content.ReadAsStringAsync();

        // av serialiserar json så den matchar AlphaVantageJson annars går det inte
        var answer = JsonSerializer.Deserialize<AlphaVantageJson>(json);
        if (answer == null)
        {
            return new StockHistoryResult
            {
                ErrorType = StockApiError.NoData,
                Message = "No data found"
            };
        }
        // Skapar lista som är baserat på StockHistory
        List<StockHistory> stockHistory = new List<StockHistory>();
        if (answer.Daily.Count == 0 )
        {
            return new StockHistoryResult
            {
                ErrorType = StockApiError.NoData,
                Message = "No data found"
            };
        }
        // for-loop som tar daglig data från answer
        foreach (var data in answer.Daily)
        {
            // Skapar en ny StockHistory och lägger till data som den behöver
            StockHistory historyDay = new StockHistory
            {
                Date = ParsedDate(data.Key),
                Open = ParseDecimal(data.Value.Open),
                High = ParseDecimal(data.Value.High),
                Low = ParseDecimal(data.Value.Low),
                Close = ParseDecimal(data.Value.Close),
                Volume = ParseLong(data.Value.Volume),
            };
            // Sparar den till listan
            stockHistory.Add(historyDay);
        }
        var stockHistories = stockHistory.OrderByDescending(t => t.Date).ToList();
        // Om allt går bra så returnerar listan och assignar den som History
        // Och History är baserat på StockHistory modellen.
        return new StockHistoryResult
        {
            History = stockHistories,
            ErrorType = StockApiError.None
        };

    }
    // Använder informationen som finns i history listan
    public StockResult StockResult(List<StockHistory> history, string ticker)
    {
        // Använder StockResult modellen för att kunna mappa innehållet
        StockResult result = new StockResult();

        var latestDay = history[0];
        var previousDay = history[1];

        result.Ticker = ticker;
        result.Price = latestDay.Close;
        result.Open = latestDay.Open;
        result.High = latestDay.High;
        result.Low = latestDay.Low;
        result.Volume = latestDay.Volume;
        result.PreviousClose = previousDay.Close;
        result.Change = latestDay.Close - previousDay.Close;
        result.ChangePercent = (latestDay.Close - previousDay.Close) / previousDay.Close * 100;
        result.LatestTradingDay =latestDay.Date;
        result.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
        // Efter att jag har mappat allt så returnerar jag det
        return result;
    }
}