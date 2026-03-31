
var CCOF = CCOF || {};
CCOF.ECEStaffInformation = CCOF.ECEStaffInformation || {};
CCOF.ECEStaffInformation.Form = CCOF.ECEStaffInformation.Form || {};

CCOF.ECEStaffInformation.Form = {
    onLoad: function (executionContext) {
		debugger;
        var formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;

            case 1: //Create/QuickCreate
                break;

            case 2: //update         
                enableFieldsBasedOnSecurityRole(executionContext);
                break;

            case 3: //readonly
                break;

            case 4: //disable
                break;

            case 6: //bulkedit
                break;
        }
    }	
}

function enableFieldsBasedOnSecurityRole(executionContext) {
    var enableFields = ["statuscode"];    // target field(s)
    var isEditAccess = false;
    var isSysAdmin = false;
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    userRoles.forEach(function hasRole(item, index) {
        if (item.name === "CCOF - Admin" || item.name === "CCOF - Accounts" || item.name === "CCOF - Sr. Accounts" || item.name === "System Administrator") {
            isEditAccess = true;
            if (item.name === "System Administrator") {
                isSysAdmin = true;
            }
        }
    });	
    
    // unlock selected fields based on security roles
    if (isEditAccess && !isSysAdmin) {
        unLockSelectedFields(executionContext, enableFields);
    }     
    if (!isEditAccess) {
        lockALLFields(executionContext);
    } 
}

function unLockSelectedFields(executionContext, enableFields) {
    var formContext = executionContext.getFormContext();
    formContext.data.entity.attributes.forEach(function (attribute, i) {
        if (enableFields.indexOf(attribute.getName()) > -1) {
            attribute.controls.get(0).setDisabled(false);    // editable
        } else {
            attribute.controls.get(0).setDisabled(true);     // readonly   
        }
    });
}

function lockALLFields(executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.data.entity.attributes.forEach(function (attribute, i) {
        attribute.controls.get(0).setDisabled(true);     // readonly   
    });
}