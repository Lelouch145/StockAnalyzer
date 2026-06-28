namespace StockAnalyzer.Api.Models;


public class StockResult
{
    public string Ticker { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal PreviousClose { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public DateOnly LatestTradingDay { get; set; }
    public long Volume { get; set; }
    public string Message { get; set; } = "";
    public DateOnly CreatedAt { get; set; }

}