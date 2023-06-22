// A webresource for custom ribbon button "Send Changes"  in Audit Log 


 // Call Application Audit Log flow
 var flowUrl;
 var result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_CCOFApplicationAuditLogSendEmailWithChangesUrl') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
 flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;
 var confirmString = { text:"Please confirm if you want to proceed and send reviwed application changes to the provider.", title:"Confirmation", confirmButtonLabel:"Confirm", cancelButtonLabel: "Cancel" };
 var entityName;
 var recordId;
 
 function onClickSendChangesBtn(primaryContext) {    
     debugger;      
     entityName =  primaryContext.data.entity.getEntityName();
     recordId = getCleanedGuid(primaryContext.data.entity.getId());
    
     Xrm.Navigation.openConfirmDialog(confirmString, null).then(
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
 
 