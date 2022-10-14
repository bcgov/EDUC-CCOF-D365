namespace CCOF.Infrastructure.WebAPI.Services
{
    public interface ID365AuthenticationService
    {
        Task<HttpClient> GetHttpClient();
        Task<HttpClient> GetHttpClient(bool isSearch);
    }
}