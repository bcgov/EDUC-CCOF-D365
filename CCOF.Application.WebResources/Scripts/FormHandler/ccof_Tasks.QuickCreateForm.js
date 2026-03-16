var CCOF = CCOF || {};
CCOF.Tasks = CCOF.Tasks || {};
CCOF.Tasks.QuickCreateForm = CCOF.Tasks.QuickCreateForm || {};
CCOF.Tasks.QuickCreateForm = {
	onLoad: function (executionContext) {
		debugger;
		let formContext = executionContext.getFormContext();
		HidePriorityOptions(executionContext);
	},
}

function HidePriorityOptions(executionContext) {
	var formContext = executionContext.getFormContext();
	var priorityfield = formContext.getControl("prioritycode")

	if (priorityfield !== null) {
		formContext.getControl("prioritycode").removeOption(0); //Low Priority
	}

}