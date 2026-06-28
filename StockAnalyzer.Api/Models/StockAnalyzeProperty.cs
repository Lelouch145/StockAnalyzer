namespace StockAnalyzer.Api.Models;

public class AnalyzeValue
{
    public string Ticker { get; set; } = "";
    public string Signal { get; set; } = "";
    public string Reason { get; set; } = "";
    public string RiskLevel { get; set; } = "";
    public DateOnly CreatedAt { get; set; }
}