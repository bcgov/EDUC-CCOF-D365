using CCOF.Infrastructure.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCOF.Infrastructure.WebAPI.Models
{
    public class D365AuthSettings
    {
        public Func<Task<string>>? GetAccessToken { get; set; }
        public required string BaseUrl { get; set; } // Dynamics Base URL
        public required string ResourceUrl { get; set; }
        public required string WebApiUrl { get; set; }
        public required string BatchUrl { get; set; }
        public required string BaseServiceUrl { get; set; } // Dynamics Base Service URL for Dataverse Search, Batch Operations etc.
        public required string RedirectUrl { get; set; }
        public required string ApiVersion { get; set; }
        public required Int16 TimeOutInSeconds { get; set; }
        public required string SearchVersion { get; set; }
        public required List<AZAppUser> AZAppUsers { get; set; }
        public required string HttpClientName { get; set; }
        public required Guid CallerObjectId { get; set; }

        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty; // Azure Registered Application ID
        public string ClientSecret { get; set; } = string.Empty;

        public string APIVersion { get; set; } = string.Empty;
    }
}
