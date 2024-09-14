namespace Mamlaka.API.Services.HttpService;
public interface IHttpClientFactory
{
    HttpClient CreateHttpClient(string baseUri);
}
