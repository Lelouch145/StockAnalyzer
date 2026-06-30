using Microsoft.AspNetCore.Http.HttpResults;
using StockAnalyzer.Api.Models;
using StockAnalyzer.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<StockService>();
builder.Services.AddSingleton<Analyze>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () => "Hello World!");
// Endpointen för att få enbart historiken 
app.MapGet("/stock/{ticker}", async (string ticker, StockService service) =>
{
    // Skickar in ticker och väntar på svar
    var result = await service.GetStockHistoryAsync(ticker);
    // Om result ger nån av följande fel så får de deras felmeddelande
    if(result.ErrorType == StockApiError.InvalidTicker)
    {
        return Results.BadRequest(result.Message);
    }
    if(result.ErrorType == StockApiError.ExternalApiError)
    {
        return Results.BadRequest(result.Message);
    }
    if(result.ErrorType == StockApiError.NoData)
    {
        return Results.NotFound(result.Message);
    }
    if (result.History.Count < 2)
    {
        return Results.BadRequest("At least 2 days of history are required");
    }
    // Använder StockResult metoden för att Mappa och visa 
    var found = service.StockResult(result.History, ticker);
    return Results.Ok(found);

});
// Får historiken och skickar till analysen
app.MapGet("/stock/{ticker}/analysis", async (Analyze analys, string ticker, StockService service) =>
{

    var result = await service.GetStockHistoryAsync(ticker);
    if(result.ErrorType == StockApiError.InvalidTicker)
    {
        return Results.BadRequest(result.Message);
    }
    if(result.ErrorType == StockApiError.ExternalApiError)
    {
        return Results.BadRequest(result.Message);
    }
    if(result.ErrorType == StockApiError.NoData)
    {
        return Results.NotFound(result.Message);
    }
    if(result.History.Count < 30)
    {
        return Results.BadRequest("At least 30 days of history are required");
    }
    // StockResult mappar innehållet från GetStockHistoryAsync
    var found = service.StockResult(result.History, ticker);
    if (found == null)
    {
        return Results.BadRequest("Could not fetch stock data");
    }

    
    // skickar informationen till analysdelen
    var analysisResult = analys.StockAnalyse(found, result.History);
    
    if (analysisResult == null)
    {
        return Results.BadRequest("Could not analyze stock data");
    }
    // Skriver ut analysen om aktien
    return Results.Ok(analysisResult);
});
app.MapGet("/stock/{ticker}/history", async (string ticker, StockService service) =>
{
    var found = await service.GetStockHistoryAsync(ticker);
    if (found == null)
    {
        return Results.BadRequest();
    }
    return Results.Ok(found);
});

app.Run();
public partial class Program { }
