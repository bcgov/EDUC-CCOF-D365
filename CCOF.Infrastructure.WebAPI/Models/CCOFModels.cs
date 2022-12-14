using System.Text.Json.Serialization;

namespace CCOF.Infrastructure.WebAPI.Models
{
    public class UserProfile
    {
        public string? ccof_username { get; set; }

        public string? ccof_userid { get; set; }

        [JsonPropertyName("Organization.accountid")]

        public string? organization_accountid { get; set; }
        [JsonPropertyName("Organization.accountnumber")]

        public string? organization_accountnumber { get; set; }

        [JsonPropertyName("Organization.name")]
        public string? organization_name { get; set; }

        [JsonPropertyName("Organization.ccof_contractstatus")]
        public int? organization_ccof_contractstatus { get; set; }

        [JsonPropertyName("Organization.ccof_formcomplete")]
        public bool? organization_ccof_formcomplete { get; set; }

        public Facility[]? facilities { get; set; }

        public Application? application { get; set; }
    }

    public class Facility
    {
        public string? accountid { get; set; }
        public string? accountnumber { get; set; }
        public string? name { get; set; }
        [JsonPropertyName("ccof_facilitystatus@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_facilitystatus_formatted { get; set; }
        public int? ccof_facilitystatus { get; set; }
        public string? ccof_facilitylicencenumber { get; set; }
        public bool? ccof_formcomplete { get; set; }
        public string? _parentaccountid_value { get; set; }
    }

    public class Application
    {
        public string? ccof_applicationid { get; set; }
        public string? ccof_name { get; set; }
        [JsonPropertyName("ccof_applicationtype@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_applicationtype_formatted { get; set; }
        public int? ccof_applicationtype { get; set; }
        public string? _ccof_programyear_value { get; set; }
        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        [JsonPropertyName("ccof_providertype@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_providertype_formatted { get; set; }
        public int? ccof_providertype { get; set; }
        public string? _ccof_organization_value { get; set; }
        public int? ccof_unlock_ccof { get; set; }
        public int? ccof_unlock_ecewe { get; set; }
        public int? ccof_unlock_licenseupload { get; set; }
        public int? ccof_unlock_supportingdocument { get; set; }
        public int? ccof_unlock_declaration { get; set; }
        public bool? ccof_licensecomplete { get; set; }
        public bool? ccof_ecewe_eligibility_complete { get; set; }
        [JsonPropertyName("ccof_ccofstatus@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_ccofstatus_formatted { get; set; }
        public int? ccof_ccofstatus { get; set; }       
        public Ccof_Programyear? ccof_ProgramYear { get; set; }
        public Ccof_Application_Basefunding_Application[]? ccof_application_basefunding_Application { get; set; }
        public Ccof_Applicationccfri_Application_Ccof_Ap[]? ccof_applicationccfri_Application_ccof_ap { get; set; }
        public Ccof_Ccof_Application_Ccof_Applicationecewe_Application[]? ccof_ccof_application_ccof_applicationecewe_application { get; set; }
    }

    public class Ccof_Programyear
    {
        public string? ccof_name { get; set; }
        public string? ccof_program_yearid { get; set; }
        public int? statuscode { get; set; }
        public DateTime? ccof_declarationbstart { get; set; }
        public DateTime? ccof_intakeperiodstart { get; set; }
        public DateTime? ccof_intakeperiodend { get; set; }
    }

    public class Ccof_Application_Basefunding_Application
    {
        public string? ccof_application_basefundingid { get; set; }
        public string? _ccof_application_value { get; set; }
        public string? _ccof_facility_value { get; set; }
        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        public bool? ccof_formcomplete { get; set; }
    }

    public class Ccof_Applicationccfri_Application_Ccof_Ap
    {
        public string? ccof_applicationccfriid { get; set; }
        public string? _ccof_application_value { get; set; }
        public string? _ccof_facility_value { get; set; }
        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        public int? ccof_ccfrioptin { get; set; }
        public bool? ccof_formcomplete { get; set; }
        public int? ccof_unlock_rfi { get; set; }
        public int? ccof_unlock_ccfri { get; set; }
        public int? ccof_unlock_nmf_rfi { get; set; }
    }

    public class Ccof_Ccof_Application_Ccof_Applicationecewe_Application
    {
        public string? ccof_applicationeceweid { get; set; }
        public string? _ccof_application_value { get; set; }
        public string? _ccof_facility_value { get; set; }
        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        public int? ccof_optintoecewe { get; set; }
        public bool? ccof_formcomplete { get; set; }
    }
}
