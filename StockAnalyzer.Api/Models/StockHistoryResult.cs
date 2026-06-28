using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

namespace StockAnalyzer.Api.Models;

public class StockHistoryResult
{
    public List<StockHistory> History {get;set;} = new List<StockHistory>();

    public string? Message {get;set;}

    public StockApiError ErrorType {get;set;}
}