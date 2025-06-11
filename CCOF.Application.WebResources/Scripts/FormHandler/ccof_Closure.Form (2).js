var CCOF = CCOF || {};
CCOF.Closure = CCOF.Closure || {};
CCOF.Closure.Form = CCOF.Closure.Form || {};

//Formload logic starts here
CCOF.Closure.Form = {
    onLoad: function (executionContext) {
        debugger;
        this.startAndEndDateOnClosureRequest(executionContext);
        this.requiredFieldsonApproval(executionContext);
        this.requiredFieldsonClosureType(executionContext);
        var formContext = executionContext.getFormContext();
        formContext.getAttribute("ccof_closure_type").addOnChange(this.requiredFieldsonClosureType);
    },

    startAndEndDateOnClosureRequest: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var closureRequest = formContext.getAttribute("ccof_change_action_closure").getValue();

        if (closureRequest && closureRequest.length > 0) {
            var closureRequestId = closureRequest[0].id.replace("{", "").replace("}", "");

            // Fetch the parent account of selected account
            Xrm.WebApi.retrieveRecord("ccof_change_action_closure", closureRequestId, "?$select=ccof_closure_end_date,ccof_closure_start_date").then(
                function success(result) {
                    console.log(result);
                    var requestStartDate = (new Date(result["ccof_closure_start_date"])).toLocaleDateString('en', { timeZone: 'UTC' });
                    var requestEndDate = new Date(result["ccof_closure_end_date"]).toLocaleDateString('en', { timeZone: 'UTC' });

                    var closureStartDate = (formContext.getAttribute("ccof_startdate")?.getValue()).toLocaleDateString();
                    var closureEndDate = formContext.getAttribute("ccof_enddate")?.getValue().toLocaleDateString();

                    // Clear existing notifications
                    formContext.ui.clearFormNotification("dateValidation");

                    if (closureStartDate && closureEndDate) {
                        if (closureStartDate < requestStartDate || closureEndDate > requestEndDate) {
                            formContext.ui.setFormNotification(
                                "Warning: The start and end dates do not align with the dates on the Closure Request.",
                                "WARNING",
                                "dateValidation"
                            );
                        }
                    }
                },
                function (error) {
                    console.log("Error retrieving Change Action Closure: " + error.message);
                }
            );
        }
    },

    requiredFieldsonApproval: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var statusReason = formContext.getAttribute("ccof_closure_status").getValue();
        if (statusReason == 100000001) {
            formContext.getAttribute("ccof_approved_as").setRequiredLevel("required");
            formContext.getAttribute("ccof_payment_eligibility").setRequiredLevel("required");
        }
        else {
            formContext.getAttribute("ccof_approved_as").setRequiredLevel("none");
            formContext.getAttribute("ccof_payment_eligibility").setRequiredLevel("none");
        }
    },
    requiredFieldsonClosureType: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var closureType = formContext.getAttribute("ccof_closure_type").getValue();
        if (closureType == 100000002) // Unexpected Closure
        {
            formContext.getAttribute("ccof_emergency_closure_type").setRequiredLevel("none");
            formContext.getAttribute("ccof_closure_approved_under_emergency_type").setRequiredLevel("none");
            formContext.getAttribute("ccof_enrollment_report_submitted_reviewed").setRequiredLevel("none");
        };
        if (closureType == 100000000) // Planed Closure
        {
            formContext.getAttribute("ccof_emergency_closure_type").setRequiredLevel("none");
            formContext.getAttribute("ccof_closure_approved_under_emergency_type").setRequiredLevel("none");
            formContext.getAttribute("ccof_enrollment_report_submitted_reviewed").setRequiredLevel("none");
        };
        //		else
        //		{
        //			formContext.getAttribute("ccof_emergency_closure_type").setRequiredLevel("required");
        //            formContext.getAttribute("ccof_closure_approved_under_emergency_type").setRequiredLevel("required");
        //			formContext.getAttribute("ccof_enrollment_report_submitted_reviewed").setRequiredLevel("required");
        //		};
        if (closureType != null) {
            formContext.getAttribute("ccof_closure_type").setRequiredLevel("none");
        }
        else {
            formContext.getAttribute("ccof_closure_type").setRequiredLevel("required");
        }
    },
}