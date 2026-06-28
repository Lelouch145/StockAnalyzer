namespace StockAnalyzer.Tests;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualBasic;
using NuGet.Frameworks;
using StockAnalyzer.Api.Models;
using StockAnalyzer.Api.Services;
using Microsoft.Extensions.Configuration;
public class UnitTest1
{

    [Theory]
    [InlineData(3, "Bullish")]
    [InlineData(2, "Neutral")]
    [InlineData(-3, "Bearish")]
    public void DetermineSignal_ReturnsCorrectSignal(int score, string expectedSignal)
    {
        Analyze analyze = new Analyze();
        // score är int värden i InlineData
        var result = analyze.DetermineSignal(score);
        // Jämför två värden. Den första är Expected och den andra är actual
        // expectedSignal är Theory strängarna 
        Assert.Equal(expectedSignal, result);
    }
    [Theory]
    [InlineData(4, "High")]
    [InlineData(-4, "High")]
    [InlineData(3, "Medium")]
    [InlineData(-3, "Medium")]
    [InlineData(1, "Low")]
    [InlineData(0, "Low")]
    public void DetermineRiskLevel_ReturnsCorrectRiskLevel(decimal changePercent, string expectedRiskLevel)
    {
        StockResult stockResult = new StockResult();
        Analyze analyze = new Analyze();
        stockResult.ChangePercent = changePercent;
        var placeHolder = analyze.DetermineRiskLevel(stockResult);
        Assert.Equal(expectedRiskLevel, placeHolder);


    }
    [Theory]
    [InlineData(10, 20, 30, 3, 20)]
    [InlineData(10, 20, 30, 4, null)]
    public void CalculateMovingAverage(int close1, int close2, int close3, int period, int? expectedAverage)
    {
        List<StockHistory> list = new List<StockHistory>()
        {
            new StockHistory
            {
                Close = close1,
            },
            new StockHistory
            {
                Close = close2
            },
            new StockHistory
            {
                Close = close3
            }
        };
        Analyze analyze = new Analyze();
        var result = analyze.CalculateMovingAverage(list, period);
        Assert.Equal(expectedAverage, result);

    }
    [Theory]
    [InlineData(120, 110, 100, 3, 20)]
    [InlineData(120, 110, 100, 4, null)]
    public void CalculateMovingTrend(int close1, int close2, int close3, int period, int? expectedTrend)
    {
        List<StockHistory> list = new List<StockHistory>()
        {
            new StockHistory
            {
                Close = close1,
            },
            new StockHistory
            {
                Close = close2
            },
            new StockHistory
            {
                Close = close3
            }
        };
        Analyze analyze = new Analyze();
        var result = analyze.CalculateMovingTrend(list, period);
        Assert.Equal(expectedTrend, result);
    }

    [Theory]
    [InlineData(120, 110, 0, 3)]
    public void CalculateMovingTrend_WhenPreviousCloseIsZero_ReturnsNull(int close1, int close2, int close3, int period)
    {
        Analyze analyze = new Analyze();
       
        List<StockHistory> list = new List<StockHistory>()

        {
            new StockHistory
            {
                Close = close1
            },
            new StockHistory
            {
                Close = close2
            },
            new StockHistory
            {
                Close = close3
            }
        };

        var result = analyze.CalculateMovingTrend(list, period);
        Assert.Null(result);
    }
    [Theory]
    [InlineData(90, 100, 2, -10)]
    [InlineData(100, 100, 2, 0)]
    public void CalculateMovingTrend_ReturnsNegativeOrZeroTrend(int latestClose, int previousClose, int period, int expectedTrend)
    {
        Analyze analyze = new Analyze();

        List<StockHistory> list = new List<StockHistory>()
        {
            new StockHistory
            {
                Close = latestClose
            },
            new StockHistory
            {
                Close = previousClose
            }
        };
        var result = analyze.CalculateMovingTrend(list, period);
        Assert.Equal(expectedTrend, result);


    }
    [Theory]
    [InlineData(3, 110, 100, 120, 90, 95, 4)]
    public void CalculateOneDayScore(int changePercent, int price, int open, int high, int low, int previousClose, int expectedValue)
    {
        StockResult result = new StockResult()
        {
            ChangePercent = changePercent,
            Price = price,
            Open = open,
            High = high,
            Low = low,
            PreviousClose = previousClose
        };
        Analyze analyze = new Analyze();
        var test = analyze.CalculateOneDayScore(result);
        Assert.Equal(expectedValue, test);
    }

    [Fact]
    public void StockAnalyse_WhenHistoryIsEmpty_ReturnsNull()
    {
        // Skapar en tom StockResult
        StockResult result = new StockResult();
        Analyze analyze = new Analyze();
        // Skapar en tom lista
        List<StockHistory> emptyList = new List<StockHistory>();
        // Skickar de till StockAnalyse
        var test = analyze.StockAnalyse(result, emptyList);
        // Förväntar mig Null
        Assert.Null(test);

        

    }

    [Theory]
    [InlineData("Test", 3, 150, 145, 151, 140, 148, "Bullish", -1)]
    [InlineData("Test", -3, 100, 145, 151, 90, 148, "Bearish", 1)]
    [InlineData("Test", 0, 100, 100, 110, 90, 100, "Neutral", 0)]
    public void StockAnalyse_Complete_Bullish_Scenario(string ticker, int changePercent, int price, int open, int high, int low, int previousClose, string expectedSignal, int expectedChange)
    {
        List<StockHistory> stockHistory = new List<StockHistory>();
        Analyze analyze = new Analyze();
        StockResult stockResult = new StockResult()
        {
            Ticker = ticker,
            ChangePercent = changePercent,
            Price = price,
            Open = open,
            High = high,
            Low = low,
            PreviousClose = previousClose
        };

        for(int i = 0; i < 50; i++)
        {
            var test = new StockHistory
            {
                Close = price + expectedChange * i
            };
            stockHistory.Add(test);

        }
        var result = analyze.StockAnalyse(stockResult, stockHistory);
        Assert.NotNull(result);
        Assert.Equal(expectedSignal, result.Signal);
    }

    [Fact]
    public void StockAnalyse_With40Days_UsesMA30ButSkipsMA50()
    {
        int startPrice = 140;
        int historyStep = -1;
        Analyze analyze = new Analyze();
        StockResult stockResult = new StockResult();
        List<StockHistory> list = new List<StockHistory>();

        for(int i = 0; i < 40; i++)
        {
            var test = new StockHistory
            {
                Close = startPrice + historyStep * i
            };
            list.Add(test);
        }

        var result = analyze.StockAnalyse(stockResult, list);
        Assert.NotNull(result);
        Assert.Equal("Neutral", result.Signal);
        Assert.Contains("30", result.Reason);
        Assert.DoesNotContain("50", result.Reason);
        Assert.Equal(40, list.Count);
    }
  


    [Fact]
    public void StockResultCheck()
    {
        var httpClient = new HttpClient();
        var configuration = new ConfigurationBuilder().Build();
        var service = new StockService(httpClient, configuration);

        List<StockHistory> stockHistory = new List<StockHistory>()
        {
            new StockHistory
            {
                Date = new DateOnly(2026, 6, 27),
                Open = 105,
                High = 115,
                Low = 90,
                Close = 110,
                Volume = 25_255
               
            },
            new StockHistory
            {
                Date = new DateOnly(2026, 6, 26),
                Close = 100,
            },

        };

            var result = service.StockResult(stockHistory, "Test");
            Assert.Equal("Test", result.Ticker);
            Assert.Equal(110, result.Price);
            Assert.Equal(100, result.PreviousClose);
            Assert.Equal(10, result.Change);
            Assert.Equal(10, result.ChangePercent);
            Assert.Equal(105, result.Open);
            Assert.Equal(115, result.High);
            Assert.Equal(90, result.Low);
            Assert.Equal(25_255, result.Volume);
            Assert.Equal(new DateOnly(2026, 6, 27), result.LatestTradingDay);
        }
    }

