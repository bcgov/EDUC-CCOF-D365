function hideCalculateCost(primaryControl) {
	debugger;
	var formContext = primaryControl;
	var CCFRIFacilityStatus = formContext.getAttribute("statuscode").getValue();
	var flagLockUnlockValue = formContext.getAttribute("ccof_flaglockunlockinitialand24monthsadjr").getValue();
	if ((CCFRIFacilityStatus == 6 || CCFRIFacilityStatus == 7 || CCFRIFacilityStatus == 8) && (flagLockUnlockValue == 0)) {
		formContext.getControl("ccof_meficap").setDisabled(true);
		formContext.getControl("ccof_limitfeestonmfbenchmark").setDisabled(true);
		formContext.getControl("ccof_capsindicator").setDisabled(true);
		return false; //Hide Calculate Costs Button
	}
	else {
		formContext.getControl("ccof_meficap").setDisabled(false);
		formContext.getControl("ccof_limitfeestonmfbenchmark").setDisabled(false);
		formContext.getControl("ccof_capsindicator").setDisabled(false);
		return true; // Show Calculate Costs Button
	}
}

function hideActivateS3Calculator(primaryControl) {
	debugger;
	var formContext = primaryControl;
	var CCFRIFacilityStatus = formContext.getAttribute("statuscode").getValue();
	var flagLockUnlockValue = formContext.getAttribute("ccof_flaglockunlockinitialand24monthsadjr").getValue();
	if ((CCFRIFacilityStatus == 6 || CCFRIFacilityStatus == 7 || CCFRIFacilityStatus == 8) && (flagLockUnlockValue == 0)) {
		return true; //show button Activate S3 Calculator
	}
	else {
		return false; //hide button Activate S3 Calculator
	}
}
// A webresource for custom 'Activate S3 Calculator' ribbon button
// Call CCFRI Facility - Actvate S3 Calculator
//var flowUrl = "https://prod-01.canadacentral.logic.azure.com:443/workflows/828029680b9f4a648413271fa9a8c564/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=XU2gfEp6Whz84MPJJP5XhVfA15OLwmia4zsWssSL-Ec"
//var flowUrl;
//flowUrl = "https://prod-12.canadacentral.logic.azure.com:443/workflows/314f736b5c0345bebfbe330ae93ff842/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=hoAj1Q_0ivnDF_sTiybcBCojLiKgGHXW583NVKyDUCg"

var confirmStrings = {
	text: "Please confirm if you want to Activate S3 Calculator.",
	title: "Confirmation",
	confirmButtonLabel: "Confirm",
	cancelButtonLabel: "Cancel"
};
var entityName;
var recordId;

function onClickOfActivateS3CalculatorBtn(primaryControl) {
	debugger;
	var formContext = primaryControl;
	entityName = formContext.data.entity.getEntityName();
	recordId = getCleanedGuid(formContext.data.entity.getId());
	Xrm.Navigation.openConfirmDialog(confirmStrings, null).then(

		function (success) {
			if (success.confirmed) callFlow();
			else console.log("Not OK");
		});
}

function callFlow() {
	debugger;
	let body = {
		"Entity": entityName,
		"id": recordId
	};
	var flowUrl;
	var result = getSyncMultipleRecord("environmentvariabledefinitions?$select=defaultvalue&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)&$filter=(schemaname eq 'ccof_ChamgeActionMTFIActivateS3Calculatorurl') and (environmentvariabledefinition_environmentvariablevalue/any(o1:(o1/environmentvariablevalueid ne null)))&$top=50");
	flowUrl = result[0]["environmentvariabledefinition_environmentvariablevalue"][0].value;

	let req = new XMLHttpRequest();
	req.open("POST", flowUrl, true);
	req.setRequestHeader("Content-Type", "application/json");
	req.onreadystatechange = function () {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 200) {
				let resultJson = JSON.parse(this.response);
			}
			else {
				console.log(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(body));
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