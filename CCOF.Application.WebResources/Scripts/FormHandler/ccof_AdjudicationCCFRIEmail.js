// JavaScript source code

var AdjCCFRI = AdjCCFRI || {};
AdjCCFRI.Form = AdjCCFRI.Form || {};
AdjCCFRI.SubGrid = AdjCCFRI.SubGrid || {};
//Formload logic starts here
AdjCCFRI.SubGrid = {
    GenEmails: function (primaryControl, selectedControl) {
        debugger;
        var formContext = primaryControl;
        var entityId = formContext.data.entity.getId(); // get parent record id
        var entityPrimaryAttrValue = formContext.data.entity.getPrimaryAttributeValue(); // get parent record name
        var userSettings = Xrm.Utility.getGlobalContext().userSettings;
        var currentuserid = userSettings.userId;
        var username = userSettings.userName;
        var selectedRows = selectedControl.getGrid().getSelectedRows();
        console.log("selected record num:" + selectedRows.getLength());
        if (selectedRows.getLength() === 0) {
            Xrm.Navigation.openErrorDialog({ message: "Please select a record at least!" }).then(
                function (success) {
                    return;
                },
                function (error) {
                    console.log(error);
                });
            return;
        }
        formContext.ui.setFormNotification("Validation Primary contact email address...", "INFO", "CreatingEmails");
        // Validate Email address
        var appid = formContext.getAttribute("ccof_application").getValue()[0].id;
        var orgInfo = getSyncSingleRecord("ccof_applications(" + getCleanedGuid(appid) + ")?$select=_ccof_organization_value");
        if (orgInfo === null) {
            formContext.ui.clearFormNotification("CreatingEmails");
            Xrm.Navigation.openAlertDialog("There are no Org info,Data error!");
            return;
        }
        var bceidInfo = getSyncSingleRecord("accounts(" + orgInfo["_ccof_organization_value"] + ")?$select=_primarycontactid_value&$expand=primarycontactid($select=contactid, fullname, emailaddress1)");
        if (bceidInfo === null) {
            formContext.ui.clearFormNotification("CreatingEmails");
            Xrm.Navigation.openAlertDialog("There are no Primary Bceid info,Data error!");
            return;
        }
        if (bceidInfo["primarycontactid"]["emailaddress1"] === null||!ValidateEmail(bceidInfo["primarycontactid"]["emailaddress1"])) {
            formContext.ui.clearFormNotification("CreatingEmails");
            Xrm.Navigation.openAlertDialog("Primary Email address is illegle!");
            return;
        }
        // Xrm.Utility.showProgressIndicator("Creating draft email...")
        // formContext.ui.setFormNotification("Creating draft email...", "INFO", "CreatingEmails");
        formContext.ui.setFormNotification("Creating draft email(s)...", "WARNING", "CreatingEmails");
        var clientURL = Xrm.Utility.getGlobalContext().getClientUrl();
        var entity = {};
        var entityArray = [];
        var subgridRecordGuid;
        selectedRows.forEach(function (row, i) {
            debugger;
            let facility = row.getData().getEntity().attributes.get("ccof_facility").getValue();
            let emailType = row.getData().getEntity().attributes.get("ccof_emailtype").getValue();  // CCFRI Initial, Preapproval, MTFI,Temporary Approval
            console.log(row.getData().getEntity().getId());
            console.log(row.getData().getEntity().getEntityName());
            console.log(row.getData().getEntity().getPrimaryAttributeValue());
            subgridRecordGuid = row.getData().getEntity().getId().replace('{', '').replace('}', '');
            entity.ccof_facilityId = facility[0].id.replace('{', '').replace('}', '');
            entity.ccof_facilityName = facility[0].name;
            entity.ccfriEmailId = subgridRecordGuid
            entity.emailType = emailType;
            entity.currentuserid = currentuserid.replace('{', '').replace('}', '');
            entity.ccof_ccfriId = entityId.replace('{', '').replace('}', '');
            entityArray.push(entity);
            entity = {};
        });
        console.log(entityArray);
        //Xrm.Utility.alertDialog("entityArray:" + JSON.stringify(entityArray));

        var flowUrl;
        var result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_EmailDecisionUrl') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
        flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;

        //var flowUrl = "https://prod-11.canadacentral.logic.azure.com:443/workflows/c53af8c9c1c44fa1b8faaad89076b35f/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=NH7j0MWwxj2KjK9wY4qwKPoOC73ekzIOPltv_odXlBA";
        var input = JSON.stringify(entityArray);
        var req = new XMLHttpRequest();
        req.open("POST", flowUrl, true);
        req.setRequestHeader('Content-Type', 'application/json');
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    debugger;
                    var result = this.response;
                    //Xrm.Utility.closeProgressIndicator()
                    formContext.ui.clearFormNotification("CreatingEmails");
                    Xrm.Navigation.openAlertDialog(result)
                    //formContext.getControl('gridCCFRIEmail').refresh();
                    //var quickViewControl = Xrm.Page.ui.quickForms.get("quickViewCCFRIEmail");
                    //quickViewControl.refresh(); 
                }
                else if (this.status === 400) {
                    // Xrm.Utility.closeProgressIndicator();
                    formContext.ui.clearFormNotification("CreatingEmails");
                    var result = "There are something error! Please contact administrator!\n" + this.response;
                    var alertStrings = { confirmButtonLabel: "Ok", text: result, title: "Error!" };
                    var alertOptions = { height: 240, width: 520 };
                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                    // alert("Error: " + result);
                }
            }
        };

        req.send(input);

    }
};
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
function getCleanedGuid(id) {
    return id.replace("{", "").replace("}", "");
}
function getSyncSingleRecord(request) {
    var results = null;
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
                var result = JSON.parse(this.response);
                results = result;
            }
            else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
    return results;
}
function ValidateEmail(input) {
    var validRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;
    if (input.match(validRegex)) {
        // alert("Valid email address!");
        return true;
    } else {
        // alert("Invalid email address!");
        return false;
    }
}