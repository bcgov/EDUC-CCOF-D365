// A webresource for AccountRibbon only

//Create Namespace Object if its defined
var CCOF = CCOF || {};
CCOF.ECE_MER = CCOF.ECE_MER || {};
CCOF.ECE_MER.Ribbon = CCOF.ECE_MER.Ribbon || {};

CCOF.ECE_MER.Ribbon = {
    // description
    RecalculateMER: function (primarycontrol, primaryEntiityId) {
         
        var gridContext = primarycontrol;
        var pageInput = {
            pageType: "custom",
            name: "ccof_monthlyecereportrecalculatehours_7e8cc",
            recordId: primaryEntiityId
        };
        var navigationOptions = {
            target: 2,
            position: 1,
            height: 306,
            width: 703,
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    primarycontrol?.refresh();
                    console.log("Success");
                }
            ).catch(
                function (error) {
                    console.log(Error);
                }
            );
    },
    BulkApproveMER: function (selectedControl, selectedItemIds) {
        debugger;
        var gridContext = selectedControl;
        var pageInput = {
            pageType: "custom",
            name: "ccof_bulkapproveecemonthlyreports_5ed7d",
            recordId: selectedItemIds
        };
        var navigationOptions = {
            target: 2,
            position: 1,
            height: 900,
            width: {value: 75, unit:"%"},
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    gridContext.refresh();
                    console.log("Success");
                }
            ).catch(
                function (error) {
                    console.log(Error);
                }
            );
    },
    showHideBulkApproveMER: function (selectedControl) {
        debugger;
        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Sr. Accounts" || item.name === "System Administrator") {
                visible = true;
            }
        });

        return visible;
    },
    UndoPaymentApproval: function (primaryControl) {
        debugger;
        let formContext = primaryControl;
        let entityId = formContext.data.entity.getId();
        entityId = getCleanedGuid(entityId);
        let activePaymentLines = getSyncMultipleRecord("ofm_payments?$select=_ccof_coding_line_type_value,_ccof_invoice_value,_ccof_monthly_ecewe_report_value,_ofm_application_value,_ofm_facility_value,ofm_name,ofm_payment_type,statecode,statuscode&$filter=(statecode eq 0 and _ccof_monthly_ecewe_report_value eq " + entityId + ")&$orderby=_ccof_invoice_value desc");
        if (activePaymentLines.length === 0 || activePaymentLines[0]["_ccof_invoice_value"] != null) {
            let alertStrings = { confirmButtonLabel: "Ok", text: "You cannot undo the payment approval because no payment lines were generated, or the payments have already been invoiced.", title: "Undo Paymentlines Approval is prohibited" };
            let alertOptions = { height: 240, width: 520 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function (success) {
                    console.log("Alert dialog closed");
                },
                function (error) {
                    console.log(error.message);
                }
            );
            return;
        }
        let confirmStrings = {
            title: "Confirm Undo Payment Approval",
            text: "Are you sure you want to undo payment approval for this record? Please click Yes button to continue, or click No button to cancel.",
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        };
        let confirmOptions = { height: 240, width: 520 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    let promises = [];
                    activePaymentLines.forEach(function (record) {
                        let data = {
                            statecode: 1,
                            statuscode: 7 // Cancelled
                        };
                        promises.push(
                            Xrm.WebApi.updateRecord("ofm_payment", record["ofm_paymentid"], data)
                        );
                      }
					)
					formContext.getAttribute("statuscode").setValue(4);            // 4 = Review
					formContext.getAttribute("ccof_verified_by").setValue(null);
					formContext.getAttribute("ccof_verification_date").setValue(null);
					formContext.getAttribute("ccof_ministry_approver").setValue(null);
					formContext.getAttribute("ccof_approval_date").setValue(null);		
					
                    formContext.data.save();
                }
                else {
                    console.log("The Undo Payment Approval does NOT proceed");
                }
            },
            function (error) {
                Xrm.Navigation.openErrorDialog({ message: error });
            }
        );
    },
    showHideUndoPaymentApprovalButton: function (primaryControl) {
        debugger;
        let formContext = primaryControl;
        let visible = false;
        let userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Sr. Accounts" || item.name === "CCOF - Admin" ||item.name === "System Administrator") {
                visible = true;
            }
        });
        if (!visible) return visible;
        let entityId = formContext.data.entity.getId();
        entityId = getCleanedGuid(entityId);
        let activePaymentLines = getSyncMultipleRecord("ofm_payments?$select=_ccof_coding_line_type_value,_ccof_invoice_value,_ccof_monthly_ecewe_report_value,_ofm_application_value,_ofm_facility_value,ofm_name,ofm_payment_type,statecode,statuscode&$filter=(statecode eq 0 and _ccof_monthly_ecewe_report_value eq " + entityId + ")&$orderby=_ccof_invoice_value desc");
        if (activePaymentLines.length === 0 || activePaymentLines[0]["_ccof_invoice_value"] != null) {
            visible = false;
        } else {
            visible = true;
        }
        return visible;
    }
}
function getCleanedGuid(id) {
    return id.replace("{", "").replace("}", "");
}
function getSyncMultipleRecord(request) {
    var result = null;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/" + request, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                result = results.value;
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
    return result;
}