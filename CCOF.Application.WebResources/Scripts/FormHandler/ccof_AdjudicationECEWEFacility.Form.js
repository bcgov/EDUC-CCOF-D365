// A webresource for BaseFunding only

//Create Namespace Object if its defined
var AdjECEWEFacility = AdjECEWEFacility || {};
AdjECEWEFacility.ECEWEFac = AdjECEWEFacility.ECEWEFac || {};
AdjECEWEFacility.ECEWEFac.Form = AdjECEWEFacility.ECEWEFac.Form || {};

//Formload logic starts here
AdjECEWEFacility.ECEWEFac.Form = {
    onLoad: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate

            case 2: // update                           
                this.getTypeOfForm();
                break;
            case 3: //readonly
                break;
            case 4: //disable
                break;
            case 6: //bulkedit
                break;
        }
    },


    //A function called on save
    onSave: function (executionContext) {

    },

    getTypeOfForm: function () {
        debugger;
        var optInSate = Xrm.Page.getAttribute("ccof_optinstartdate").getValue();
        console.log("optInSate " + optInSate);
        if (optInSate !== null) {
            var yearDOB = optInSate.getFullYear().toString();
            var monthDOB = (optInSate.getMonth() + 1);
            console.log("monthDOB " + monthDOB);
            if (monthDOB >= 4 && monthDOB <= 12) {
                monthDOB = monthDOB - 3;
                console.log("monthDOB - 3 " + monthDOB);
            }
            else if (monthDOB <= 3) {
                monthDOB = monthDOB + 9;
                console.log("monthDOB + 9 " + monthDOB);
            }
            var dayDOB = optInSate.getDate().toString();
            Xrm.Page.getAttribute("ccof_optinstartmonth").setValue(monthDOB);
            //formContext.getAttribute("ccof_optinstartmonth").setValue(monthDOB);
        }
        else { }
    }
};

function enableDisableEceweDecision() {
    debugger;
    var applicationEcewe = Xrm.Page.getAttribute('ccof_adjudicationccfrifacility').getValue();
    if (applicationEcewe != null || applicationEcewe != undefined) {
        var applicationEceweId = applicationEcewe[0].id;
        console.log(applicationEceweId);
        Xrm.WebApi.retrieveRecord("ccof_adjudication_ccfri_facility", applicationEceweId, "?$select=ccof_ccfriqcdecision").then(
            function success(result) {
                var ccfriQcDecisionVal = result.ccof_ccfriqcdecision;
                console.log("Retrieved values: CcfriQC Decision: " + ccfriQcDecisionVal);
                // fix bug based on ticket 1987
                if (ccfriQcDecisionVal != null || ccfriQcDecisionVal != undefined) {
                    Xrm.Page.getControl('ccof_ecewedecision').setDisabled(false);
                   // Xrm.Page.getControl('ccof_ecewedecision').setDisabled(true);
                }
                else {
                    Xrm.Page.getControl('ccof_ecewedecision').setDisabled(true);
                    //Xrm.Page.getControl('ccof_ecewedecision').setDisabled(false);
                }
            },
            function (error) {
                console.log(error.message);
                // handle error conditions
            }
        );
    }
    else { }
}

function onRecordSelect(exeContext) {
    debugger;
    var _formContext = exeContext.getFormContext();
    var disableFields = ["ccof_ecewedecision"];
    var ccfriQCDecisionVal = _formContext.getAttribute("ccof_ccfriqcdecision").getValue();
    console.log("ccfriQCDecisionVal " + ccfriQCDecisionVal);
    // fix bug based on ticket 1987
    if (ccfriQCDecisionVal === null || ccfriQCDecisionVal === undefined) {
 //       unLockFields(exeContext, disableFields);
        lockFields(exeContext, disableFields);

    }
    else {
 //       lockFields(exeContext, disableFields);
        unLockFields(exeContext, disableFields);
    }
}

function lockFields(exeContext, disableFields) {
    var _formContext = exeContext.getFormContext();
    var currentEntity = _formContext.data.entity;
    currentEntity.attributes.forEach(function (attribute, i) {
        if (disableFields.indexOf(attribute.getName()) > -1) {
            var attributeToDisable = attribute.controls.get(0);
            attributeToDisable.setDisabled(true);
        }
    });
}

function unLockFields(exeContext, disableFields) {
    var _formContext = exeContext.getFormContext();
    var currentEntity = _formContext.data.entity;
    currentEntity.attributes.forEach(function (attribute, i) {
        if (disableFields.indexOf(attribute.getName()) > -1) {
            var attributeToDisable = attribute.controls.get(0);
            attributeToDisable.setDisabled(false);
        }
    });
}