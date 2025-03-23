namespace DynamicDnsClient.Clients;

public interface IHttpClient : IDisposable
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}
