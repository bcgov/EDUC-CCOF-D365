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
            formContext.getControl("ccof_adjustment_created_by").setVisible(false);
        }
        else // Adjustment
        {
            formContext.ui.tabs.get("allAdjustmentER").setVisible(false);
            formContext.getControl("ccof_originalenrollmentreport").setVisible(true);
            formContext.getControl("ccof_prevenrollmentreport").setVisible(true);
            formContext.getControl("ccof_adjustment_created_by").setVisible(true);
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
        if (formContext.getAttribute("ccof_ccof_internal_status").getValue() === 7) formContext.getControl("ccof_ccof_base_verification").setDisabled(true);  // "Approved for payment"
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
    },
    CreateAdjustmentER: function (primaryControl) {
        debugger;
        let formContext = primaryControl;
        let entityId = formContext.data.entity.getId();
        entityId = getCleanedGuid(entityId);
        let userSettings = Xrm.Utility.getGlobalContext().userSettings;
        let currentuserid = userSettings.userId;
        let username = userSettings.userName;
        let CCFRIStatus = formContext.getAttribute("ccof_ccfri_internal_status").getValue();
        let CCOFStatus = formContext.getAttribute("ccof_ccof_internal_status").getValue();

        let CCFRIStatus_Eligible = [1, 2, 3, 4, 6];        // 1-Created, 2-Incomplete, 3-Submitted, 4-Review, 6*-Verified
        let CCFRIStatus_Warning = [5, 10];                 // 5-Rejected, 10-Expired	
        let CCFRIStatus_Prohibited = [7, 8, 9];            // 7-Approved for payment, 8-Paid, 9-Processing Error	

        let CCOFStatus_Eligible = [1, 2, 3, 4, 6];         // 1-Created, 2-Incomplete, 3-Submitted, 4-Review, 6*-Verified
        let CCOFStatus_Warning = [5, 10];                  // 5-Rejected, 10-Expired	
        let CCOFStatus_Prohibited = [7, 8, 9];             // 7-Approved for payment, 8-Paid, 9-Processing Error	

        isCreateAdjusment = false;
        if (CCFRIStatus_Prohibited.includes(CCFRIStatus) && CCOFStatus_Prohibited.includes(CCOFStatus)) {
            isCreateAdjusment = true;
        }
        if (!isCreateAdjusment) {
            let alertStrings = { confirmButtonLabel: "Ok", text: "You are not allowed to create adjustment enrolment report for it. Please verify if the CCFRI & CCOF statuses are eligible to proceed.", title: "Adjustment Enrolment Report Creation is prohibited" };
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
        let facilityId = formContext.getAttribute("ccof_facility").getValue()[0].id;
        let year = formContext.getAttribute("ccof_year").getValue();
        let month = formContext.getAttribute("ccof_month").getValue();
        let reportVersion = formContext.getAttribute("ccof_reportversion").getValue();
        let latestER = getSyncMultipleRecord("ccof_monthlyenrollmentreports?$select=ccof_ccfri_external_status,ccof_ccfri_internal_status,ccof_ccof_external_status,ccof_ccof_internal_status,ccof_name,ccof_reportversion,ccof_year&$filter=(_ccof_facility_value eq " + facilityId + " and ccof_month eq " + month + " and ccof_year eq '" + year + "' and statecode eq 0)&$orderby=ccof_reportversion desc");
        if (latestER.length === 0 || reportVersion != latestER[0]["ccof_reportversion"]) {
            let alertStrings = { confirmButtonLabel: "Ok", text: "You are not allowed to create adjustment enrolment report for it. This is not latest Enrolment Report.", title: "Adjustment Enrolment Report Creation is prohibited" };
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
        var confirmStrings = {
            title: "Confirm Creating Adjusment Monthly Enrolment Report",
            text: "Are you sure you want to create adjustment enrolment report for this record? Please click Yes button to continue, or click No button to cancel.",
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        };
        var confirmOptions = { height: 240, width: 520 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    formContext.ui.setFormNotification("Creating Adjustment ER...", "INFO", "AERCreation");
                    let flowUrl;
                    let result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_AdjustmentERCreation') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
                    flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;
                    let body = {
                        "ERGuid": entityId,
                        "targetRecordGuid": getCleanedGuid(currentuserid),
                        "targetName": username,
                        "targetEntitySetName": "systemusers",
                        "targetEntityLogicalName": "systemuser"
                    };
                    // let flowUrl = "https://1a49df49f24be835ab86dc8d9c0010.f5.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/e770ae8aae7a4c37bc401dfa3783b988/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=bLYcH5CkLCwmM2akaqlXmmOfd9lzE9oTaIXzpy97yds";
                    var input = JSON.stringify(body);
                    var req = new XMLHttpRequest();
                    req.open("POST", flowUrl, true);
                    req.setRequestHeader('Content-Type', 'application/json');
                    req.onreadystatechange = function () {
                        if (this.readyState === 4) {
                            req.onreadystatechange = null;
                            if (this.status === 200) {
                                debugger;
                                var result = this.response;
                                formContext.ui.setFormNotification("Adjustment Enrolment Report is complete", "INFO", "AERCreation");
                                formContext.ui.clearFormNotification("AERCreation");
                                Xrm.Navigation.openAlertDialog(result);
                            }
                            else if (this.status === 400) {
                                Xrm.Utility.closeProgressIndicator();
                                formContext.ui.clearFormNotification("AERCreation");
                                var result = "There are something error! Please contact administrator!\n" + this.response;
                                var alertStrings = { confirmButtonLabel: "Ok", text: result, title: "Error!" };
                                var alertOptions = { height: 240, width: 520 };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                            }
                        }
                    };
                    req.send(input);
                }
                else {
                    console.log("The Adjusment ER Creation does NOT proceed");
                }
            },
            function (error) {
                Xrm.Navigation.openErrorDialog({ message: error });
            });

    },
    RecalculateER: function (primaryControl) {
        debugger;
        let formContext = primaryControl;
        let entityId = formContext.data.entity.getId();
        entityId = getCleanedGuid(entityId);
        var userSettings = Xrm.Utility.getGlobalContext().userSettings;
        var currentuserid = userSettings.userId;
        var username = userSettings.userName;
        let CCFRIStatus = formContext.getAttribute("ccof_ccfri_internal_status").getValue();
        let CCOFStatus = formContext.getAttribute("ccof_ccof_internal_status").getValue();

        let CCFRIStatus_Eligible = [1, 2, 3, 4, 6];       // 1-Created, 2-Incomplete, 3-Submitted, 4-Review, 6*-Verified
        let CCFRIStatus_Warning = [5, 10];                // 5-Rejected, 10-Expired	
        let CCFRIStatus_Prohibited = [7, 8, 9];           // 7-Approved for payment, 8-Paid, 9-Processing Error	

        let CCOFStatus_Eligible = [1, 2, 3, 4, 6];        // 1-Created, 2-Incomplete, 3-Submitted, 4-Review, 6*-Verified
        let CCOFStatus_Warning = [5, 10];                 // 5-Rejected, 10-Expired	
        let CCOFStatus_Prohibited = [7, 8, 9];            // 7-Approved for payment, 8-Paid, 9-Processing Error	

        isRecalcAllowed = false;
        if (CCFRIStatus_Eligible.includes(CCFRIStatus) && CCOFStatus_Eligible.includes(CCOFStatus)) {
            isRecalcAllowed = true;
        }
        if (!isRecalcAllowed) {
            let alertStrings = { confirmButtonLabel: "Ok", text: "You are not allowed to perform recalculation. Please verify if the CCFRI & CCOF statuses are eligible to proceed.", title: "Recalculation is prohibited" };
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

        var confirmStrings = {
            title: "Confirm Recalculation",
            text: "Are you sure you want to perform recalculation on this record? Please click Yes button to continue, or click No button to cancel.",
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        };
        var confirmOptions = { height: 240, width: 520 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    formContext.ui.setFormNotification("Recalculating ER...", "INFO", "RecalculatingER");
                    let flowUrl;
                    let result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_RecalculateER') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
                    flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;
                    let body = {
                        "ERGuid": entityId,
                        "targetRecordGuid": getCleanedGuid(currentuserid),
                        "targetName": username,
                        "targetEntitySetName": "systemusers",
                        "targetEntityLogicalName": "systemuser"
                    };
                    // let flowUrl = "https://1a49df49f24be835ab86dc8d9c0010.f5.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/e770ae8aae7a4c37bc401dfa3783b988/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=bLYcH5CkLCwmM2akaqlXmmOfd9lzE9oTaIXzpy97yds";
                    var input = JSON.stringify(body);
                    var req = new XMLHttpRequest();
                    req.open("POST", flowUrl, true);
                    req.setRequestHeader('Content-Type', 'application/json');
                    req.onreadystatechange = function () {
                        if (this.readyState === 4) {
                            req.onreadystatechange = null;
                            if (this.status === 200) {
                                debugger;
                                var result = this.response;
                                if (formContext.getAttribute("ccof_ccfri_internal_status").getValue() == 6 || formContext.getAttribute("ccof_ccof_internal_status").getValue() == 6) {
                                    formContext.getAttribute("ccof_ccfri_internal_status").setValue(4);           // 4-Review, 6*-Verified
                                    formContext.getAttribute("ccof_ccof_internal_status").setValue(4);            // 4-Review, 6*-Verified
                                    formContext.getAttribute("ccof_ccof_base_verification").setValue(101510001);  // 101510001-Undo Verify
                                    formContext.getAttribute("ccof_ccfri_verification").setValue(101510001);      // 101510001-Undo Verify                                       
                                    formContext.data.entity.save();
                                }
                                formContext.ui.setFormNotification("Recalculating ER is complete", "INFO", "RecalculatingER");
                                formContext.ui.clearFormNotification("RecalculatingER");
                                Xrm.Navigation.openAlertDialog(result);
                            }
                            else if (this.status === 400) {
                                Xrm.Utility.closeProgressIndicator();
                                formContext.ui.clearFormNotification("RecalculatingER");
                                var result = "There are something error! Please contact administrator!\n" + this.response;
                                var alertStrings = { confirmButtonLabel: "Ok", text: result, title: "Error!" };
                                var alertOptions = { height: 240, width: 520 };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                            }
                        }
                    };
                    req.send(input);
                }
                else {
                    console.log("The recalculation does NOT proceed");
                }
            },
            function (error) {
                Xrm.Navigation.openErrorDialog({ message: error });
            }
        );
    },
    showHideCreateAdjustmentERButton: function (primaryControl) {
        debugger;
        var formContext = primaryControl;

        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Accounts" || item.name === "CCOF - Sr. Accounts" || item.name === "CCOF - Sr. Adjudicator"
                || item.name === "CCOF - Mod QC" || item.name === "CCOF - Leadership") {
                visible = true;
            }
        });
        return visible;
    },
    showHideRecalculateButton: function (primaryControl) {
        debugger;
        var formContext = primaryControl;

        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Accounts" || item.name === "CCOF - Sr. Accounts" || item.name === "CCOF - Sr. Adjudicator"
                || item.name === "CCOF - Mod QC" || item.name === "CCOF - Leadership") {
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
        var userSettings = Xrm.Utility.getGlobalContext().userSettings;
        var currentuserid = userSettings.userId;
        var username = userSettings.userName;

        let activePaymentLines = getSyncMultipleRecord("ofm_payments?$select=_ccof_coding_line_type_value,_ccof_invoice_value,_ccof_monthly_enrollment_report_value,_ofm_application_value,_ofm_facility_value,ofm_name,ofm_payment_type,statecode,statuscode&$filter=(statecode eq 0 and _ccof_monthly_enrollment_report_value eq " + entityId + ")&$orderby=_ccof_invoice_value desc");
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
            title: "Confirm Undo Paymentline Approval",
            text: "Are you sure you want to undo paymentline approval  for this record? Please click Yes button to continue, or click No button to cancel.",
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        };
        let confirmOptions = { height: 240, width: 520 };
        let isCCOF = false;
        let isCCFRI = false;
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    let promises = [];

                    activePaymentLines.forEach(function (record) {
                        let data = {
                            statecode: 1,
                            statuscode: 7 // Cancelled
                        };
                        if (record["ofm_payment_type"] === 7) { // CCOF
                            isCCOF = true;
                        };
                        if (record["ofm_payment_type"] === 8 || record["ofm_payment_type"] === 10) { // CCFRI, CCFRI Provider 
                            isCCFRI = true;
                        };
                        promises.push(
                            Xrm.WebApi.updateRecord("ofm_payment", record["ofm_paymentid"], data)
                        );
                    }
                    )
                    if (isCCOF) {
                        formContext.getAttribute("ccof_ccof_internal_status").setValue(4); // Reveiw
                        formContext.getAttribute("ccof_ccof_base_verification").setValue(null);
                        formContext.getAttribute("ccof_qr_verified_by_ccof").setValue(null);
                        formContext.getAttribute("ccof_qr_verified_on_ccof").setValue(null);
                    };
                    if (isCCFRI) {
                        formContext.getAttribute("ccof_ccfri_internal_status").setValue(4); // Reveiw
                        formContext.getAttribute("ccof_ccfri_verification").setValue(null);
                        formContext.getAttribute("ccof_qr_verified_by_ccfri").setValue(null);
                        formContext.getAttribute("ccof_qr_verified_on_ccfri").setValue(null);
                    };
                }
                else {
                    console.log("The Undo Payment Approval does NOT proceed");
                }
            },
            function (error) {
                Xrm.Navigation.openErrorDialog({ message: error });
            }
        );
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
    let userSettings = Xrm.Utility.getGlobalContext().userSettings;
    let currentUser = new Array();
    currentUser[0] = new Object();
    currentUser[0].entityType = "systemuser";
    currentUser[0].id = userSettings.userId;
    currentUser[0].name = userSettings.userName;
    let currentDateTime = new Date();
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccof_base_verification").getValue() === 101510002) { // Reject
        formContext.getControl("ccof_rejectreason").setVisible(true);
        formContext.getAttribute("ccof_rejectreason").setRequiredLevel("required")
        formContext.getControl("ccof_internalreason").setVisible(true);
        if (formContext.getAttribute("ccof_ccfri_verification").getValue() != 101510002) {
            formContext.getAttribute("ccof_ccfri_verification").setValue(101510002);         // set CCFRI Verification to Reject
            formContext.getAttribute("ccof_qr_verified_by_ccfri").setValue(null);     // set lookup to current user
            formContext.getAttribute("ccof_qr_verified_on_ccfri").setValue(null); // set now() to verification date             
        }
        formContext.getAttribute("ccof_qr_verified_by_ccof").setValue(null);          // set lookup to  null
        formContext.getAttribute("ccof_qr_verified_on_ccof").setValue(null);      // set now() to verification null    
        formContext.getControl("ccof_qr_verified_by_ccof").setDisabled(false);         // set unlocked
        formContext.getControl("ccof_qr_verified_on_ccof").setDisabled(false);  	//set unlocked  
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
            formContext.getAttribute("ccof_ccfri_verification").setValue(null);             // set CCFRI Verification to null
            formContext.getAttribute("ccof_qr_verified_by_ccfri").setValue(null);           // set lookup to null
            formContext.getAttribute("ccof_qr_verified_on_ccfri").setValue(null);           // set now() to null  	
            formContext.getControl("ccof_qr_verified_by_ccfri").setDisabled(false);         // set unlocked
            formContext.getControl("ccof_qr_verified_on_ccfri").setDisabled(false);  	//set unlocked  		
        }
        if (formContext.getAttribute("ccof_ccof_base_verification").getValue() === 101510000) { //verrified
            formContext.getAttribute("ccof_qr_verified_by_ccof").setValue(currentUser);         // set lookup to current user
            formContext.getAttribute("ccof_qr_verified_on_ccof").setValue(currentDateTime);     // set now() to verification date   
            formContext.getControl("ccof_qr_verified_by_ccof").setDisabled(true);         // set locked
            formContext.getControl("ccof_qr_verified_on_ccof").setDisabled(true);  	//setlocked
        } else {
            formContext.getAttribute("ccof_qr_verified_by_ccof").setValue(null);         // set lookup to current user
            formContext.getAttribute("ccof_qr_verified_on_ccof").setValue(null);     // set now() to verification date 
            formContext.getControl("ccof_qr_verified_by_ccof").setDisabled(false);         // set unlocked
            formContext.getControl("ccof_qr_verified_on_ccof").setDisabled(false);  	//set unlocked  
        }

    }
}
function onChange_CCFRIVerification(executionContext) {
    debugger;
    let userSettings = Xrm.Utility.getGlobalContext().userSettings;
    let currentUser = new Array();
    currentUser[0] = new Object();
    currentUser[0].entityType = "systemuser";
    currentUser[0].id = userSettings.userId;
    currentUser[0].name = userSettings.userName;
    let currentDateTime = new Date();
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccfri_verification").getValue() === 101510002) { // Reject
        formContext.getControl("ccof_rejectreason").setVisible(true);
        formContext.getAttribute("ccof_rejectreason").setRequiredLevel("required")
        formContext.getControl("ccof_internalreason").setVisible(true);
        if (formContext.getAttribute("ccof_ccof_base_verification").getValue() != 101510002) {
            formContext.getAttribute("ccof_ccof_base_verification").setValue(101510002);       // set Base Verification to Reject
            formContext.getAttribute("ccof_qr_verified_by_ccof").setValue(null);        // set lookup to current user
            formContext.getAttribute("ccof_qr_verified_on_ccof").setValue(null);    // set now() to verification date            
        }
        formContext.getAttribute("ccof_qr_verified_by_ccfri").setValue(null);           // set lookup to current user
        formContext.getAttribute("ccof_qr_verified_on_ccfri").setValue(null);       // set now() to verification date
        formContext.getControl("ccof_qr_verified_by_ccfri").setDisabled(false);         // set unlocked
        formContext.getControl("ccof_qr_verified_on_ccfri").setDisabled(false);  	//set unlocked           
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
            formContext.getAttribute("ccof_ccof_base_verification").setValue(null);            // set Base Verification to Reject
            formContext.getAttribute("ccof_qr_verified_by_ccof").setValue(null);               // set lookup to null
            formContext.getAttribute("ccof_qr_verified_on_ccof").setValue(null);               // set now() to null 	
            formContext.getControl("ccof_qr_verified_by_ccof").setDisabled(false);         // set unlocked
            formContext.getControl("ccof_qr_verified_on_ccof").setDisabled(false);  	//set unlocked  	
        }
        if (formContext.getAttribute("ccof_ccfri_verification").getValue() === 101510000) {   //verify
            formContext.getAttribute("ccof_qr_verified_by_ccfri").setValue(currentUser);           // set lookup to current user
            formContext.getAttribute("ccof_qr_verified_on_ccfri").setValue(currentDateTime);       // set now() to verification date     
            formContext.getControl("ccof_qr_verified_by_ccfri").setDisabled(true);         // set locked
            formContext.getControl("ccof_qr_verified_on_ccfri").setDisabled(true);  	//setlocked
        } else {
            formContext.getAttribute("ccof_qr_verified_by_ccfri").setValue(null);           // set lookup to current user
            formContext.getAttribute("ccof_qr_verified_on_ccfri").setValue(null);
            formContext.getControl("ccof_qr_verified_by_ccfri").setDisabled(false);         // set unlocked
            formContext.getControl("ccof_qr_verified_on_ccfri").setDisabled(false);  	//set unlocked   

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
    if (formContext.getAttribute("ccof_ccof_internal_status").getValue() === 7) formContext.getControl("ccof_ccof_base_verification").setDisabled(true);  // "Approved for payment"
}
function onChange_CCFRIInternalStatus(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 7) formContext.getControl("ccof_ccfri_verification").setDisabled(true);      // "Approved for payment"
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