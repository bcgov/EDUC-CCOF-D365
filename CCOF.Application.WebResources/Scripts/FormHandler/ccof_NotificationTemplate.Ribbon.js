
var CCOF = CCOF || {};
CCOF.NotificationTemplate = CCOF.NotificationTemplate || {};
CCOF.NotificationTemplate.Ribbon = CCOF.NotificationTemplate.Ribbon || {};

CCOF.NotificationTemplate.Ribbon = {

    CloneRecord: function (primaryControl) {
        debugger;
        var formContext = primaryControl;
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
   
		var displayText = "Are you sure you want to clone this record? Please click Yes button to continue, or click No button to cancel.";
        
        var confirmStrings = {
			title: "Confirmation - Clone Record",
			text: displayText,
			confirmButtonLabel: "Yes",
			cancelButtonLabel: "No"
		};

		var confirmOptions = { height: 240, width: 520 };
		Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
			function (success) {
				if (success.confirmed) {
                    formContext.ui.setFormNotification("Cloning Notification Template Record ...", "INFO", "CloneRecord");
                   
                    // fields to clone
                 // var name = formContext.getAttribute("ccof_name").getValue();
                    var templateName = formContext.getAttribute("ccof_template_name").getValue();                    
                    var category = formContext.getAttribute("ccof_category").getValue();  
                    var subjectLine = formContext.getAttribute("ccof_subject_line").getValue();
                    var notificationContent = formContext.getAttribute("ccof_notification_content").getValue();  
                    var queryType = formContext.getAttribute("ccof_query_type").getValue();
                    var criteria = formContext.getAttribute("ccof_criteria").getValue();  
                    var comments = formContext.getAttribute("ccof_comments").getValue();
                    var version = formContext.getAttribute("ccof_version").getValue(); 
                    var statusReason = formContext.getAttribute("statuscode").getValue(); 
          
                    if (formContext.getAttribute("ccof_program_year").getValue() != null) {
                        var programYearId = formContext.getAttribute("ccof_program_year").getValue()[0].id.replace(/[{}]/g, "");;                         
                        var programYearLookup = "/ccof_program_years(" + `${programYearId}` + ")";                 
                    } else {
                        var programYearLookup = "";                       
                    }

                    if (comments == null) comments = ""; 
                    var newStatusReason = 1;   // "Created" 
                    var newVersion = 1;
                    
                    var currentDateTime = formatCustomDate(new Date());	                                       
                    var data = {
                        "ccof_template_name": `${templateName}` + " - cloned on " + `${currentDateTime}`,
                        "ccof_category": `${category}`,           
                        "ccof_subject_line": `${subjectLine}`,
                        "ccof_notification_content": `${notificationContent}`,              
                        "ccof_query_type": `${queryType}`,
                        "ccof_criteria": `${criteria}`,           
                        "ccof_comments": `${comments}`,            
                        "ccof_version": `${newVersion}`,
                        "statuscode": `${newStatusReason}`,                      
                        "ccof_program_year@odata.bind": `${programYearLookup}`
                        }    
                
                    Xrm.WebApi.createRecord("ccof_notification_template", data).then(
                        function success(result) {                                           
                            console.log("New Notification Template record was created with ID: " + result.id);
                            
                            // perform operations on record creation
                            formContext.ui.setFormNotification("Cloning Record is complete", "INFO", "CloneRecord");
                            formContext.ui.clearFormNotification("CloneRecord"); 
                            
                            var displayText = "Cloning Notification Template Record is complete";
                            var alertStrings = { confirmButtonLabel: "Ok", text: displayText, title: "Response - Clone Record" };
                            var alertOptions = { height: 240, width: 520 };
                            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);                                                                                                 
                        },
                        function (error) {
                            console.log(error.message);
                            // handle error conditions
                            formContext.ui.clearFormNotification("CloneRecord");
                            var displayText = "There is some error! Please contact administrator!\n" + error.message;
                            var alertStrings = { confirmButtonLabel: "Ok", text: displayText, title: "Error!" };
                            var alertOptions = { height: 240, width: 520 };
                            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);                                                       
                        }
                    );                
				}
				else {               
					console.log("User cancelled the action.");
				}
			},
			function (error) {
				Xrm.Navigation.openErrorDialog({ message: error });
			}
		);    
        
    },

    SendNotifications: function (primaryControl) {
        var formContext = primaryControl;
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
        
        var confirmStrings = {
            title: "Confirmation - Sending Notifications",
            text: "Are you sure you want to send notification(s)? Please click Yes button to continue, or click No button to cancel.",
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        };
        var confirmOptions = { height: 240, width: 520 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    formContext.ui.setFormNotification("Sending Notifications ...", "INFO", "SendNotifications");
                 // let flowUrl = "https://1a49df49f24be835ab86dc8d9c0010.f5.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/0a54846f3ade46f99a7addac0982de2a/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=6T8AKbJagjm5e6j5-fijm7nFn3Qfrr_rrWS-E-3Lv58";
                    let result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_SendNotifications') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
                    let flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;
                    let body = {
                        "notificationTemplateId": recordId
                    };
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
                                formContext.ui.setFormNotification("Sending Notifications is complete", "INFO", "SendNotifications");
                                formContext.ui.clearFormNotification("SendNotifications");
                                Xrm.Navigation.openAlertDialog(result);
                            }
                            else if (this.status === 400) {
                                Xrm.Utility.closeProgressIndicator();
                                formContext.ui.clearFormNotification("SendNotifications");
                                var result = "There is some error! Please contact administrator!\n" + this.response;
                                var alertStrings = { confirmButtonLabel: "Ok", text: result, title: "Error!" };
                                var alertOptions = { height: 240, width: 520 };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                            }
                        }
                    };
                    req.send(input);
                }
                else {
                    console.log("Sending notifications does NOT proceed");
                }
            },
            function (error) {
                Xrm.Navigation.openErrorDialog({ message: error });
            }
        );        
		
    },   

    showHideSendNotifications: function (primaryControl) {
        debugger;
        var formContext = primaryControl;
        var statusReason = formContext.getAttribute("statuscode").getValue();
        var category = formContext.getAttribute("ccof_category").getValue();	
        var deliveryOption = formContext.getAttribute("ccof_delivery_option").getValue();	

        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "CCOF - Super Awesome Mods Squad" || item.name === "CCOF - Mod QC" ||item.name === "System Administrator") {
                visible = true;
            }
        });	
        
		// "Approved" (statusReason) && "Manual Sending" (deliveryOption)      
        var showButton = (statusReason === 101510002 && deliveryOption === 101510000 );   
        return showButton && visible;   
    },
    
    showHideCloneRecord: function (primaryControl) {
        debugger;
        var formContext = primaryControl;
        var statusReason = formContext.getAttribute("statuscode").getValue();
        var category = formContext.getAttribute("ccof_category").getValue();		

        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "CCOF - Super Awesome Mods Squad" || item.name === "CCOF - Mod QC" ||item.name === "System Administrator") {
                visible = true;
            }
        });	
		
        var showButton = (statusReason === 101510002);   // "Approved" (statusReason)
        return showButton && visible; 
    }
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

function formatCustomDate(date) {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Months are 0-based
    const day = date.getDate().toString().padStart(2, '0');
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    const seconds = date.getSeconds().toString().padStart(2, '0');

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
}
