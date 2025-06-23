namespace CCOF.Infrastructure.WebAPI.Services.D365WebAPI
{
    public interface ID365AuthenticationService
    {
        Task<HttpClient> GetHttpClient();
        Task<HttpClient> GetHttpClient(bool isSearch);
    }
}