using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace CCOF.Infrastructure.WebAPI.Services.Processes;

public class Location
{
    [property: JsonProperty("@odata.type")]
    public string ODataType => "Microsoft.Dynamics.CRM.expando";

    // Gets or sets the latitude of the Location.
    public double? Latitude { get; set; }

    [JsonProperty("Latitude@odata.type")]
    public static string LatitudeType => "Double";

    // Gets or sets the longitude of the Location.
    public double? Longitude { get; set; }

    [JsonProperty("Longitude@odata.type")]
    public static string LongitudeType => "Double";
}

public record ProcessParameter
{
    //DO NOT change the optional properties
    [property: JsonPropertyName("triggeredBy")]
    public string? TriggeredBy { get; set; }

    [property: JsonPropertyName("triggeredOn")]
    public DateTime? TriggeredOn { get; set; }

    [property: JsonPropertyName("callerObjectId")]
    public Guid? CallerObjectId { get; set; }

    [property: JsonPropertyName("paymentfile")]

    public PaymentParameter? PaymentFile { get; set; }

    [property: JsonPropertyName("dataImportId")]
    public Guid? DataImportId { get; set; }
    [property: JsonPropertyName("enrolmentReportid")]
    public Guid? EnrolmentReportid { get; set; }
    [property: JsonPropertyName("organization")]
    public OrganizationParameter? Organization { get; set; }
    [property:JsonPropertyName("programApproved")]
    public int? programapproved { get; set; }

    #region Inner Parameter Record Objects

    public record PaymentParameter
    {
        [property: JsonPropertyName("paymentfileid")]
        public string? paymentfileId { get; set; }
    }

    #endregion
    #region Initial CCOF Enrolment Report 
    [property: JsonPropertyName("initialEnrolmentReport")]
    public InitialEnrolmentReportParameter? InitialEnrolmentReport { get; set; }
    public record InitialEnrolmentReportParameter
    {
        [property: JsonPropertyName("year")]
        public string? Year { get; set; }

        [property: JsonPropertyName("month")]
        public int? Month { get; set; }

        [property: JsonPropertyName("programYearId")]
        public string? ProgramYearId { get; set; }

        [property: JsonPropertyName("facilityGuid")]
        public string[]? FacilityGuid { get; set; }
    }
    #endregion
    public record OrganizationParameter
    {
        [property: JsonPropertyName("organizationId")]
        public Guid? organizationId { get; set; }
        [property: JsonPropertyName("facilityId")]
        public Guid? facilityId { get; set; }

        [property: JsonPropertyName("legalName")]
        public string? legalName { get; set; }

        [property: JsonPropertyName("incorporationNumber")]
        public string? incorporationNumber { get; set; }
    }
 }