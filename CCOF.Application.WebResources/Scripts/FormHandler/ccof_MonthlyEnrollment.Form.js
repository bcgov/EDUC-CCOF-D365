
var CCOF = CCOF || {};
CCOF.MonthlyEnrollment = CCOF.MonthlyEnrollment || {};
CCOF.MonthlyEnrollment.Form = CCOF.MonthlyEnrollment.Form || {};
CCOF.MonthlyEnrollment.Form = {
	onLoad: function (executionContext) {
		debugger;
		let formContext = executionContext.getFormContext();
		var reportType = formContext.getAttribute("ccof_reporttype").getValue();



		// if (formContext.getAttribute("ccof_locked").getValue()) {
		// 	formContext.getControl("ccof_locked").setDisabled(false);
		// } else {
		// 	formContext.getControl("ccof_locked").setDisabled(true);
		// }

		formContext.getAttribute("ccof_locked").addOnChange(onChange_locked);
		if (reportType === 100000000) // Baseline
		{
			formContext.ui.tabs.get("allAdjustmentER").setVisible(true);
			formContext.getControl("ccof_originalenrollmentreport").setVisible(false);
			formContext.getControl("ccof_prevenrollmentreport").setVisible(false);
		}
		else // Adjustment
		{
			formContext.ui.tabs.get("allAdjustmentER").setVisible(false);
			formContext.getControl("ccof_originalenrollmentreport").setVisible(true);
			formContext.getControl("ccof_prevenrollmentreport").setVisible(true);
		}
	},
	onSave: function (executionContext) {
		debugger;
		let formContext = executionContext.getFormContext();
		if ((formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 1 || formContext.getAttribute("ccof_ccfri_internal_status").getValue() === 2)
			&& (formContext.getAttribute("ccof_ccof_internal_status").getValue() === 1 || formContext.getAttribute("ccof_ccof_internal_status").getValue() === 2)) {
			let today = new Date();
			today.setHours(0, 0, 0, 0);
			let submissionDeadline = formContext.getAttribute("ccof_submissiondeadline").getValue();
			submissionDeadline.setHours(0, 0, 0, 0);
			if (submissionDeadline < today) {
				formContext.ui.setFormNotification("The date must be greater than today.", "ERROR", "date_check");
				if (executionContext.getEventArgs()) {
					executionContext.getEventArgs().preventDefault();
				}
			}
			else {
				formContext.ui.clearFormNotification("date_check");
			}
		}
	}
}
function onChange_locked(executionContext) {
	debugger;
	let formContext = executionContext.getFormContext();

	if (formContext.getAttribute("ccof_locked").getValue()) {

	if (!formContext.getAttribute("ccof_locked").getValue()) {
		// formContext.getControl("ccof_locked").setDisabled(true);

		formContext.getAttribute("ccof_submissiondeadline").setValue(null);
		formContext.getAttribute("ccof_submissiondeadline").setRequiredLevel("required")
		formContext.getAttribute("ccof_lockedunlockedreason").setRequiredLevel("required")
		formContext.getAttribute("ccof_ccfri_internal_status").setValue(1); // Created
		formContext.getAttribute("ccof_ccof_internal_status").setValue(1); // Created

		let alertStrings = { confirmButtonLabel: "OK", text: "Please input unlock reason and reset submission deadline and save it!" };
		let alertOptions = { height: 120, width: 260 };
		Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
			function () {
				console.log("Alert closed");
			},
			function (error) {
				console.log("Error showing alert: ", error.message);
			}
		);
	}
}
