using CCOF.Infrastructure.WebAPI.Models;

namespace CCOF.Infrastructure.WebAPI.Services.AppUsers;

public interface ID365AppUserService
{
    AZAppUser GetAZAppUser(string userType);
    AZAppUser AZPortalAppUser { get; }
    AZAppUser AZSystemAppUser { get; }
    AZAppUser AZNoticationAppUser { get; }
}