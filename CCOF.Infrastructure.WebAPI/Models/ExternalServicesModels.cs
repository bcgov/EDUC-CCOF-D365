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
    public string effectivedate { get; set; }
    public string expirydate { get; set; }
    public string? firstname { get; set; }
    public string? lastname { get; set; }
    //public string Statuscode { get; set; }
    public int statuscode { get; set; }
}

public enum Active
{
    Active = 1,
    Expired = 621870001,
    Reprinted = 621870002,
    Cancelled = 621870003,
    Suspended = 621870004
}