namespace StockAnalyzer.Api.Models;
using System.Diagnostics.Tracing;

public enum StockApiError
{
    None,
    InvalidTicker,
    RateLimit,
    ExternalApiError,
    NoData

}