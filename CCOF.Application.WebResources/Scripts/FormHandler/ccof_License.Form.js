var CCOF = CCOF || {};
CCOF.License = CCOF.License || {};
CCOF.License.Form = CCOF.License.Form || {};

//Formload logic starts here
CCOF.License.Form = {
    onLoad: function (executionContext) {
        this.filterFundingLookup(executionContext);
    },

    filterFundingLookup: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var facility = formContext.getAttribute("ccof_facility").getValue();

        if (facility && facility.length > 0) {
            var facilityId = facility[0].id.replace("{", "").replace("}", "");

            // Fetch the parent account of selected account
            Xrm.WebApi.retrieveRecord("account", facilityId, "?$select=_parentaccountid_value").then(
                function success(result) {
                    var orgId = result._parentaccountid_value;
                    if (orgId) {
                        var fetchFilter = "<filter type='and'><condition attribute='ccof_organization' operator='eq' value='" + orgId + "' /></filter >";

                        formContext.getControl("ccof_associated_funding_agreement_number").addPreSearch(function () {
                            formContext.getControl("ccof_associated_funding_agreement_number").addCustomFilter(fetchFilter);
                        });

                    }
                    else {
                        console.log("No parent account found.");
                    }
                    
                },
                function (error) {
                    console.log("Error retrieving parent account: " + error.message);
                }
            );
        }
    }
}