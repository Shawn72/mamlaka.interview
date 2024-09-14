namespace Mamlaka.API.Services.HttpService;
public class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateHttpClient(string baseUri)
    {
        HttpClient client = new HttpClient();
        SetupClientDefaults(client, baseUri);
        return client;
    }
    protected virtual void SetupClientDefaults(HttpClient client, string baseUri)
    {
        client.Timeout = TimeSpan.FromSeconds(120); //set client timeout.
        client.BaseAddress = new Uri(baseUri);
    }
}
