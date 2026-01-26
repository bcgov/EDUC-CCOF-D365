//the below function working only onLoad of the form
function onChangeOfApplicationType(executionContext) {
    debugger;
    var formType = Xrm.Page.ui.getFormType();
    hideShow(formType);
    // Jan 26,2026 ticket 6933
    let formContext = executionContext.getFormContext();
    let appType = formContext.getAttribute("ccof_applicationtype").getValue();
    if (appType === 100000002) {  // Renewal applicaiton
        formContext.getControl("ccof_unlock_ccof").setVisible(false);
        formContext.getControl("ccof_unlock_renewal").setVisible(true);

    } else {
        formContext.getControl("ccof_unlock_ccof").setVisible(true);
        formContext.getControl("ccof_unlock_renewal").setVisible(false);
    }

}

//Make all fields read only on the application form
function hideShow(formTypeVal) {
    if (formTypeVal != 1) {
        Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(true);
    }
    else   //create
    {
        Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(false);
    }
}

//Make all fields read only on the application form
function HideShowJJEWageRates(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    var programYear = formContext.getAttribute("ccof_programyear").getValue();

    var programYearId = programYear[0].id;

    Xrm.WebApi.retrieveRecord("ccof_program_year", programYearId, "?$select=ccof_programyearnumber").then(
        function success(results) {
            console.log(results);
            if (results["ccof_programyearnumber"] != null) {
                programyearnumber = results["ccof_programyearnumber"];
                if (programyearnumber < 6) {
                    formContext.getControl("ccof_ecewe_confirmation").setVisible(true);
                    formContext.getControl("ccof_describe_your_org").setVisible(false);
                    formContext.getControl("ccof_union_agreement_reached").setVisible(false);
                }
                else {
                    formContext.getControl("ccof_ecewe_confirmation").setVisible(false);
                    formContext.getControl("ccof_describe_your_org").setVisible(true);
                    formContext.getControl("ccof_union_agreement_reached").setVisible(true);
                }
            }
        },
        function (error) {
            console.log(error.message);
        }
    );



}