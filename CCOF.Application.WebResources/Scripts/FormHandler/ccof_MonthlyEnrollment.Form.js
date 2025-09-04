﻿
var CCOF = CCOF || {};
CCOF.MonthlyEnrollment = CCOF.MonthlyEnrollment || {};
CCOF.MonthlyEnrollment.Form = CCOF.MonthlyEnrollment.Form || {};
CCOF.MonthlyEnrollment.Form = {
    onLoad: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        var reportType = formContext.getAttribute("ccof_reporttype").getValue();

        // if (formContext.getAttribute("ccof_locked").getValue()) {
        // 	formContext.getControl("ccof_locked").setDisabled(false);
        // } else {
        // 	formContext.getControl("ccof_locked").setDisabled(true);
        // }
        formContext.getAttribute("ccof_locked").addOnChange(onChange_locked);
        formContext.getAttribute("ccof_ccof_base_verification").addOnChange(onChange_CCOFBaseVerification);
        formContext.getAttribute("ccof_ccfri_verification").addOnChange(onChange_CCFRIVerification);
        formContext.getAttribute("ccof_rejectreason").addOnChange(onChange_RejectReason);
        formContext.getAttribute("ccof_ccof_internal_status").addOnChange(onChange_CCOFInternalStatus);
        formContext.getAttribute("ccof_ccfri_internal_status").addOnChange(onChange_CCFRIInternalStatus);          

        if (reportType === 100000000) // Baseline
        {
            formContext.ui.tabs.get("allAdjustmentER").setVisible(true);
            formContext.getControl("ccof_originalenrollmentreport").setVisible(false);
            formContext.getControl("ccof_prevenrollmentreport").setVisible(false);
        }
        else // Adjustment
        {
            formContext.ui.tabs.get("allAdjustmentER").setVisible(false);
            formContext.getControl("ccof_originalenrollmentreport").setVisible(true);
            formContext.getControl("ccof_prevenrollmentreport").setVisible(true);
        }
        if (formContext.getAttribute("ccof_ccof_base_verification").getValue() === 101510002) { // Reject
            formContext.getControl("ccof_rejectreason").setVisible(true);
            formContext.getControl("ccof_internalreason").setVisible(true);
            formContext.getAttribute("ccof_rejectreason").setRequiredLevel("required")
            let rejectReson = formContext.getAttribute("ccof_rejectreason").getValue();
            if (rejectReson && rejectReson.includes(101510008)) { // Others
                formContext.getControl("ccof_rejectreasonother").setVisible(true);
                formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("required")

            } else {
                formContext.getControl("ccof_rejectreasonother").setVisible(false);
                formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("none")
            }
        } else {
            formContext.getControl("ccof_rejectreason").setVisible(false);
            formContext.getAttribute("ccof_rejectreason").setRequiredLevel("none")
            formContext.getControl("ccof_internalreason").setVisible(false);
            formContext.getControl("ccof_rejectreasonother").setVisible(false);
            formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("none")
        }
        if (formContext.getAttribute("ccof_ccof_internal_status").getValue() === 7)  formContext.getControl("ccof_ccof_base_verification").setDisabled(true);  // "Approved for payment"
        if (formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 7) formContext.getControl("ccof_ccfri_verification").setDisabled(true);      // "Approved for payment"        
    },
    onSave: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        if ((formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 1 || formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 2)
            && (formContext.getAttribute("ccof_ccof_internal_status").getValue() === 1 || formContext.getAttribute("ccof_ccof_internal_status").getValue() === 2)) {
            let today = new Date();
            today.setHours(0, 0, 0, 0);
            let submissionDeadline = formContext.getAttribute("ccof_submissiondeadline").getValue();
            submissionDeadline.setHours(0, 0, 0, 0);
            if (submissionDeadline < today) {
                formContext.ui.setFormNotification("The date must be greater than today.", "ERROR", "date_check");
                if (executionContext.getEventArgs()) {
                    executionContext.getEventArgs().preventDefault();
                }
            }
            else {
                formContext.ui.clearFormNotification("date_check");
            }
        }
    }
}
function onChange_locked(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_locked").getValue()) {
        // formContext.getControl("ccof_locked").setDisabled(true);
        formContext.getAttribute("ccof_submissiondeadline").setValue(null);
        formContext.getAttribute("ccof_submissiondeadline").setRequiredLevel("required")
        formContext.getAttribute("ccof_lockedunlockedreason").setRequiredLevel("required")
        formContext.getAttribute("ccof_ccfri_internal_status").setValue(1); // Created
        formContext.getAttribute("ccof_ccof_internal_status").setValue(1); // Created

        let alertStrings = { confirmButtonLabel: "OK", text: "Please input unlock reason and reset submission deadline and save it!" };
        let alertOptions = { height: 120, width: 260 };
        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
            function () {
                console.log("Alert closed");
            },
            function (error) {
                console.log("Error showing alert: ", error.message);
            }
        );
    }
}
function onChange_CCOFBaseVerification(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccof_base_verification").getValue() === 101510002) { // Reject
        formContext.getControl("ccof_rejectreason").setVisible(true);
        formContext.getAttribute("ccof_rejectreason").setRequiredLevel("required")
        formContext.getControl("ccof_internalreason").setVisible(true);
        if (formContext.getAttribute("ccof_ccfri_verification") != 101510002) {
            formContext.getAttribute("ccof_ccfri_verification").setValue(101510002); // set CCFRI Verification to Reject
        }
    } else {
        formContext.getAttribute("ccof_rejectreason").setRequiredLevel("none")
        formContext.getAttribute("ccof_rejectreason").setValue(null);
        formContext.getControl("ccof_rejectreason").setVisible(false);
        formContext.getControl("ccof_rejectreasonother").setVisible(false);
        formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("none")
        formContext.getAttribute("ccof_rejectreasonother").setValue(null);
        formContext.getControl("ccof_internalreason").setVisible(false);
        formContext.getAttribute("ccof_internalreason").setValue(null);
        if (formContext.getAttribute("ccof_ccfri_verification").getValue() === 101510002) { // Reject
            formContext.getAttribute("ccof_ccfri_verification").setValue(null); // set CCFRI Verification to null
        }
    }
}
function onChange_CCFRIVerification(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccfri_verification").getValue() === 101510002) { // Reject
        formContext.getControl("ccof_rejectreason").setVisible(true);
        formContext.getAttribute("ccof_rejectreason").setRequiredLevel("required")
        formContext.getControl("ccof_internalreason").setVisible(true);
        if (formContext.getAttribute("ccof_ccof_base_verification") != 101510002) {
            formContext.getAttribute("ccof_ccof_base_verification").setValue(101510002); //  set Base Verification to Reject
        }
    } else {
        formContext.getAttribute("ccof_rejectreason").setRequiredLevel("none")
        formContext.getAttribute("ccof_rejectreason").setValue(null);
        formContext.getControl("ccof_rejectreason").setVisible(false);
        formContext.getControl("ccof_rejectreasonother").setVisible(false);
        formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("none")
        formContext.getAttribute("ccof_rejectreasonother").setValue(null);
        formContext.getControl("ccof_internalreason").setVisible(false);
        formContext.getAttribute("ccof_internalreason").setValue(null);
        if (formContext.getAttribute("ccof_ccof_base_verification").getValue() === 101510002) {
            formContext.getAttribute("ccof_ccof_base_verification").setValue(null); //  set Base Verification to Reject
        }
    }
}
function onChange_RejectReason(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    let rejectReson = formContext.getAttribute("ccof_rejectreason").getValue();
    if (rejectReson && rejectReson.includes(101510008)) { // Others
        formContext.getControl("ccof_rejectreasonother").setVisible(true);
        formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("required")
    } else {
        formContext.getControl("ccof_rejectreasonother").setVisible(false);
        formContext.getAttribute("ccof_rejectreasonother").setValue(null);
        formContext.getAttribute("ccof_rejectreasonother").setRequiredLevel("none")
    }
}
function onChange_CCOFInternalStatus(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccof_internal_status").getValue() === 7)  formContext.getControl("ccof_ccof_base_verification").setDisabled(true);  // "Approved for payment"
}
function onChange_CCFRIInternalStatus(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 7) formContext.getControl("ccof_ccfri_verification").setDisabled(true);      // "Approved for payment"
}