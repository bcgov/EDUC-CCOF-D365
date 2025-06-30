using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;

namespace CCOF.Infrastructure.WebAPI.Services.D365WebAPI
{
    public interface ID365AuthenticationService
    {
        Task<HttpClient> GetHttpClient();
        Task<HttpClient> GetHttpClient(bool isSearch);
        Task<HttpClient> GetHttpClientAsync(D365ServiceType type, AZAppUser spn);
    }
}