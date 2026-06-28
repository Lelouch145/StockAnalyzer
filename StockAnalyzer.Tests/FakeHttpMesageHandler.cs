namespace StockAnalyzer.Tests;
using System.Net.Http;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage httpResponseMessage;

    public FakeHttpMessageHandler(HttpResponseMessage _httpResponse)
    {
        httpResponseMessage = _httpResponse;
    }

    protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(httpResponseMessage);
    }
}