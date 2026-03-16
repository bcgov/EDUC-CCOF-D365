var CCOF = CCOF || {};
CCOF.Emails = CCOF.Emails || {};
CCOF.Emails.Form = CCOF.Emails.Form || {};
CCOF.Emails.Form = {
	onLoad: function (executionContext) {
		debugger;
		let formContext = executionContext.getFormContext();
		HidePriorityOptions(executionContext);
	},
}

function HidePriorityOptions(executionContext) {
	var formContext = executionContext.getFormContext();
	var priorityfield = formContext.getControl("header_prioritycode")

	if (priorityfield !== null) {
		formContext.getControl("header_prioritycode").removeOption(0); //Low Priority
	}

}