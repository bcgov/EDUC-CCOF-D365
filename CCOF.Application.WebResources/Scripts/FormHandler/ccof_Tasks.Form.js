var CCOF = CCOF || {};
CCOF.Tasks = CCOF.Tasks || {};
CCOF.Tasks.Form = CCOF.Tasks.Form || {};
CCOF.Tasks.Form = {
	onLoad: function (executionContext) {
		debugger;
		let formContext = executionContext.getFormContext();
		HidePriorityOptions(executionContext);
	},
}

function HidePriorityOptions(executionContext) {
	var formContext = executionContext.getFormContext();
	var priorityfield = formContext.getControl("header_priority")

	if (priorityfield !== null) {
		formContext.getControl("header_priority").removeOption(0); //Low Priority
	}

}