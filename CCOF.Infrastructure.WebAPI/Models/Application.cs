namespace CCOF.Infrastructure.WebAPI.Models
{
    public class Application
    {
        public int statuscode { get; set; }
        public int ccof_providertype { get; set; }
        public string ccof_submittedby { get; set; }
        public int ccof_version { get; set; }
        public bool ccof_consent { get; set; }
        public bool ccof_ecewe_optintoecewe { get; set; }
        public int ccof_applicationtype { get; set; }
        public int ccof_ecewe_selecttheapplicablefundingmodel { get; set; }
        public int ccof_ecewe_selecttheapplicablesector { get; set; }
        public string ccof_ProgramYearodatabind { get; set; }
        public string ccof_Organizationodatabind { get; set; }
        public Ccof_Applicationccfri_Application_Ccof_Ap[] ccof_applicationccfri_Application_ccof_ap { get; set; }
        public Ccof_Ccof_Application_Ccof_Applicationecewe_Application[] ccof_ccof_application_ccof_applicationecewe_application { get; set; }
        public Ccof_Ccof_Application_Ccof_Rfipfi_Application[] ccof_ccof_application_ccof_rfipfi_application { get; set; }
    }

    public class Ccof_Applicationccfri_Application_Ccof_Ap
    {
        public int ccof_optintoapplicationccfri { get; set; }
        public int ccof_feecorrectccfri { get; set; }
        public int ccof_chargefeeccfri { get; set; }
        public string ccof_Facilityodatabind { get; set; }
        public Ccof_Application_Ccfri_Ccc[] ccof_application_ccfri_ccc { get; set; }
    }

    public class Ccof_Application_Ccfri_Ccc
    {
        public float ccof_april { get; set; }
        public float ccof_may { get; set; }
        public float ccof_june { get; set; }
        public float ccof_july { get; set; }
        public float ccof_august { get; set; }
        public float ccof_september { get; set; }
        public float ccof_october { get; set; }
        public float ccof_november { get; set; }
        public float ccof_december { get; set; }
        public float ccof_january { get; set; }
        public float ccof_february { get; set; }
        public float ccof_march { get; set; }
        public int ccof_parentfeesperiod { get; set; }
        public string ccof_ChildcareCategoryodatabind { get; set; }
        public string ccof_ProgramYearodatabind { get; set; }
    }

    public class Ccof_Ccof_Application_Ccof_Applicationecewe_Application
    {
        public bool ccof_optintoecewe { get; set; }
        public string ccof_Facilityodatabind { get; set; }
    }

    public class Ccof_Ccof_Application_Ccof_Rfipfi_Application
    {
        public bool ccof_haveyouincreasedparentfeesbefore { get; set; }
        public string ccof_Facilityodatabind { get; set; }
        public Ccof_Ccof_Rfipfi_Ccof_Rfi_Pfi_Fee_History_Deta[] ccof_ccof_rfipfi_ccof_rfi_pfi_fee_history_deta { get; set; }
        public Ccof_Ccof_Rfipfi_Ccof_Rfipfiserviceexpansiondetail_Rfipfi[] ccof_ccof_rfipfi_ccof_rfipfiserviceexpansiondetail_rfipfi { get; set; }
    }

    public class Ccof_Ccof_Rfipfi_Ccof_Rfi_Pfi_Fee_History_Deta
    {
        public float ccof_feebeforeincrease { get; set; }
        public string ccof_dateoffeeincrease { get; set; }
        public float ccof_feeafterincrease { get; set; }
        public int ccof_carecategory { get; set; }
    }

    public class Ccof_Ccof_Rfipfi_Ccof_Rfipfiserviceexpansiondetail_Rfipfi
    {
        public string ccof_facilitysprevioushoursofoperation { get; set; }
        public float ccof_amountofexpense { get; set; }
        public string ccof_dateofchange { get; set; }
        public string ccof_facilitysnewhoursofoperation { get; set; }
        public string ccof_paymentfrequencydetails { get; set; }
    }
}