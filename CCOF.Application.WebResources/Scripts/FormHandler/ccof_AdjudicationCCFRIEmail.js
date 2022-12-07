
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
        var selectedRows = selectedControl.getGrid().getSelectedRows();

        console.log("Current recordId:" + formContext.data.entity.getId());

        //  alert(" :" + selectedRows.getLength());

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
            var clientURL = Xrm.Utility.getGlobalContext().getClientUrl();
            var entity = {};
            var entityArray = [];
            var subgridRecordGuid;
            var recordsCount = 0;
            selectedRows.forEach(function (row, i) {
                debugger;
                console.log(row.getData().getEntity().attributes.get("ccof_facility").getValue());
                let facility = row.getData().getEntity().attributes.get("ccof_facility").getValue();
                let emailType = row.getData().getEntity().attributes.get("ccof_emailtype").getValue();  // CCFRI Initial, Preapproval, MTFI,Temporary Approval
                console.log("emailType:" + emailType);
                console.log(row.getData().getEntity().getId());
                console.log(row.getData().getEntity().getEntityName());
                console.log(row.getData().getEntity().getPrimaryAttributeValue());
                subgridRecordGuid = row.getData().getEntity().getId().replace('{', '').replace('}', '');
                entity.ccof_facilityId = facility[0].id.replace('{', '').replace('}', '');
                entity.ccof_facilityName = facility[0].name;
                entity.ccfriEmailId = subgridRecordGuid
                entity.emailType = emailType;
                entityArray.push(entity);
                entity = {};

                //alert("subgridRecordGuid" + subgridRecordGuid);
                recordsCount = recordsCount + 1;
            });
            console.log(entityArray);
            Xrm.Utility.alertDialog("entityArray:" + JSON.stringify(entityArray));

            var flowUrl = "https://prod-11.canadacentral.logic.azure.com:443/workflows/c53af8c9c1c44fa1b8faaad89076b35f/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=NH7j0MWwxj2KjK9wY4qwKPoOC73ekzIOPltv_odXlBA";
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
                        var entityFormOptions = {};
                        entityFormOptions["entityName"] = "opportunity";
                        entityFormOptions["entityId"] = result;
                        formContext.ui.clearFormNotification("renewContract");
                        Xrm.Navigation.openForm(entityFormOptions, null).then(
                            function (lookup) { console.log("Success"); },
                            function (error) { console.log("Error"); }
                        );
                    }
                    else if (this.status === 400) {
                        formContext.ui.clearFormNotification("renewContract");
                        var result = this.response;
                        formContext.data.refresh(true);
                        alert("Error: " + result);
                    }
                }
            };

            req.send(input);

        }

    }
};
