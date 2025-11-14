using System.ComponentModel.DataAnnotations;

namespace CCOF.Infrastructure.WebAPI.Models;

//ECER API Model
// Model for the token response from the POST endpoint
public class TokenResponse
{
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
}

// Model for each file returned from the ECER files endpoint
public class CertificationFile
{
    public string id { get; set; }
    public string fileName { get; set; }
    public string fileId { get; set; }
    public string createdOn { get; set; }
}

// Model for the certification details downloaded for a file
public class CertificationDetail
{
    public string? registrationnumber { get; set; }
    public string? certificatelevel { get; set; }
    public DateTime? effectivedate { get; set; }
    public DateTime? expirydate { get; set; }
    public string? firstname { get; set; }
    public string? lastname { get; set; }
}
