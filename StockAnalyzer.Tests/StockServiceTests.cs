namespace StockAnalyzer.Tests;

using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockAnalyzer.Api.Models;
using System.Net;
using System.Text;
using StockAnalyzer.Api.Services;
using Microsoft.Extensions.Configuration;

public class StockServiceTests
{
    [Fact]
    public async Task GetStockHistoryAsync_ValidResponse_ReturnsHistory()
    {
        string json = """
        {

                "Time Series (Daily)": {
                "2026-06-26": {
                    "1. open": "120",
                    "2. high": "160",
                    "3. low": "80",
                    "4. close": "115",
                    "5. volume": "30300"
                },
                "2026-06-27": {
                    "1. open": "110",
                    "2. high": "120",
                    "3. low": "90",
                    "4. close": "100",
                    "5. volume": "20200"
                }
        
            }
        }
        """;

        HttpResponseMessage fakeData = new HttpResponseMessage(HttpStatusCode.OK);
        fakeData.Content = new StringContent(json, Encoding.UTF8, "application/json");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(fakeData);
        HttpClient httpClient = new HttpClient(handler);
        var config = new ConfigurationBuilder().Build();

        StockService service = new StockService(httpClient, config);

        var result = await service.GetStockHistoryAsync("Test");
        Assert.Equal(2, result.History.Count);
        Assert.Equal(StockApiError.None, result.ErrorType);
        Assert.Equal(new DateOnly(2026, 6, 27), result.History[0].Date);
        Assert.Equal(new DateOnly(2026, 6, 26), result.History[1].Date);
        Assert.Equal(110, result.History[0].Open);
        Assert.Equal(120, result.History[0].High);
        Assert.Equal(90, result.History[0].Low);
        Assert.Equal(100, result.History[0].Close);
        Assert.Equal(20_200, result.History[0].Volume);

    }
}