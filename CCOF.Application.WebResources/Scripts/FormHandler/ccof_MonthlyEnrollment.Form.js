
var CCOF = CCOF || {};
CCOF.MonthlyEnrollment = CCOF.MonthlyEnrollment || {};
CCOF.MonthlyEnrollment.Form = CCOF.MonthlyEnrollment.Form || {};
//Formload logic starts here
CCOF.MonthlyEnrollment.Form = {
	onLoad: function (executionContext) {
		debugger;
		var formContext = executionContext.getFormContext();
		var reportType = formContext.getAttribute("ccof_reporttype").getValue();
		// alert("er_general_section_prev_approved,er_general_section_prev_base,er_general_section_prev_ccfri,er_general_section_prev_ccfri_provider");
		var tab = formContext.ui.tabs.get("er_general");
		if (reportType === 100000000) // Orignal
		{
			formContext.ui.tabs.get("adjustment_er").setVisible(true);
			tab.sections.get("er_general_difference").setVisible(false);
			tab.sections.get("er_general_difference_base_amount").setVisible(false);
			tab.sections.get("er_general_difference_ccfri_amount").setVisible(false);
			tab.sections.get("er_general_difference_ccfri_provider_amount").setVisible(false);
			tab.sections.get("er_general_section_prev_approved").setVisible(false);
			tab.sections.get("er_general_section_prev_base").setVisible(false);
			tab.sections.get("er_general_section_prev_ccfri").setVisible(false);
			tab.sections.get("er_general_section_prev_ccfri_provider").setVisible(false);
			tab.sections.get("er_general_difference_grandtotal").setVisible(false);
			formContext.getControl("ccof_monthlyenrollmentreport").setVisible(false);
			formContext.getControl("ccof_prevenrollmentreport").setVisible(false);
		}
		else // Adjustment
		{
			formContext.ui.tabs.get("adjustment_er").setVisible(false);
			tab.sections.get("er_general_difference").setVisible(true);
			tab.sections.get("er_general_difference_base_amount").setVisible(true);
			tab.sections.get("er_general_difference_ccfri_amount").setVisible(true);
			tab.sections.get("er_general_difference_ccfri_provider_amount").setVisible(true);
			tab.sections.get("er_general_section_prev_approved").setVisible(true);
			tab.sections.get("er_general_section_prev_base").setVisible(true);
			tab.sections.get("er_general_section_prev_ccfri").setVisible(true);
			tab.sections.get("er_general_section_prev_ccfri_provider").setVisible(true);
			tab.sections.get("er_general_difference_grandtotal").setVisible(true);
			formContext.getControl("ccof_monthlyenrollmentreport").setVisible(true);
			formContext.getControl("ccof_prevenrollmentreport").setVisible(true);
		}
	}
}