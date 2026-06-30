using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Text;
using StockAnalyzer.Tests;
using Microsoft.Extensions.Configuration;
using StockAnalyzer.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
public class EndpointIntegrationTests
{

    [Fact]
    public async Task PlaceHolderName()
    {
       var test = new WebApplicationFactory<Program>();
       // Bryggan mellan Program.cs och Testappen
       var placeholder = test.CreateClient();


       var result = await placeholder.GetAsync("/route");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);

    
    }

    [Fact]
    public async Task GetStock_ValidTicker_ReturnsOk()
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
        HttpClient fakeAlphaVantageClient = new HttpClient(handler);

        var config = new ConfigurationBuilder().Build();

        StockService stockService = new StockService(fakeAlphaVantageClient, config);
        
        var web = new WebApplicationFactory<Program>();

        var customWeb = web.WithWebHostBuilder(builder =>
        {
           builder.ConfigureTestServices(services =>
           {
               services.RemoveAll<StockService>();
               services.AddSingleton(stockService);
           }); 
        });
        var testClient = customWeb.CreateClient();

        var response = await testClient.GetAsync("/stock/TEST");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);



    }

}