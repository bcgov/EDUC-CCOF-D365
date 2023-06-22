// A webresource for custom 'Extract Changes' ribbon button  in Audit Log 
 // Call Application Audit Log flow
//var flowUrl = "https://prod-12.canadacentral.logic.azure.com:443/workflows/87167d99064b4db08200cd8d4fbdbb39/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=rvszvsbJQhcvYMAHR9JIBSoSTd_U0UISONQConDehHU"
var flowUrl;
var result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_CCOFApplicationAuditLogExtractChangesUrl') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;
var confirmStrings = { text:"Please confirm if you want to fetch application changes to review.", title:"Confirmation", confirmButtonLabel:"Confirm", cancelButtonLabel: "Cancel" };
var entityName;
var recordId;
var changedBy;

function onClickOfExtractChangesBtn(primaryControl) {    
    debugger; 
    var formContext = primaryControl;     
    entityName =  formContext.data.entity.getEntityName();
    recordId = getCleanedGuid(formContext.data.entity.getId());
    changedBy = getCleanedGuid(formContext.getAttribute('modifiedby').getValue()[0].id);

    Xrm.Navigation.openConfirmDialog(confirmStrings, null).then(
        function (success) {    
            if (success.confirmed)
               callFlow(flowUrl);            
            else
                console.log("Not OK");
        });       
}

function callFlow(flowUrl) {
    debugger;
    let body = {    
     "Entity": entityName,
     "RecordId": recordId,
     "ChangedBy": changedBy
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

