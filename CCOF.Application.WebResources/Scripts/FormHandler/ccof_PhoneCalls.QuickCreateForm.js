var CCOF = CCOF || {};
CCOF.PhoneCalls = CCOF.PhoneCalls || {};
CCOF.PhoneCalls.QuickCreateForm = CCOF.PhoneCalls.QuickCreateForm || {};
CCOF.PhoneCalls.QuickCreateForm = {
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