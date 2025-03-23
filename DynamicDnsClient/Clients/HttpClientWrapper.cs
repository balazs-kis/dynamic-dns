namespace DynamicDnsClient.Clients;

public class HttpClientWrapper : IHttpClient
{
    private readonly HttpClient _httpClient;

    public HttpClientWrapper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) => _httpClient.SendAsync(request);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _httpClient.Dispose();
    }
}
