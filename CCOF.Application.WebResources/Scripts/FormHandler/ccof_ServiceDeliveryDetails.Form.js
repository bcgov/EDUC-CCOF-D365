var CCOF = CCOF || {};
CCOF.ServiceDeliveryDetails = CCOF.ServiceDeliveryDetails || {};
CCOF.ServiceDeliveryDetails.Form = CCOF.ServiceDeliveryDetails.Form || {};

//Formload logic starts here
CCOF.ServiceDeliveryDetails.Form = {
    onLoad: function (executionContext) {
    },

    setCareType: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var startTime = formContext.getAttribute("ccof_hours_of_operation_start").getValue();
        var endTime = formContext.getAttribute("ccof_hours_of_operation_end").getValue();

        if (startTime && endTime) {
            // Convert both times to milliseconds
            var diffInMs = endTime.getTime() - startTime.getTime();

            var diffInHours = diffInMs / (1000 * 60 * 60);

            if (diffInHours < 4) {
                formContext.getAttribute("ccof_care_type").setValue(2); // Part-time
            } else { 
                formContext.getAttribute("ccof_care_type").setValue(1); // Full-time
            }
        }
    }
}