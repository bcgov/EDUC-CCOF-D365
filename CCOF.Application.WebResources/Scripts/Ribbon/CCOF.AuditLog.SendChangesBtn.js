// A webresource for custom ribbon button "Send Changes"  in Audit Log 


// Call Application Audit Log flow
var flowUrl;
var result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_CCOFApplicationAuditLogSendEmailWithChangesUrl') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;
var entityName;
var recordId;

function onClickSendChangesBtn(primaryContext) {
    debugger;
    entityName = primaryContext.data.entity.getEntityName();
    recordId = getCleanedGuid(primaryContext.data.entity.getId());
    var Data = "";
    var latestSubmissionDate;

    var gridContext = primaryContext.getControl("Application_Logs");
    //Collecting Subgrid Rows.
    var myRows = gridContext.getGrid().getRows();
    //Obtaining Total Row Count.
    var RowCount = myRows.getLength();
    //Iterating Through Subgrid Rows.
    for (var i = 0; i < RowCount; i++) {
        //Obtaining A Single Row Data.
        var gridRowData = myRows.get(i).getData();
        //Obtaining Row Entity Object.
        var entity = gridRowData.getEntity();
        //get attribute value
        var notifyProvider = entity._attributes.getByName("ccof_notify_provider").getValue();
        var alreadySent = entity._attributes.getByName("ccof_sent").getValue();
        var changedOn = entity._attributes.getByName("ccof_changedon").getValue();
        var changedOnFormatted = entity._attributes.getByName("ccof_changedon").getValue().toLocaleString();
        latestSubmissionDate = primaryContext.getAttribute("ccof_latestsubmissiondate").getValue();
        var oldValue = entity._attributes.getByName("ccof_oldvalue").getValue();
        var newValue = entity._attributes.getByName("ccof_newvalue").getValue();
        var changedField = entity._attributes.getByName("ccof_object").getValue();
        //Collecing Row EntityRefrence.
        var entityReference = entity.getEntityReference();
    
        if (notifyProvider === true && alreadySent === false) {
            //Adding Up Row Data In A Variable.           
            Data += "Changed Date: " + changedOnFormatted + " | Field: " + changedField + " | Original Entry: " + oldValue + " | Updated Entry: " + newValue + "\n\n";
        }
    }
    var confirmString = { text: "Please confirm if you want to proceed and send reviewed application changes to the provider. \n \n" + Data, title: "Confirmation", confirmButtonLabel: "Confirm", cancelButtonLabel: "Cancel" };
    var confirmOptions = { height: 500, width: 700 };
    var stringOk = { text: "There are no changes to be sent.",title: "Information" };
   
    if (Data != ""){
    Xrm.Navigation.openConfirmDialog(confirmString, confirmOptions).then(
        function (success) {
            if (success.confirmed)
                callFlow(flowUrl);
            else
                console.log("Not OK");
        });
    }
    else{
        Xrm.Navigation.openConfirmDialog(stringOk, null).then(
            function (success) {
                if (success.confirmed)
                    console.log("OK");
                else
                    console.log("Not OK");
            });
    }
}

function callFlow(flowUrl) {
    debugger;
    let body = {
        "Entity": entityName,
        "RecordId": recordId
    };

    let req = new XMLHttpRequest();
    req.open("POST", flowUrl, true);
    req.setRequestHeader("Content-Type", "application/json");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                let resultJson = JSON.parse(this.response);
            } else {
                console.log(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(body));
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
function getCleanedGuid(id) {
    return id.replace("{", "").replace("}", "");
}
