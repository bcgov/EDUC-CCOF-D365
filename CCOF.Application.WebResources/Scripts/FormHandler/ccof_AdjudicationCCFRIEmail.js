
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
        }
        else {
            Xrm.Utility.showProgressIndicator("Creating draft email...")
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
            var response = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$filter=schemaname eq 'ccof_EmailDecisionUrl'");
            var flowUrl = response[0]["defaultvalue"];
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
                        Xrm.Utility.closeProgressIndicator()
                        Xrm.Navigation.openAlertDialog(result)
                        //formContext.getControl('gridCCFRIEmail').refresh();
                        //var quickViewControl = Xrm.Page.ui.quickForms.get("quickViewCCFRIEmail");
                        //quickViewControl.refresh(); 
                    }
                    else if (this.status === 400) {
                        Xrm.Utility.closeProgressIndicator()
                        var result = this.response;
                        alert("Error: " + result);
                    }
                }
            };

            req.send(input);

        }

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