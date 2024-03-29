// JavaScript source code
var CCOF = CCOF || {};
CCOF.ChangeActionMTFI = CCOF.ChangeActionMTFI || {};
CCOF.ChangeActionMTFI.Form = CCOF.ChangeActionMTFI.Form || {};
CCOF.ChangeActionMTFI.Form = {
	onLoad: function (executionContext) {
		debugger;
		let formContext = executionContext.getFormContext();

		HideAdjudicatorRecommendationOptions(executionContext);

		formContext.getAttribute("ccof_afcconfirmedbyqc").addOnChange(onChange_confirmedbyqc);
		formContext.getAttribute("ccof_afcconfirmedbycommittee").addOnChange(onChange_confirmedbyconfirmedbycommittee);
		formContext.getAttribute("ccof_afcsenttoexecutive").addOnChange(onChange_senttoexecutive);
		formContext.getAttribute("ccof_afcreviewedbyexecutive").addOnChange(onChange_reviewedbyexecutive);
		formContext.getAttribute("ccof_newmodifiedfacilityrecommendation").addOnChange(onChange_newmodifiedfacilityrecommendation);
		formContext.getAttribute("ccof_newmodifiedfacilityqcdecision").addOnChange(onChange_newmodifiedfacilityqcdecision);
		formContext.getAttribute("ccof_closureadjudicatorrecommendationnotes").addOnChange(onChange_closureadjudicatorrecommendationnotes);
		formContext.getAttribute("ccof_closureadjudicatorrecommendation").addOnChange(onChange_closureadjudicatorrecommendation);
		formContext.getAttribute("ccof_ccfripaymenteligibilitystartdate").addOnChange(onChange_ccfripaymenteligibilitystartdate);
		formContext.getAttribute("ccof_ccfriadjudicatorrecommendation").addOnChange(onChange_ccfriadjudicatorrecommendation);
		formContext.getAttribute("ccof_afcoccurred").addOnChange(onChange_afcoccurred);
		formContext.getAttribute("ccof_mtfi_qcdecision").addOnChange(onChange_mtfi_qcdecision);
		formContext.getAttribute("ccof_mtfipreapproval").addOnChange(onChange_mtfipreapproval);
		formContext.getAttribute("ccof_unapproved_mtfi_followup_required").addOnChange(onChange_unapproved_mtfi_followup_required);
		formContext.getAttribute("ccof_afcqcrecommendedtocommittee").addOnChange(onChange_afcqcrecommendedtocommittee);

	},
}
function refreshMedianGrid(executionContext) {
	debugger;
	var formContext = executionContext.getFormContext();
	var quickViewContext = formContext.ui.quickForms.get("QuickviewControlMedian24");
	//var median = quickViewContext.getAttribute("ccof_region3pctmedian").getValue();
	var grid = quickViewContext.getControl("Subgrid_Median");
	var fetchData = {
		"statecode": "0",
		"ccof_median_fee_sdaid": "b1b2e1ec-a254-ed11-9560-000d3af4fbcb"
	};
	var fetchXml = [
		"<fetch>",
		"  <entity name='ccof_median_fee_sda'>",
		"    <filter>",
		"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
		"      <condition attribute='ccof_median_fee_sdaid' operator='eq' value='", fetchData.ccof_median_fee_sdaid/*b1b2e1ec-a254-ed11-9560-000d3af4fbcb*/, "' uitype='ccof_median_fee_sda'/>",
		"    </filter>",
		"  </entity>",
		"</fetch>"
	].join("");
	if (!grid) {
		setTimeout(refreshMedianGrid, 2000);
		return;
	};
	grid.setFilterXml(fetchXml);
	grid.refresh();

}
function getCleanedGuid(id) {
	return id.replace("{", "").replace("}", "");
}

function getSyncSingleRecord(request) {
	var results = null;
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
				var result = JSON.parse(this.response);
				results = result;
			}
			else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send();
	return results;
}

function HideAdjudicatorRecommendationOptions(executionContext) {
	var today = new Date();
	var limitDate = new Date('2023/03/31'); //FY 2023/24
	var formContext = executionContext.getFormContext();
	//Remove Stage 2(NOM) and Stage 2 (MED) automatically for the FY 2023/24
	if (today > limitDate) {
		formContext.getControl("ccof_ccfriadjudicatorrecommendation").removeOption(100000003); //Stage 2 (NOM)
		formContext.getControl("ccof_ccfriadjudicatorrecommendation").removeOption(100000004); //Stage 2 (MED)
	}
}


function onChange_confirmedbyqc(executionContext) {
	setUserAndDate(executionContext, "ccof_afc_updatedbyconfirmedbyqc", "ccof_afc_updatedonconfirmedbyqc");
}

function onChange_confirmedbyconfirmedbycommittee(executionContext) {
	setUserAndDate(executionContext, "ccof_afc_updatebyconfirmedbycommittee", "ccof_afc_updateonconfirmedbycommittee");
}

function onChange_senttoexecutive(executionContext) {
	setUserAndDate(executionContext, "ccof_afc_updatedbysenttoexecutive", "ccof_afc_updatedonsenttoexecutive");
}

function onChange_reviewedbyexecutive(executionContext) {
	setUserAndDate(executionContext, "ccof_afc_updatedbyreviewedbyexecutive", "ccof_afc_updatedonreviewedbyexecutive");
}

function onChange_newmodifiedfacilityrecommendation(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedby_facilityrecommendation", "ccof_decision_updatedon_facilityrecommendation");
}
function onChange_newmodifiedfacilityqcdecision(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedby_facilityqcdecision", "ccof_decision_updatedon_facilityqcdecision");
}
function onChange_closureadjudicatorrecommendationnotes(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedby_closureadjrecnotes", "ccof_decision_updatedon_closureadjrecnotes");
}
function onChange_closureadjudicatorrecommendation(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedby_closureadjrec", "ccof_decision_updatedon_closureadjrec");
}
function onChange_ccfripaymenteligibilitystartdate(executionContext) {
	setUserAndDate(executionContext, "ccof_dec_updatedby_ccfripayeligibilitystart", "ccof_ccof_dec_updatedon_ccfripayeligibilitysta");
}
function onChange_ccfriadjudicatorrecommendation(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedby_ccfriadjudicatorrec", "ccof_decision_updatedonccfriadjudicatorrec");
}
function onChange_afcoccurred(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedby_afcoccurred", "ccof_decision_updatedonafcoccurred");
}
function onChange_mtfi_qcdecision(executionContext) {
	setUserAndDate(executionContext, "ccof_mtfi_updatedbyqcdecision", "ccof_mtfi_updatedonqcdecision");
}
function onChange_mtfipreapproval(executionContext) {
	setUserAndDate(executionContext, "ccof_updatedbymtfipreapproval", "ccof_updatedonmtfipreapproval");
}
function onChange_unapproved_mtfi_followup_required(executionContext) {
	setUserAndDate(executionContext, "ccof_decision_updatedbyunapprovedmtfifollowup", "ccof_decision_updatedonunapprovemtfifollowup");
}
function onChange_afcqcrecommendedtocommittee(executionContext) {
	setUserAndDate(executionContext, "ccof_updatedbyafcqcrecommendedtocommittee", "ccof_updatedonafcqcrecommendedtocommittee");
}

//Get the current user and Date Now
function setUserAndDate(executionContext, userField, dateField) {
	var formContext = executionContext.getFormContext();
	var userSettings = Xrm.Utility.getGlobalContext().userSettings;
	var date_time = new Date();
	var userLookup = new Array();
	userLookup[0] = new Object();
	userLookup[0].id = userSettings.userId;
	userLookup[0].entityType = "systemuser";
	formContext.getAttribute(userField).setValue(userLookup);
	formContext.getAttribute(dateField).setValue(date_time);
}
// JavaScript source code
function openMedianLookupModalDialog(executionContext) {
	debugger;
	var formContext = executionContext.getFormContext(); // Getting FormContext
	if (formContext.ui.quickForms.get("ApplicationCCFRIMediaFeeInfo1") != null) {
		var MedianQuickViewControl = formContext.ui.quickForms.get("ApplicationCCFRIMediaFeeInfo1");
		if (MedianQuickViewControl !== undefined) {
			if (MedianQuickViewControl.isLoaded()) {
				var ccof_region = MedianQuickViewControl.getControl("ccof_region");
				var ccof_region3pctmedian = MedianQuickViewControl.getControl("ccof_region3pctmedian");
				var ccof_regionnmfbenchmark = MedianQuickViewControl.getControl("ccof_regionnmfbenchmark");
				if (ccof_region != null) { ccof_region.addOnLookupTagClick(onLookupClick); }
				if (ccof_region3pctmedian != null) {
					ccof_region3pctmedian.addOnLookupTagClick(onLookupClick);
				}
				if (ccof_regionnmfbenchmark != null) {
					ccof_regionnmfbenchmark.addOnLookupTagClick(onLookupClick);
				}

			}
		}
	}

}


function onLookupClick(executionContext) {
	debugger;
	executionContext.getEventArgs().preventDefault();

	var record = executionContext.getEventArgs().getTagValue();

	Xrm.Navigation.navigateTo({

		pageType: "entityrecord",

		entityName: record.entityType,

		entityId: record.id

	}, {

		target: 2,   //2 - Open record in modal dialog

		width:

		{

			value: 80,

			unit: "%"

		}

	});

}
function OnSaveSetEligibilityStartDate(executionContext) {

	var formContext = executionContext.getFormContext();
	if (formContext.getAttribute("ccof_eligibilitystartdate").getValue() != null && formContext.getAttribute("ccof_eligibilitystartyear").getValue() != null) {
		var EligibilityStartMonth = formContext.getAttribute("ccof_eligibilitystartdate").getValue();
		var EligibilityStartYear = formContext.getAttribute("ccof_eligibilitystartyear").getValue();
		var Year = EligibilityStartYear;
		var Month = EligibilityStartMonth;
		var Day = 01;
		if (Year != null && Month != null) {
			var actualEligibilityStartDate = Month + "-" + Day + "-" + Year;
			var dateData = new Date(actualEligibilityStartDate);
			formContext.getAttribute("ccof_ccfripaymenteligibilitystartdate").setValue(dateData);
			formContext.getAttribute("ccof_ccfripaymenteligibilitystartdate").setSubmitMode("always");
		}
	}
}
