var CCOF = CCOF || {};
CCOF.Closure = CCOF.Closure || {};
CCOF.Closure.Form = CCOF.Closure.Form || {};

//Formload logic starts here
CCOF.Closure.Form = {
    onLoad: function (executionContext) {
        this.startAndEndDateOnClosureRequest(executionContext);
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
                    var requestStartDate = new Date(result["ccof_closure_start_date"]);
                    var requestEndDate = new Date(result["ccof_closure_end_date"]);

                    var closureStartDate = formContext.getAttribute("ccof_startdate")?.getValue();
                    var closureEndDate = formContext.getAttribute("ccof_enddate")?.getValue();

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


}