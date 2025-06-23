using CCOF.Infrastructure.WebAPI.Models;

namespace CCOF.Infrastructure.WebAPI.Services.D365WebApi;

public interface ID365TokenService
{
    Task<string> FetchAccessToken(string baseUrl, AZAppUser azSPN);
}
