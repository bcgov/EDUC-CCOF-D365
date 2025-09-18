var CCOF = CCOF || {};
CCOF.FeeFloorExempt = CCOF.FeeFloorExempt || {};
CCOF.FeeFloorExempt.Form = CCOF.FeeFloorExempt.Form || {};
CCOF.FeeFloorExempt.Form = {
    onLoad: function (executionContext) {
        debugger;
    },
    onSave: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        let validation = validateFeeFloorExempt(executionContext);
        if (validation.length > 0) {
            Xrm.Navigation.openAlertDialog({ text: "You cannot save this record as some of Daily Parent Fees are greater then CCFRI Max.\n" + validation.join("\n") });
            executionContext.getEventArgs().preventDefault();
         }
    }
}

function validateFeeFloorExempt(executionContext) {
    debugger;
    const formContext = executionContext.getFormContext();
    const facility = formContext.getAttribute("ccof_facility").getValue();
    const programYear = formContext.getAttribute("ccof_programyear").getValue();
    const months = formContext.getAttribute("ccof_months").getValue();
    if (!facility || !programYear || !months) {
        console.log("Missing required values: facility, program year, or enrolment month");
        return;
    }
    var validationValue = [];
    const facilityId = getCleanedGuid(facility[0].id);
    const programYearId = getCleanedGuid(programYear[0].id);
    const monthLogicalNameString = `
            [
              { "id": 1, "enrolmentMonth": 1, "monthNameinApprovedParentFee": "ccof_jan" },
              { "id": 2, "enrolmentMonth": 2, "monthNameinApprovedParentFee": "ccof_feb" },
              { "id": 3, "enrolmentMonth": 3, "monthNameinApprovedParentFee": "ccof_mar" },
              { "id": 4, "enrolmentMonth": 4, "monthNameinApprovedParentFee": "ccof_apr" },
              { "id": 5, "enrolmentMonth": 5, "monthNameinApprovedParentFee": "ccof_may" },
              { "id": 6, "enrolmentMonth": 6, "monthNameinApprovedParentFee": "ccof_jun" },
              { "id": 7, "enrolmentMonth": 7, "monthNameinApprovedParentFee": "ccof_jul" },
              { "id": 8, "enrolmentMonth": 8, "monthNameinApprovedParentFee": "ccof_aug" },
              { "id": 9, "enrolmentMonth": 9, "monthNameinApprovedParentFee": "ccof_sep" },
              { "id": 10, "enrolmentMonth": 10, "monthNameinApprovedParentFee": "ccof_oct" },
              { "id": 11, "enrolmentMonth": 11, "monthNameinApprovedParentFee": "ccof_nov" },
              { "id": 12, "enrolmentMonth": 12, "monthNameinApprovedParentFee": "ccof_dec" }
            ]
            `;
    const monthLogicalNameArray = JSON.parse(monthLogicalNameString);
    const monthValueMapping = {
        1: 4,   // April
        2: 5,   // May
        3: 6,   // June
        4: 7,   // July
        5: 8,   // August
        6: 9,   // September
        7: 10,  // October
        8: 11,  // November
        9: 12,  // December
        10: 1,  // January
        11: 2,  // February
        12: 3   // March
    };

    const org = getSyncSingleRecord("accounts(" + facilityId + ")?$select=accountnumber&$expand=parentaccountid($select=accountnumber,name)");
    const monthlyBusinessDay = getSyncMultipleRecord("ccof_monthlybusinessdaies?$select=ccof_businessday,ccof_month,ccof_name&$filter=(_ccof_programyear_value eq " + programYearId + " and statecode eq 0)");
    if (monthlyBusinessDay.length===0) {
        validationValue.push("Key data Business day is missing");
        return validationValue;
    }
    const rate = getSyncMultipleRecord("ccof_rates?$filter=(statecode eq 0)");
    const ApprovedParentFeeString = "ccof_parent_feeses?$select=ccof_apr,ccof_aug,ccof_availability,_ccof_childcarecategory_value,ccof_dec,_ccof_facility_value,ccof_feb,ccof_frequency,ccof_jan,ccof_jul,ccof_jun,ccof_mar,ccof_may,ccof_name,ccof_nov,ccof_oct,_ccof_programyear_value,ccof_sep,ccof_type,statecode,statuscode&$expand=ccof_ChildcareCategory($select=ccof_childcarecategorynumber,ccof_name)&$filter=(statecode eq 0 and statuscode eq 1 and _ccof_programyear_value eq " + programYearId + " and _ccof_facility_value eq " + facilityId + ") and (ccof_ChildcareCategory/ccof_childcare_categoryid ne null)";
    const ApprovedParentFee = getSyncMultipleRecord(ApprovedParentFeeString);
    let providerType = 100000000; // Group
    if (org) {
        let accountNumber = org["parentaccountid"]["accountnumber"];
        if (accountNumber && accountNumber.startsWith("G")) {
            providerType = 100000000; // Group
        } else {
            providerType = 100000001; // Family
        }
    }
    for (const month of months) {
        const normalMonth = monthValueMapping[month];
        console.log(`OptionSet Month Value: ${month}, Converted to Normal Month: ${normalMonth}`);
        // get busniess days for specific month
        let businessDay = monthlyBusinessDay.find(n => n?.["ccof_month"] === normalMonth && n?.["ccof_businessday"] != null)["ccof_businessday"];
        // get rate for specific business day and Provider Type
        const ccfriMax = rate.find(n => n?.["ccof_providertype"] === providerType && n?.["ccof_ratetype"] === 100000004 && n?.["ccof_businessday"] === businessDay) ?? null;  //100000004 CCFRI Max;
        const ccfriMin = rate.find(n => n?.["ccof_providertype"] === providerType && n?.["ccof_ratetype"] === 100000005 && n?.["ccof_businessday"] === businessDay) ?? null;  //100000005 CCFRI Min;

        // Find mapping object for this month
        const monthLogicalNameObj = monthLogicalNameArray.find(m => m.enrolmentMonth === normalMonth);
        if (monthLogicalNameObj) {
            const monthLogicalName = monthLogicalNameObj.monthNameinApprovedParentFee;
            console.log(`Month ${normalMonth} maps to field: ${monthLogicalName}`);
            // get Approved Parent fee for Specfic month
            const approvedParentfee0to18 = ApprovedParentFee?.find(node => node["ccof_ChildcareCategory"]?.["ccof_childcarecategorynumber"] && parseInt(node["ccof_ChildcareCategory"]["ccof_childcarecategorynumber"], 10) === 1) ?? null; 
            const approvedParentfee18to36 = ApprovedParentFee?.find(node => node["ccof_ChildcareCategory"]?.["ccof_childcarecategorynumber"] && parseInt(node["ccof_ChildcareCategory"]["ccof_childcarecategorynumber"], 10) === 2) ?? null;
            const approvedParentfee3YK = ApprovedParentFee?.find(node => node["ccof_ChildcareCategory"]?.["ccof_childcarecategorynumber"] && parseInt(node["ccof_ChildcareCategory"]["ccof_childcarecategorynumber"], 10) === 3) ?? null;
            const approvedParentfeeOOSCK = ApprovedParentFee?.find(node => node["ccof_ChildcareCategory"]?.["ccof_childcarecategorynumber"] && parseInt(node["ccof_ChildcareCategory"]["ccof_childcarecategorynumber"], 10) === 4) ?? null;
            const approvedParentfeeOOSCG = ApprovedParentFee?.find(node => node["ccof_ChildcareCategory"]?.["ccof_childcarecategorynumber"] && parseInt(node["ccof_ChildcareCategory"]["ccof_childcarecategorynumber"], 10) === 5) ?? null;
            const approvedParentfeePre = ApprovedParentFee?.find(node => node["ccof_ChildcareCategory"]?.["ccof_childcarecategorynumber"] && parseInt(node["ccof_ChildcareCategory"]["ccof_childcarecategorynumber"], 10) === 6) ?? null;

            console.log(
                `OptionSet Month Value: ${month}, Converted to Normal Month: ${normalMonth}` +
                " approvedParentfee0to18: " + JSON.stringify(approvedParentfee0to18)
            );
            const approvedParentFee = {
                "ccof_approvedparentfee0to18": approvedParentfee0to18?.[monthLogicalName] ?? null,
                "ccof_approvedparentfee18to36": approvedParentfee18to36?.[monthLogicalName] ?? null,
                "ccof_approvedparentfee3yk": approvedParentfee3YK?.[monthLogicalName] ?? null,
                "ccof_approvedparentfeeoosck": approvedParentfeeOOSCK?.[monthLogicalName] ?? null,
                "ccof_approvedparentfeeooscg": approvedParentfeeOOSCG?.[monthLogicalName] ?? null,
                "ccof_approvedparentfeepre": approvedParentfeePre?.[monthLogicalName] ?? null,
                "ccof_approvedparentfeefrequency0to18": approvedParentfee0to18?.["ccof_frequency"] ?? null,
                "ccof_approvedparentfeefrequency18to36": approvedParentfee18to36?.["ccof_frequency"] ?? null,
                "ccof_approvedparentfeefrequency3yk": approvedParentfee3YK?.["ccof_frequency"] ?? null,
                "ccof_approvedparentfeefrequencyoosck": approvedParentfeeOOSCK?.["ccof_frequency"] ?? null,
                "ccof_approvedparentfeefrequencyooscg": approvedParentfeeOOSCG?.["ccof_frequency"] ?? null,
                "ccof_approvedparentfeefrequencypre": approvedParentfeePre?.["ccof_frequency"] ?? null
            };

            console.log(`Approved Parent Fee JSON created for month ${monthLogicalName}: ${JSON.stringify(approvedParentFee)}`);
            const dailyParentFee0to18 = CalculateDailyParentFee(approvedParentFee?.["ccof_approvedparentfee0to18"] ?? null, approvedParentFee?.["ccof_approvedparentfeefrequency0to18"] ?? null, businessDay);
            const dailyParentFee18to36 = CalculateDailyParentFee(approvedParentFee?.["ccof_approvedparentfee18to36"] ?? null, approvedParentFee?.["ccof_approvedparentfeefrequency18to36"] ?? null, businessDay);
            const dailyParentFee3yk = CalculateDailyParentFee(approvedParentFee?.["ccof_approvedparentfee3yk"] ?? null, approvedParentFee?.["ccof_approvedparentfeefrequency3yk"] ?? null, businessDay);
            const dailyParentFeeoosck = CalculateDailyParentFee(approvedParentFee?.["ccof_approvedparentfeeoosck"] ?? null, approvedParentFee?.["ccof_approvedparentfeefrequencyoosck"] ?? null, businessDay);
            const dailyParentFeeooscg = CalculateDailyParentFee(approvedParentFee?.["ccof_approvedparentfeeooscg"] ?? null, approvedParentFee?.["ccof_approvedparentfeefrequencyooscg"] ?? null, businessDay);
            let dailyParentFeepre = null;
            if (providerType === 100000000) { // Group
                dailyParentFeepre = CalculateDailyParentFee(approvedParentFee["ccof_approvedparentfeepre"] ?? null, approvedParentFee["ccof_approvedparentfeefrequencypre"] ?? null, businessDay);
            }
            // Compare with CCFRI Max
            if (ccfriMax && dailyParentFee0to18 != null && dailyParentFee0to18 != 0 && dailyParentFee0to18 > ccfriMax["ccof_over0to18"]) {
                let tempString = "Month: " + normalMonth + " ChildCare Category: 0-18.";
                validationValue.push(tempString);
            }
            if (ccfriMax && dailyParentFee18to36 != null && dailyParentFee18to36 != 0 && dailyParentFee18to36 > ccfriMax["ccof_over18to36"]) {
                let tempString = "Month: " + normalMonth + " ChildCare Category: 18-36.";
                validationValue.push(tempString);
            }
            if (ccfriMax && dailyParentFee3yk != null && dailyParentFee3yk != 0 && dailyParentFee3yk > ccfriMax["ccof_over3yk"]) {
                let tempString = "Month: " + normalMonth + " ChildCare Category: 3Y-K.";
                validationValue.push(tempString);
            }
            if (ccfriMax && dailyParentFeeoosck != null && dailyParentFeeoosck != 0 && dailyParentFeeoosck > ccfriMax["ccof_overoosck"]) {
                let tempString = "Month: " + normalMonth + " ChildCare Category: OOSC-K.";
                validationValue.push(tempString);
            }
            if (ccfriMax && dailyParentFeeooscg != null && dailyParentFeeooscg != 0 && dailyParentFeeooscg > ccfriMax["ccof_overooscg"]) {
                let tempString = "Month: " + normalMonth + " ChildCare Category: OOSC-G.";
                validationValue.push(tempString);
            } if (ccfriMax && dailyParentFeepre != null && dailyParentFeepre != 0 && dailyParentFeepre > ccfriMax["ccof_lesspre"]) {
                let tempString = "Month: " + normalMonth + " ChildCare Category: Pre";
                validationValue.push(tempString);
            }
        }
    }
    console.log("Validation complete");
    return validationValue;
}
function CalculateDailyParentFee(fee, frequency, businessDay) {
    if (fee == null) return null;

    if (frequency == null) return null;

    if (frequency === 100000002) { // Daily
        return fee;
    }

    if (businessDay >= 20) {
        return fee != null ? fee / 20 : null;
    } else {
        return fee != null ? fee / 19 : null;
    }
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