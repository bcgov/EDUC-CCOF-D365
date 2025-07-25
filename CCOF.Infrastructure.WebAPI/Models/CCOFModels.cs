﻿using System.Text.Json.Serialization;
using System.Xml.Linq;
using CCOF.Core.DataContext;

namespace CCOF.Infrastructure.WebAPI.Models
{
    public class UserProfile
    {
        public string? ccof_username { get; set; }

        public string? ccof_userid { get; set; }

        public string? contactid { get; set; }

        [JsonPropertyName("Organization.accountid")]
        public string? organization_accountid { get; set; }
        [JsonPropertyName("Organization.accountnumber")]

        public string? organization_accountnumber { get; set; }

        [JsonPropertyName("Organization.name")]
        public string? organization_name { get; set; }

        [JsonPropertyName("Organization.ccof_fundingagreementnumber")]
        public string? organization_ccof_fundingagreementnumber { get; set; }

        [JsonPropertyName("Organization.ccof_contractstatus")]
        public int? organization_ccof_contractstatus { get; set; }

        [JsonPropertyName("Organization.ccof_formcomplete")]
        public bool? organization_ccof_formcomplete { get; set; }

        public Facility[]? facilities { get; set; }

        public Application? application { get; set; }
        [JsonPropertyName("Portalrole.ofm_portal_roleid")]
        public string? portalrole_id { get; set; }
    }
    public class FiscalUserProfile
    {
        public string? ccof_username { get; set; }

        public string? ccof_userid { get; set; }

        public string? contactid { get; set; }

        [JsonPropertyName("Organization.accountid")]

        public string? organization_accountid { get; set; }
        [JsonPropertyName("Organization.accountnumber")]

        public string? organization_accountnumber { get; set; }

        [JsonPropertyName("Organization.name")]
        public string? organization_name { get; set; }

        [JsonPropertyName("Organization.ccof_fundingagreementnumber")]
        public string? organization_ccof_fundingagreementnumber { get; set; }
        [JsonPropertyName("Organization.ccof_bypass_goodstanding_check")]
        public bool? organization_ccof_bypass_goodstanding_check { get; set; }
        [JsonPropertyName("Organization.ccof_good_standing_status")]
        public int? organization_ccof_good_standing_status { get; set; }

        [JsonPropertyName("Organization.ccof_contractstatus")]
        public int? organization_ccof_contractstatus { get; set; }

        [JsonPropertyName("Organization.ccof_formcomplete")]
        public bool? organization_ccof_formcomplete { get; set; }

        public Facility[]? facilities { get; set; }

        public Application[]? application { get; set; }
        [JsonPropertyName("Portalrole.ofm_portal_roleid")]
        public string? portalrole_id { get; set; }
    }

    public class Facility
    {        
        public string? accountid { get; set; }        
        public string? accountnumber { get; set; }
        public string? name { get; set; }
        [JsonPropertyName("ccof_facilitystatus@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_facilitystatus_formatted { get; set; }
        public int? ccof_facilitystatus { get; set; }
        [JsonPropertyName("ccof_healthauthority@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_healthauthority_formatted { get; set; }
        public int? ccof_healthauthority { get; set; }
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
        public string? ccof_application_template_version { get; set; }
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
        public ccof_ccof_change_request_Application_ccof_appl[]? ccof_ccof_change_request_Application_ccof_appl { get; set; }       
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
        public bool? ccof_closureformcomplete { get; set; }

        public int? ccof_unlock_rfi { get; set; }
        public int? ccof_unlock_ccfri { get; set; }
        public int? ccof_unlock_nmf_rfi { get; set; }
        public int? ccof_unlock_afsenable { get; set; }
        public int? ccof_unlock_afs { get; set; }
        public int? ccof_afs_status { get; set; }
        public bool? ccof_has_nmf { get; set; }
        public bool? ccof_has_rfi { get; set; }
        public bool? ccof_nmf_formcomplete { get; set; }
        public bool? ccof_rfi_formcomplete { get; set; }
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

    public class ccof_change_request_new_facility_change_act
    {
        public string? ccof_change_request_new_facilityid { get; set; }
        public string? _ccof_change_action_value { get; set; }
        public string? _ccof_ccof_value { get; set; }
        public string? _ccof_ecewe_value { get; set; }
        public string? _ccof_facility_value { get; set; }

        public string? _ccof_ccfri_value { get; set; }
        public string? ccof_name { get; set; }

        [JsonPropertyName("ccof_unlock_ccfri@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_ccfri_formatted { get; set; }
        public bool? ccof_unlock_ccfri { get; set; }

        [JsonPropertyName("ccof_unlock_rfi@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_rfi_formatted { get; set; }
        public bool? ccof_unlock_rfi { get; set; }

        [JsonPropertyName("ccof_unlock_nmf_rfi@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_nmf_rfi_formatted { get; set; }
        public bool? ccof_unlock_nmf_rfi { get; set; }

        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        [JsonPropertyName("statecode@OData.Community.Display.V1.FormattedValue")]
        public string? statecode_formatted { get; set; }
        public int? statecode { get; set; }

    }

    public class ccof_change_action_change_request
    {
    
         public bool? ccof_unlock_ccof { get; set; }
        public string? ccof_change_actionid { get; set; }
        public string? _ccof_change_request_value { get; set; }
        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        [JsonPropertyName("statecode@OData.Community.Display.V1.FormattedValue")]
        public string? statecode_formatted { get; set; }
        public int? statecode { get; set; }
        [JsonPropertyName("ccof_changetype@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_changetype_formatted { get; set; }
        public int? ccof_changetype { get; set; }
        public string? _ccof_ccfri_value { get; set; }
        public string? ccof_name { get; set; }

        [JsonPropertyName("ccof_unlock_ecewe@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_ecewe_formatted { get; set; }
        public bool? ccof_unlock_ecewe { get; set; }

        [JsonPropertyName("ccof_unlock_licence_upload@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_licence_upload_formatted { get; set; }
        public bool? ccof_unlock_licence_upload { get; set; }

        [JsonPropertyName("ccof_unlock_supporting_document@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_supporting_document_formatted { get; set; }
        public bool? ccof_unlock_supporting_document { get; set; }

        [JsonPropertyName("ccof_unlock_change_request@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_change_request_formatted { get; set; }
        public bool? ccof_unlock_change_request { get; set; }

        [JsonPropertyName("ccof_unlock_other_changes_document@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_unlock_other_changes_document_formatted { get; set; }
        public bool? ccof_unlock_other_changes_document { get; set; }

        public ccof_change_request_new_facility_change_act[]? ccof_change_request_new_facility_change_act { get; set; }
    }

    public class ccof_ccof_change_request_Application_ccof_appl
    {
        public string? ccof_change_requestid { get; set; }
        [JsonPropertyName("statuscode@OData.Community.Display.V1.FormattedValue")]
        public string? statuscode_formatted { get; set; }
        public int? statuscode { get; set; }
        [JsonPropertyName("statecode@OData.Community.Display.V1.FormattedValue")]
        public string? statecode_formatted { get; set; }
        public int? statecode { get; set; }
        [JsonPropertyName("ccof_externalstatus@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_externalstatus_formatted { get; set; }
        public int? ccof_externalstatus { get; set; }    
        public string? ccof_name { get; set; }
         [JsonPropertyName("ccof_declaration@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_declaration_formatted { get; set; }
        public bool? ccof_declaration { get; set; }
        public string? ccof_unlock_declaration_formatted { get; set; }
        public bool? ccof_unlock_declaration { get; set; }
        [JsonPropertyName("ccof_licensecomplete@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_licensecomplete_formatted { get; set; }
        public bool? ccof_licensecomplete { get; set; }
        [JsonPropertyName("ccof_ecewe_eligibility_complete@OData.Community.Display.V1.FormattedValue")]
        public string? ccof_ecewe_eligibility_complete_formatted { get; set; }
        public bool? ccof_ecewe_eligibility_complete { get; set; }

        public ccof_change_action_change_request[]? ccof_change_action_change_request { get; set; }
    }

    public class ApplicationDocumentResponse
    {
        public string subject { get; set; }
        public string filename { get; set; }
        public string filesize { get; set; }
        public string annotationid { get; set; }
        public string applicationFacilityDocumentId { get; set; }
    }
    public class ApplicationSummaryDocumentResponse
    {
      
        public string filename { get; set; }
        public string filesize { get; set; }
        public string annotationid { get; set; }
        public string id { get; set; }
        public string uploadedon { get; set; }
        public string applicationtype { get; set; }
        public string programyear { get; set; }

    }
    public class ChangeRequestDocumentResponse
    {
        public string filename { get; set; }

        public string filesize { get; set; }
        public string annotationid { get; set; }
        public string id { get; set; }
        public string uploadedon { get; set; }
        public string applicationtype { get; set; }
        public string programyear { get; set; }

    }
}
