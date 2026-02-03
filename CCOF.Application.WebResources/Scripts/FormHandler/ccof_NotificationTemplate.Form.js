
var CCOF = CCOF || {};
CCOF.NotificationTemplate = CCOF.NotificationTemplate || {};
CCOF.NotificationTemplate.Form = CCOF.NotificationTemplate.Form || {};

CCOF.NotificationTemplate.Form = {
    onLoad: function (executionContext) {
		debugger;
        var formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;

            case 1: //Create/QuickCreate
                break;

            case 2: //update	
			    var queryType = formContext.getAttribute("ccof_query_type").getValue();	                 // 101510000 = "Pre-defined Query", 101510001 = "Custom Query"
			    var deliveryOption = formContext.getAttribute("ccof_delivery_option").getValue();	     // 101510000 = "Manual Sending", 101510001 = "Scheduled Sending"		
			    var category = formContext.getAttribute("ccof_category").getValue();	                 // 101510000 = "Universal Funding Agreement Update - FA Template", 101510001 = "New Funding Agreement", 101510002 = "MOD Funding Agreement"		
                // QueryType	
                if (queryType == null) formContext.getAttribute("ccof_query_type").setValue(101510000);  // default: Pre-defined Query			
				if (queryType == 101510001) {        // 101510001 = "Custom Query"                                            
					formContext.getControl("ccof_criteria").setVisible(true);                            // show
					formContext.getAttribute("ccof_criteria").setRequiredLevel("required");              // required
				}
				else {				
					formContext.getControl("ccof_criteria").setVisible(false);                           // hide
					formContext.getAttribute("ccof_criteria").setRequiredLevel("none");                  // optional
				}                             
                // DeliveryOption
				if (deliveryOption == 101510001) {    // 101510001 = "Scheduled Sending"                                            
					formContext.getControl("ccof_scheduled_send_date").setVisible(true);                 // show
					formContext.getAttribute("ccof_scheduled_send_date").setRequiredLevel("required");   // required
				}
				else {				
					formContext.getControl("ccof_scheduled_send_date").setVisible(false);                // hide
                    formContext.getAttribute("ccof_scheduled_send_date").setValue(null);                 // set empty
					formContext.getAttribute("ccof_scheduled_send_date").setRequiredLevel("none");       // optional
				}                 
                // Category
                if (category === 101510000) {         // 101510000 = "Universal Funding Agreement Update - FA Template"
                      formContext.getAttribute("ccof_query_type").setValue(101510000);  // 101510000 = "Pre-defined Query"
                      formContext.getControl("ccof_query_type").setDisabled(true);
                }
				else {				
                      formContext.getAttribute("ccof_query_type").setValue(101510001);  // 101510001 = "Custom Query"                              
                      formContext.getControl("ccof_query_type").setDisabled(true);
				} 
                // onChange              
                formContext.getAttribute("ccof_query_type").addOnChange(onChange_QueryType);
                formContext.getAttribute("ccof_delivery_option").addOnChange(onChange_DeliveryOption);
                formContext.getAttribute("ccof_category").addOnChange(onChange_Category);
                
                formContext.data.refresh(true);
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

function onChange_QueryType(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var queryType = formContext.getAttribute("ccof_query_type").getValue();	                 // 101510000 = "Pre-defined Query", 101510001 = "Custom Query"
    if (queryType == null) formContext.getAttribute("ccof_query_type").setValue(101510000);  // default: Pre-defined Query			
    if (queryType == 101510001) {                                                
        formContext.getControl("ccof_criteria").setVisible(true);                            // show
        formContext.getAttribute("ccof_criteria").setRequiredLevel("required");              // required
    }
    else {				
        formContext.getControl("ccof_criteria").setVisible(false);                           // hide
        formContext.getAttribute("ccof_criteria").setRequiredLevel("none");                  // optional
    }
}

function onChange_DeliveryOption(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var deliveryOption = formContext.getAttribute("ccof_delivery_option").getValue();	     // 101510000 = "Manual Sending", 101510001 = "Scheduled Sending"		
    if (deliveryOption == 101510001) {                                                
        formContext.getControl("ccof_scheduled_send_date").setVisible(true);                 // show
        formContext.getAttribute("ccof_scheduled_send_date").setRequiredLevel("required");   // required
    }
    else {				
        formContext.getControl("ccof_scheduled_send_date").setVisible(false);                // hide
        formContext.getAttribute("ccof_scheduled_send_date").setValue(null);                 // set empty
        formContext.getAttribute("ccof_scheduled_send_date").setRequiredLevel("none");       // optional
    }        
}

function onChange_Category(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var category = formContext.getAttribute("ccof_category").getValue();	                 // 101510000 = "Universal Funding Agreement Update - FA Template", 101510001 = "New Funding Agreement", 101510002 = "MOD Funding Agreement"		
    if (category === 101510000) {         
          formContext.getAttribute("ccof_query_type").setValue(101510000);  // "Pre-defined Query"
          formContext.getControl("ccof_query_type").setDisabled(true);
          formContext.getControl("ccof_criteria").setVisible(false);                               
    }
    else {				
          formContext.getAttribute("ccof_query_type").setValue(101510001);  // "Custom Query"                              
          formContext.getControl("ccof_query_type").setDisabled(true);
          formContext.getControl("ccof_criteria").setVisible(true);               
    }    
}