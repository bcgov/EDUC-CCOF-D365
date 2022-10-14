using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCOF.Infrastructure.WebAPI.Models
{
    public class D365AuthSettings
    {
        public string BaseUrl { get; set; } = string.Empty; // Base URL
        public string ResourceUrl { get; set; } = string.Empty;
        public string WebApiUrl { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty; // Azure Registered Application ID
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string APIVersion { get; set; } = string.Empty;
        public string SearchVersion { get; set; } = "v1.0";
    }
}
