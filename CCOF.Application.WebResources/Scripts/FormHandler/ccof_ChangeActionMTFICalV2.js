
// Initial Tab and MTFI recalculation
var CCOF = CCOF || {};
CCOF.ChangeActionMTFI = CCOF.ChangeActionMTFI || {};
CCOF.ChangeActionMTFI.Calculation = CCOF.ChangeActionMTFI.Calculation || {};
CCOF.ChangeActionMTFI.Calculation = {
    Calculation: function (primaryControl) {
        try {
            debugger;
            var formContext = primaryControl;
            // INITIAL CALCULATION RECALCULATION
            var InitialTotalAllowableStagePolicy;
            var entityId = formContext.getAttribute("ccof_ccfri_facility").getValue()[0].id; // get parent record id
            var isMTFI = true;
            var ccfriFacilityInfo = getSyncSingleRecord("ccof_adjudication_ccfri_facilities(" + getCleanedGuid(entityId) + ")?$select=_ccof_applicationccfri_value,ccof_totalexpenses_exceptionalcircumstances,ccof_totalexpenses_priorityserviceexpansion,ccof_totalexpenses_wageincrease,ccof_limitfeestonmfbenchmark,ccof_meficap,ccof_ccfriqcdecision,ccof_newmodifiedfacilityqcdecision&$expand=ccof_ProgramYear($select=ccof_programyearnumber)");
            var ccfriFacilityQCDecision = ccfriFacilityInfo['ccof_ccfriqcdecision'];
            var ccfri_facility_allowable_amountEntityName = "ccof_ccfri_facility_allowable_amount";
            var ccfriFacilityAppCCFRI = ccfriFacilityInfo['_ccof_applicationccfri_value'];
            var newModifiedQCDecision = ccfriFacilityInfo['ccof_newmodifiedfacilityqcdecision'];
            var programYearNumber = ccfriFacilityInfo['ccof_ProgramYear']['ccof_programyearnumber'];
            var MTFISequenceNumber = formContext.getAttribute("ccof_mtfi_sequence_number").getValue();
            var MTFIUnusedExpenses = formContext.getAttribute("ccof_unused_carried_forward_amount").getValue();
            var currentMTFIentityId = formContext.data.entity.getId();
            var MTFIappCCFRI = formContext.getAttribute("ccof_ccfri").getValue()[0].id;
            var ccfri_facility_allowable_amount_mtfiEntityName = "ccof_ccfri_facility_allowable_amount_mtfi";
            var alertOptions = { height: 240, width: 320 };

            // Get Region, Median, NMF
            var regionInfo = getSyncSingleRecord("ccof_applicationccfris(" + getCleanedGuid(ccfriFacilityAppCCFRI) + ")?$select=ccof_applicationccfriid,_ccof_region_value&$expand=ccof_Application($select=ccof_applicationid,ccof_name,_ccof_programyear_value,ccof_providertype),ccof_Region3PctMedian($select=ccof_0to18months,ccof_10percentageof0to18,ccof_10percentageof18to36,ccof_10percentageof3ytok,ccof_10percentageofoosctog,ccof_10percentageofoosctok,ccof_10percenatgeofpre,ccof_18to36months,ccof_3percentageof0to18,ccof_3percentageof18to36,ccof_3percentageof3ytok,_ccof_3percentmedian_value,ccof_3percentageofoosctog,ccof_3percentageofoosctok,ccof_3percentageofpre,ccof_3yearstokindergarten,ccof_name,ccof_outofschoolcaregrade1,ccof_outofschoolcarekindergarten,ccof_preschool),ccof_RegionNMFBenchmark($select=ccof_fee_benchmark_sdaid,ccof_0to18m,ccof_18to36m,ccof_3ytok,ccof_name,ccof_oosctograde,ccof_oosctok,ccof_preschool)");
            var MediansFee = // 10 MEFI from Median table based on SDA,Org,OrgType and Program year.
            {
                "0-18": regionInfo['ccof_Region3PctMedian']['ccof_0to18months'],
                "18-36": regionInfo['ccof_Region3PctMedian']['ccof_18to36months'],
                "3Y-K": regionInfo['ccof_Region3PctMedian']['ccof_3yearstokindergarten'],
                "OOSC-K": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcarekindergarten'],
                "OOSC-G": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcaregrade1'],
                "PRE": regionInfo['ccof_Region3PctMedian']['ccof_preschool'],
                "0-18_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof0to18'],
                "18-36_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof18to36'],
                "3Y-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof3ytok'],
                "OOSC-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctok'],
                "OOSC-G_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctog'],
                "PRE_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofpre'],
                "0-18_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof0to18'],
                "18-36_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof18to36'],
                "3Y-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof3ytok'],
                "OOSC-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctok'],
                "OOSC-G_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctog'],
                "PRE_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percenatgeofpre']
            }
            // list Total Allowable Fee Increase from Initial 
            var FacilityAmountAllowedRecords = getSyncMultipleRecord("ccof_ccfri_facility_allowable_amounts?$select=ccof_3yearstokindergarten,ccof_outofschoolcarekindergarten,ccof_preschool,ccof_18to36months,ccof_0to18months,ccof_outofschoolcaregrade1,ccof_stage3policy,ccof_displayorder&$filter=(ccof_stage3policy eq 100000004 and _ccof_ccfrifacility_value eq " + entityId + ")&$top=50&$orderby=ccof_displayorder asc");
            var Initialstage2capammount = {
                "0-18": MediansFee['0-18_Per10'],
                "18-36": MediansFee['18-36_Per10'],
                "PRE": MediansFee['PRE_Per10'],
                "OOSC-K": MediansFee['OOSC-K_Per10'],
                "3Y-K": MediansFee['3Y-K_Per10'],
                "OOSC-G": MediansFee['OOSC-G_Per10']
            }
            InitialTotalAllowableStagePolicy = {
                "0-18": FacilityAmountAllowedRecords[0]['ccof_0to18months'],
                "18-36": FacilityAmountAllowedRecords[0]['ccof_18to36months'],
                "PRE": FacilityAmountAllowedRecords[0]['ccof_preschool'] ,
                "OOSC-K": FacilityAmountAllowedRecords[0]['ccof_outofschoolcarekindergarten'],
                "3Y-K": FacilityAmountAllowedRecords[0]['ccof_3yearstokindergarten'],
                "OOSC-G": FacilityAmountAllowedRecords[0]['ccof_outofschoolcaregrade1']
            }
            var FeeIncreaseDetails = getSyncMultipleRecord("ccof_ccfri_facility_parent_fees?$select=_ccof_adjudicationccfrifacility_value,ccof_averageenrolment,_ccof_childcarecategory_value,ccof_cumulativefeeincrease,ccof_feebeforeincrease,ccof_feeincreasetype,ccof_name,_ccof_programyear_value&$filter=(_ccof_adjudicationccfrifacility_value eq " + getCleanedGuid(entityId) + " and ccof_feebeforeincrease ne 'N/A')&$orderby=_ccof_childcarecategory_value asc");
            var ifCalculateInitial = true;  // not equal Stage 1 (NRC)
            if (FeeIncreaseDetails.length === 0) {
                ifCalculateInitial = false;
            } else {
                if (ccfriFacilityQCDecision != 100000002) {
                    for (let i in FeeIncreaseDetails) {
                        var ccofname = FeeIncreaseDetails[i]["ccof_name"]
                        var category = FeeIncreaseDetails[i]["_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue"];
                        if (InitialTotalAllowableStagePolicy[category] === null) {
                            ifCalculateInitial = false;
                        }
                    }
                }
            }
            if (!ifCalculateInitial) {
                Xrm.Navigation.openAlertDialog("The Initial Adjudication is not in Stage 1, the Allowable Fee Increase must not be blank", alertOptions);
                return;
            }
            InitialTotalAllowableStagePolicy = {
                "0-18": (FacilityAmountAllowedRecords[0]['ccof_0to18months'] === null) ? 0 : parseFloat(FacilityAmountAllowedRecords[0]['ccof_0to18months']),
                "18-36": (FacilityAmountAllowedRecords[0]['ccof_18to36months'] === null) ? 0 : parseFloat(FacilityAmountAllowedRecords[0]['ccof_18to36months']),
                "PRE": (FacilityAmountAllowedRecords[0]['ccof_preschool'] === null) ? 0 : parseFloat(FacilityAmountAllowedRecords[0]['ccof_preschool']),
                "OOSC-K": (FacilityAmountAllowedRecords[0]['ccof_outofschoolcarekindergarten'] === null) ? 0 : parseFloat(FacilityAmountAllowedRecords[0]['ccof_outofschoolcarekindergarten']),
                "3Y-K": (FacilityAmountAllowedRecords[0]['ccof_3yearstokindergarten'] === null) ? 0 : parseFloat(FacilityAmountAllowedRecords[0]['ccof_3yearstokindergarten']),
                "OOSC-G": (FacilityAmountAllowedRecords[0]['ccof_outofschoolcaregrade1'] === null) ? 0 : parseFloat(FacilityAmountAllowedRecords[0]['ccof_outofschoolcaregrade1'])
            }

            //Check if MTFIs exist  ccof_mtfi_sequence_number
            var MTFIInfo = getSyncMultipleRecord("ccof_change_request_mtfis?$select=_ccof_ccfri_value,_ccof_ccfri_facility_value,ccof_change_request_mtfiid,ccof_limitfeestonmfbenchmark,ccof_meficap,ccof_stage3calculatornotesinitial,ccof_totalexpenses_exceptionalcircumstances,ccof_totalexpenses_priorityserviceexpansion,ccof_totalexpenses_wageincrease&$expand=ccof_ProgramYear($select=ccof_programyearnumber)&$filter=(_ccof_ccfri_facility_value eq " + getCleanedGuid(entityId) + ") and (statuscode eq 6 or statuscode eq 3)  and (ccof_ProgramYear/ccof_programyearnumber eq " + programYearNumber + "and ccof_mtfi_sequence_number lt " + MTFISequenceNumber + ")&$orderby=ccof_mtfi_sequence_number asc");
            if (MTFIInfo.length > 0) {
                for (let i in MTFIInfo) {
                    var mtfiEntityId = MTFIInfo[i]['ccof_change_request_mtfiid']; // get MTFI record id
                    var mtfiAppCCFRI = MTFIInfo[i]['_ccof_ccfri_value'];
                    FacilityAmountAllowedRecords = getSyncMultipleRecord("ccof_ccfri_facility_allowable_amount_mtfis?$select=ccof_3yearstokindergarten,ccof_outofschoolcarekindergarten,ccof_preschool,ccof_18to36months,ccof_0to18months,ccof_outofschoolcaregrade1,ccof_stage3policy,ccof_displayorder&$filter=(ccof_stage3policy eq 100000006 and _ccof_ccfrifacilitymtfi_value eq " + mtfiEntityId + ")&$top=50&$orderby=ccof_displayorder asc");
                    // calculate combined approvable value.
                    InitialTotalAllowableStagePolicy = {
                        "0-18": (FacilityAmountAllowedRecords[0]['ccof_0to18months'] === null) ? InitialTotalAllowableStagePolicy['0-18'] : InitialTotalAllowableStagePolicy['0-18'] + parseFloat(FacilityAmountAllowedRecords[0]['ccof_0to18months']),
                        "18-36": (FacilityAmountAllowedRecords[0]['ccof_18to36months'] === null) ? InitialTotalAllowableStagePolicy['18-36'] : InitialTotalAllowableStagePolicy['18-36'] + parseFloat(FacilityAmountAllowedRecords[0]['ccof_18to36months']),
                        "PRE": (FacilityAmountAllowedRecords[0]['ccof_preschool'] === null) ? InitialTotalAllowableStagePolicy['PRE'] : InitialTotalAllowableStagePolicy['PRE'] + parseFloat(FacilityAmountAllowedRecords[0]['ccof_preschool']),
                        "OOSC-K": (FacilityAmountAllowedRecords[0]['ccof_outofschoolcarekindergarten'] === null) ? InitialTotalAllowableStagePolicy['OOSC-K'] : InitialTotalAllowableStagePolicy['OOSC-K'] + parseFloat(FacilityAmountAllowedRecords[0]['ccof_outofschoolcarekindergarten']),
                        "3Y-K": (FacilityAmountAllowedRecords[0]['ccof_3yearstokindergarten'] === null) ? InitialTotalAllowableStagePolicy['3Y-K'] : InitialTotalAllowableStagePolicy['3Y-K'] + parseFloat(FacilityAmountAllowedRecords[0]['ccof_3yearstokindergarten']),
                        "OOSC-G": (FacilityAmountAllowedRecords[0]['ccof_outofschoolcaregrade1'] === null) ? InitialTotalAllowableStagePolicy['OOSC-G'] : InitialTotalAllowableStagePolicy['OOSC-G'] + parseFloat(FacilityAmountAllowedRecords[0]['ccof_outofschoolcaregrade1'])
                    }
                }
            }
            //  MTFI CALCULATION STARTED
            console.log("MTFI Adjudication Calculation started....");
            // Get MTFI expense Info, Region, Mefi cap, NMF info.
            var mtfiFeeIncreaseDetails = getSyncMultipleRecord("ccof_ccfri_facility_fee_increase_mtfis?$select=ccof_averageenrolment,_ccof_ccfrifacilitymtfi_value,_ccof_childcarecategory_value,ccof_mtfi_amount,ccof_mtfifeeincreasepercent,ccof_feebeforeincrease,ccof_feeincreasetype,ccof_name,_ccof_programyear_value&$filter=(_ccof_ccfrifacilitymtfi_value eq " + getCleanedGuid(currentMTFIentityId) + " and ccof_feebeforeincrease ne 'N/A')&$orderby=_ccof_childcarecategory_value asc");
            if (mtfiFeeIncreaseDetails.length > 0) {
                var MTFIExpenseInfo = {};
                MTFIExpenseInfo['Exceptional Circumstances'] = formContext.getAttribute("ccof_totalexpenses_exceptionalcircumstances").getValue();
                MTFIExpenseInfo['Direct Care Staff Wages'] = formContext.getAttribute("ccof_totalexpenses_wageincrease").getValue();
                MTFIExpenseInfo['MTFI:Unused Expenses'] = MTFIUnusedExpenses;
                MTFIExpenseInfo['Priority Service Expansion'] = formContext.getAttribute("ccof_totalexpenses_priorityserviceexpansion").getValue();
                MTFIExpenseInfo['Total Monthly Expenses'] = MTFIExpenseInfo['Exceptional Circumstances'] + MTFIExpenseInfo['Direct Care Staff Wages'] + MTFIExpenseInfo['Priority Service Expansion'] + MTFIExpenseInfo['MTFI:Unused Expenses'];
                MTFIExpenseInfo['MEFI Cap'] = formContext.getAttribute("ccof_meficap").getValue();
                MTFIExpenseInfo['Limit Fees to NMF Benchmark'] = formContext.getAttribute("ccof_limitfeestonmfbenchmark").getValue();
                var mefiCAP = formContext.getAttribute("ccof_meficap").getValue();
                var limitfeestonmfbenchmark = formContext.getAttribute("ccof_limitfeestonmfbenchmark").getValue();
                var MTFIRegionInfos = getSyncSingleRecord("ccof_applicationccfris(" + getCleanedGuid(MTFIappCCFRI) + ")?$select=ccof_applicationccfriid,_ccof_region_value&$expand=ccof_Region3PctMedian($select=ccof_0to18months,ccof_10percentageof0to18,ccof_10percentageof18to36,ccof_10percentageof3ytok,ccof_10percentageofoosctog,ccof_10percentageofoosctok,ccof_10percenatgeofpre,ccof_18to36months,ccof_3percentageof0to18,ccof_3percentageof18to36,ccof_3percentageof3ytok,_ccof_3percentmedian_value,ccof_3percentageofoosctog,ccof_3percentageofoosctok,ccof_3percentageofpre,ccof_3yearstokindergarten,ccof_name,ccof_outofschoolcaregrade1,ccof_outofschoolcarekindergarten,ccof_preschool),ccof_RegionNMFBenchmark($select=ccof_fee_benchmark_sdaid,ccof_0to18m,ccof_18to36m,ccof_3ytok,ccof_name,ccof_oosctograde,ccof_oosctok,ccof_preschool)");
                //  Validate FeeIncrease info
                var ifCalculateMTFI = true;
                if (mtfiFeeIncreaseDetails.length === 0) {
                    ifCalculateMTFI = false;
                } else {
                    for (let i in mtfiFeeIncreaseDetails) {
                        if (mtfiFeeIncreaseDetails[i]['ccof_averageenrolment'] === null || mtfiFeeIncreaseDetails[i]['ccof_mtfi_amount'] === null || mtfiFeeIncreaseDetails[i]['ccof_feebeforeincrease'] === null) {

                            ifCalculateMTFI = false;
                        }
                    }
                }
                if (ifCalculateMTFI) {
                    FacilityAmountAllowedRecords = getSyncMultipleRecord("ccof_ccfri_facility_allowable_amount_mtfis?$select=ccof_3yearstokindergarten,ccof_outofschoolcarekindergarten,ccof_preschool,ccof_18to36months,ccof_0to18months,ccof_outofschoolcaregrade1,ccof_stage3policy,ccof_displayorder&$filter=(_ccof_ccfrifacilitymtfi_value eq " + currentMTFIentityId + ")&$top=50&$orderby=ccof_displayorder asc");
                    isMTFI = true;
                    var returnValue = Calculator(MTFIRegionInfos, mtfiFeeIncreaseDetails, MTFIExpenseInfo, InitialTotalAllowableStagePolicy, Initialstage2capammount, newModifiedQCDecision, isMTFI);
                    var TotalAllowableStagePolicy = PopulateSummaryApprovedAmount(returnValue, FacilityAmountAllowedRecords, ccfri_facility_allowable_amount_mtfiEntityName, mtfiFeeIncreaseDetails, MTFIRegionInfos, currentMTFIentityId, mefiCAP, limitfeestonmfbenchmark, InitialTotalAllowableStagePolicy, Initialstage2capammount, newModifiedQCDecision, isMTFI);

                    var mtfistage3calculatornotes = {
                        "ccof_stage3calculatornotesinitial": returnValue['AdjudicatorNote']
                    }
                    UpdateEntityRecord("ccof_change_request_mtfi", currentMTFIentityId, mtfistage3calculatornotes);
                    console.log("End MTFI Adjudication Calculation");
                    formContext.data.save();
                    ////refresh the summary grid
                    formContext.getControl("AllowableAmountforMTFI").refresh();
                    formContext.data.refresh(true);
                    Xrm.Navigation.openAlertDialog("MTFI Adjudication is completed", alertOptions);
                } else {
                    Xrm.Navigation.openAlertDialog(" MTFI will not be calculated as Fee Increase Amount is missing! or No Fee Increase Amount data need to be calculated! ", alertOptions);
                }
            }
            else {
                Xrm.Navigation.openAlertDialog(" MTFI will not be calculated as Fee Increase Amount is missing! or No Fee Increase Amount data need to be calculated! ", alertOptions);
            }
        }
        catch (err) {
            alert("There are some exceptional errors happened, please contact Administrator!" + err);
        }
    },
};

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

function Calculator(regionInfo, feeIncreaseDetails, expenseInfo, InitialTotalAllowableStagePolicy, Initialstage2capammount, newModifiedQCDecision, isMTFI) {
    debugger;
    // var OrgType = regionInfo['ccof_Application']['ccof_providertype@OData.Community.Display.V1.FormattedValue'];   //"Group" or Family
    var SDA = regionInfo['_ccof_region_value@OData.Community.Display.V1.FormattedValue']; //"North Fraser";
    // var Programyear = regionInfo['ccof_Application']['_ccof_programyear_value@OData.Community.Display.V1.FormattedValue']; //"2022/23";
    var Limitfeesto70Percentile = expenseInfo['Limit Fees to NMF Benchmark'];   
    // C33 of Stage 3 Calculator is from CRM Limit Fees to NMF Benchmark  (toggle) 

    //var DilutionCap = expenseInfo['MEFI Cap']; // CRM MEFI Cap (toggle) //B7  ?? need confirm  // comment it Oct 24, 2023
    var InitalCalculation_DilutionCap = true;  // B7 of Calculations
    var TotalAllowedExpenses = 0; //B13 Total Allowed Expenses
    var AllowedExpensesLessExpenses = true;// B14 Allowed Expenses less than or equal to expenses
    var TotalIfLessThanCapped = true; // Pass // F15:If less than, capped ?; F16:Full Allowance Given ? (if less than request)
    var TotalFullAllowanceGiven = true;
    var CalculatorValidation = true;  // I12 J12 Populate Stage 3 Calculator  Calculator Validation

    // CRM Childcare Categories name: 0-18, 18-36,3Y-K,OOSC-K,OOSC-G,PRE  Map CRM childcare Categories to Calculator categories and Total Allowable Fee Increase's logical name
    const ChildcareCategories = new Map([  // for future refator
        ['0-18', ['0-18', 'ccof_total018']],
        ['18-36', ['18-36', 'ccof_total']],
        ['3Y-K', ['3Y-K', 'ccof_tota']],
        ['OOSC-K', ['OOSC-K', 'ccof_tota']],
        ['OOSC-G', ['OOSC-G', 'ccof_tota']],
        ['PRE', ['PRE', 'ccof_tota']],
    ]);
    // console.log(ChildcareCategories.get('0-18')[0]);


    var TotalMonthlyExpenses = expenseInfo['Total Monthly Expenses'];// B11 ? comes from CRM

    var FacilityExpense =   // comes from CRM 
    {
        "Exceptional Circumstances": expenseInfo['Exceptional Circumstances'],
        "Direct Care Staff Wages": expenseInfo['Direct Care Staff Wages'],
        // "MTFI: Unused Expenses": 0,
        "MTFI: Unused Expenses": expenseInfo['MTFI:Unused Expenses'],
        "Priority Service Expansion": expenseInfo['Priority Service Expansion'],
        "Priority SE (Indigenous)": 0,
        "Total Monthly Expenses": 0  // no use now.
    }
    var FacilityInfo = [];
    for (let i in feeIncreaseDetails) {
        let entity = {};
        entity['CareCategory'] = feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'];
        entity['AverageEnrollment'] = parseFloat(feeIncreaseDetails[i]['ccof_averageenrolment']);
        if (isMTFI) {
            entity['RequestedFeeIncrease'] = feeIncreaseDetails[i]['ccof_mtfi_amount'];
        }
        else {
            entity['RequestedFeeIncrease'] = feeIncreaseDetails[i]['ccof_cumulativefeeincrease'];
        }

        entity['FeeBeforeIncrease'] = feeIncreaseDetails[i]['ccof_feebeforeincrease'];
        FacilityInfo.push(entity);
    }
    console.log("FacilityInfo" + JSON.stringify(FacilityInfo));
    var MediansFee = // 10 MEFI from Median table based on SDA,Org,OrgType and Program year.  3%  need to confirm. 
    {
        "0-18": regionInfo['ccof_Region3PctMedian']['ccof_0to18months'],
        "18-36": regionInfo['ccof_Region3PctMedian']['ccof_18to36months'],
        "3Y-K": regionInfo['ccof_Region3PctMedian']['ccof_3yearstokindergarten'],
        "OOSC-K": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcarekindergarten'],
        "OOSC-G": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcaregrade1'],
        "PRE": regionInfo['ccof_Region3PctMedian']['ccof_preschool'],
        "0-18_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof0to18'],
        "18-36_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof18to36'],
        "3Y-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof3ytok'],
        "OOSC-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctok'],
        "OOSC-G_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctog'],
        "PRE_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofpre'],
        "0-18_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof0to18'],
        "18-36_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof18to36'],
        "3Y-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof3ytok'],
        "OOSC-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctok'],
        "OOSC-G_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctog'],
        "PRE_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percenatgeofpre']
    }
    console.log("MediansFee" + JSON.stringify(MediansFee));
    var SDA70thPercentileF = // 70th Percentile for SDA // comes from CRM NMF Benchmarks table based on SDA, Org,OrgType and Program year
    {
        "0-18": regionInfo['ccof_RegionNMFBenchmark']['ccof_0to18m'],
        "18-36": regionInfo['ccof_RegionNMFBenchmark']['ccof_18to36m'],
        "3Y-K": regionInfo['ccof_RegionNMFBenchmark']['ccof_3ytok'],
        "OOSC-K": regionInfo['ccof_RegionNMFBenchmark']['ccof_oosctok'],
        "OOSC-G": regionInfo['ccof_RegionNMFBenchmark']['ccof_oosctograde'],
        "PRE": regionInfo['ccof_RegionNMFBenchmark']['ccof_preschool'],
    }
    console.log("SDA70thPercentileF" + JSON.stringify(SDA70thPercentileF));

    // Populate Allowances from Median 3% fields
    //  Get NMF Increase Cap based on FacilityInfo get from CRM
    var NMFIncreaseCap = {};
    var AllowancesOnCalculator = {};  // Stage3 Calculator 
    var InitalCalculation = {};
    for (let i in FacilityInfo) {
        let tempCap = {};
        let tempAllowances = {};
        let tempInital = {};
        // Cap = SDA70thPercentileF(NMFBenchmark)-FeeBeforeIncrease and get min compare with 10% of Median
        tempCap['Cap'] = Limitfeesto70Percentile ? (Math.round(SDA70thPercentileF[FacilityInfo[i]['CareCategory']] - FacilityInfo[i]['FeeBeforeIncrease'])) : null;
        // FacilityInfo[i]['CareCategory'] childcare category name + Per10 to get per10 median field's name
        tempCap['Lesser'] = (tempCap['Cap'] === null) ? MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per10')] : Math.min(tempCap['Cap'], MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per10')]);
        NMFIncreaseCap[FacilityInfo[i]['CareCategory']] = tempCap;
        // FacilityInfo[i]['CareCategory'] childcare category name + Per10 to get per3 median field's name
        if (isMTFI) {
            // Oct 19, 2023 updated 
            //if ((newModifiedQCDecision == 100000001 || newModifiedQCDecision == 100000002) && TotalMonthlyExpenses != 0) {
            if ((newModifiedQCDecision == 100000001 || newModifiedQCDecision == 100000002)) {

                tempAllowances['3% Allowable Fee Increase'] = 0;
            }
            else {
                // updated logic based on ticket 2978
               // tempAllowances['3% Allowable Fee Increase'] = MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per3')] - InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']] ?;
                tempAllowances['3% Allowable Fee Increase'] = ((MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per3')] - InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']]) < 0) ? 0 : (MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per3')] - InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']]);
            }

        }
        else {
            tempAllowances['3% Allowable Fee Increase'] = MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per3')];
        }
        //tempAllowances['3% Allowable Fee Increase'] = MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per3')];
        AllowancesOnCalculator[FacilityInfo[i]['CareCategory']] = tempAllowances;
        // Populate Inital Calculation B3 Average Enrollment
        tempInital['Average Enrollment'] = FacilityInfo[i]['AverageEnrollment'];
        // Calculation!B4  // Updated for MTFI Oct 10,2023
        if (isMTFI) {
            tempInital['Allowances'] = (tempCap['Cap'] === null) ? tempAllowances['3% Allowable Fee Increase'] + InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']] : ((tempCap['Cap'] < tempAllowances['3% Allowable Fee Increase'] + InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']]) ? tempCap['Cap'] : (tempAllowances['3% Allowable Fee Increase'] + InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']]));
            // Calculation!B5  // Updated for MTFI Oct 10,2023
            tempInital['Request'] = FacilityInfo[i]['RequestedFeeIncrease'] + InitialTotalAllowableStagePolicy[FacilityInfo[i]['CareCategory']];
            // tempInital['Request'] = FacilityInfo[i]['RequestedFeeIncrease'] + PreviouslyApprovedIncreaseAmount[FacilityInfo[i]['CareCategory']]['Max Approvable'];
        } else {
            tempInital['Allowances'] = (tempCap['Cap'] === null) ? tempAllowances['3% Allowable Fee Increase'] : ((tempCap['Cap'] < tempAllowances['3% Allowable Fee Increase']) ? tempCap['Cap'] : (tempAllowances['3% Allowable Fee Increase']));
            // Calculation!B5  // Updated for MTFI Oct 10,2023
            tempInital['Request'] = FacilityInfo[i]['RequestedFeeIncrease'];
        }
        // Calculation!B6. K3=Limit fees to 70 Percentile
        tempInital['Dilution Cap'] = Limitfeesto70Percentile ? tempCap['Lesser'] : MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per10')];
        // Calculation!B7
        HstCap5Percent = true;         // Oct 19,2023 
        //       InitalCalculation_DilutionCap = Limitfeesto70Percentile;
        InitalCalculation_DilutionCap = Limitfeesto70Percentile || HstCap5Percent;
        // inital B9, will get it  until Round end.
        tempInital['FINAL APPROVABLE'] = 0;
        // Calculation!B10 Request/Dilution Cap Result
        tempInital['Request/Dilution Cap Result'] = (InitalCalculation_DilutionCap && (tempInital['Request'] > tempInital['Dilution Cap'])) ? tempInital['Dilution Cap'] : tempInital['Request'];
        InitalCalculation[FacilityInfo[i]['CareCategory']] = tempInital;
    }
    console.log("NMFIncreaseCap:" + JSON.stringify(NMFIncreaseCap));
    console.log("InitalCalculation" + JSON.stringify(InitalCalculation));
    // Round 1 
    // B11=Expense=Total Monthly Expense= sum(Exceptional Circumstances;Direct Care Staff Wages Priority Service Expansion(Extended Hours or Indigenous))
    var Round1 = {};
    // get B20, B21 first
    for (const item in InitalCalculation) {
        let entity = {};
        // Round 1 Calculation B20
        entity['Enrollment'] = (TotalMonthlyExpenses === 0) ? 0 : InitalCalculation[item]['Average Enrollment'];
        //Round 1 Calculation B21
        if (isMTFI) {
            //if (entity['Enrollment'] == 0) {
            //    entity['Allowance'] = (InitalCalculation[item]['Allowances'] < (InitalCalculation[item]['Request/Dilution Cap Result']) + parseFloat(InitialTotalAllowableStagePolicy[item].toFixed(2))) ? InitalCalculation[item]['Allowances'] + parseFloat(InitialTotalAllowableStagePolicy[item].toFixed(2)) : InitalCalculation[item]['Request/Dilution Cap Result'];
            //}
            //else {
            entity['Allowance'] = (InitalCalculation[item]['Allowances'] < InitalCalculation[item]['Request/Dilution Cap Result']) ? InitalCalculation[item]['Allowances'] : InitalCalculation[item]['Request/Dilution Cap Result'];
            //}

        }
        else {
            entity['Allowance'] = (InitalCalculation[item]['Allowances'] < InitalCalculation[item]['Request/Dilution Cap Result']) ? InitalCalculation[item]['Allowances'] : InitalCalculation[item]['Request/Dilution Cap Result'];
        }

        Round1[item] = entity;
        // console.log("Round1 first: " + JSON.stringify(Round1));
    }
    // Populate Get Round 1 B22 Final Amount
    var sumEnrollment = 0;
    var weightSumAllowance = 0;
    for (const item in Round1) {
        sumEnrollment = sumEnrollment + Round1[item]['Enrollment'];
        weightSumAllowance = weightSumAllowance + Round1[item]['Enrollment'] * Round1[item]['Allowance']
        //console.log("sumEnrollment: " + sumEnrollment + "weightSumAllowance:  " + weightSumAllowance);
    }
    // Get rest rows of Round1 
    var Round1RevenueAllowed = 0;
    for (const item in InitalCalculation) {
        //Round 1 Calculation B22. if B20===0 
        if (Round1[item]['Enrollment'] === 0) {
            Round1[item]['Final Amount'] = Round1[item]['Allowance'];

        } else {
            Round1[item]['Final Amount'] = ((TotalMonthlyExpenses + weightSumAllowance) / sumEnrollment);

        }
        // B23=B22-B21 Amount added=Final Amount-Allowance
        Round1[item]['Amount added'] = (Round1[item]['Final Amount'] - Round1[item]['Allowance']);
        //  Round1[item]['Amount added'] = (Round1[item]['Final Amount'] - Round1[item]['Allowance']).toFixed(2);

        // B24  Check for negative
        Round1[item]['Check for negative'] = (Round1[item]['Amount added'] < 0) ? true : false;
        // B25 Approvable 1 
        Round1[item]['Approvable 1'] = !Round1[item]['Check for negative'] ? Round1[item]['Final Amount'] : ((Round1[item]['Final Amount'] > Round1[item]['Allowance']) ? Round1[item]['Final Amount'] : Round1[item]['Allowance']);
        // B26 Check for dilution cap
        Round1[item]['Check for dilution cap'] = ((Round1[item]['Final Amount'] > InitalCalculation[item]['Dilution Cap']) && InitalCalculation_DilutionCap) ? true : false;
        // B27 Approvable 2
        Round1[item]['Approvable 2'] = !Round1[item]['Check for dilution cap'] ? Round1[item]['Approvable 1'] : InitalCalculation[item]['Dilution Cap'];
        // B28 Check for request cap  Request/Dilution Cap Result
        Round1[item]['Check for request cap'] = (Round1[item]['Final Amount'] > InitalCalculation[item]['Request/Dilution Cap Result']) ? true : false;
        // B29 Final approvable 
        Round1[item]['Final approvable'] = !Round1[item]['Check for request cap'] ? Round1[item]['Approvable 2'] : InitalCalculation[item]['Request/Dilution Cap Result'];
        // populate  B9     InitalCalculation[item]['FINAL APPROVABLE']

        // populate  B9     InitalCalculation[item]['FINAL APPROVABLE']
        InitalCalculation[item]['FINAL APPROVABLE'] = (InitalCalculation_DilutionCap && (InitalCalculation[item]['Allowances'] > InitalCalculation[item]['Dilution Cap'])) ?
            ((InitalCalculation[item]['Request'] < InitalCalculation[item]['Allowances']) ? InitalCalculation[item]['Request'] : InitalCalculation[item]['Allowances']) : Round1[item]['Final approvable'];
        //  Calculation!B12 Allowed Expense /category
        InitalCalculation[item]['Allowed Expense /category'] = (InitalCalculation[item]['Allowances'] > InitalCalculation[item]['Request']) ? 0 :
            InitalCalculation[item]['Average Enrollment'] * (InitalCalculation[item]['FINAL APPROVABLE'] - InitalCalculation[item]['Allowances']);

        // get  B31 Revenue allowed
        Round1RevenueAllowed = Round1RevenueAllowed + Round1[item]['Enrollment'] * (Round1[item]['Final approvable'] - Round1[item]['Allowance']);
        // console.log(Round1RevenueAllowed);
    }
    // Get B31 Revenue allowed
    Round1['Revenue allowed'] = Round1RevenueAllowed;
    // Round1['Revenue allowed'] = Round1RevenueAllowed.toFixed(2);

    // get B32 Expenses Left 
    Round1['Expenses Left'] = (TotalMonthlyExpenses - Round1['Revenue allowed']);
    // Round1['Expenses Left'] = (TotalMonthlyExpenses - Round1['Revenue allowed']).toFixed(2);

    console.log("Round1 end:" + JSON.stringify(Round1));
    // Round1 End

    // Populate Left Round 
    var RoundArray = [];
    var roundClone = structuredClone(Round1);
    if (FacilityInfo.length > 1) {
        for (let i = 1; i < FacilityInfo.length; i++) {
            let roundTemp = {};
            // get B20, B21 first Enrollment Allowance.will not use name of Adjusted Enrollment , Adjusted Allowance 
            let tempRevenueAllowed = 0;
            let Revenueallowed = roundClone['Revenue allowed'];
            delete roundClone['Revenue allowed'];
            let ExpensesLeft = parseFloat(roundClone['Expenses Left']);
            delete roundClone['Expenses Left'];
            for (const item in roundClone) {
                let entity = {};
                //Round left Enrollment(Adjusted Enrollment),
                //=IF(AND(B24="Yes",B22<B4, B23<=C23, B23<=D23, B23<=E23, B32<0), 0,IF(AND(OR(C24="Yes",D24="Yes",E24="Yes"), $B$32<0),B20,IF(OR(B26="Yes",B28="Yes"),0,B20)))
                let ifAmountAdded = true;
                let Checkfornegative = false;
                // Enrollment based on B23<=C23, B23<=D23, B23<=E23; C24="Yes",D24="Yes",E24="Yes"
                let tempRoundClone = structuredClone(roundClone);
                let tempAmountAdded = roundClone[item]['Amount added'];
                //console.log("tempAmountAdded:" + tempAmountAdded);
                delete tempRoundClone[item];
                // console.log("tempRoundClone:" + JSON.stringify(tempRoundClone))
                for (const j in tempRoundClone) {
                    // 20230209 fix a bug add parseFloat otherwise the value is not true( -6.59<=-1.59) 
                    ifAmountAdded = ifAmountAdded && (parseFloat(tempAmountAdded) <= parseFloat(tempRoundClone[j]['Amount added']));
                    Checkfornegative = Checkfornegative || tempRoundClone[j]['Check for negative']
                }
                tempRoundClone = {};
                //console.log("ifAmountAdded:" + ifAmountAdded);
                //console.log(" Checkfornegative:" + Checkfornegative);
                if (roundClone[item]['Check for negative'] && (parseFloat(roundClone[item]['Final Amount']) < parseFloat(InitalCalculation[item]['Allowances'])) && ifAmountAdded && (ExpensesLeft < 0)) {
                    entity['Enrollment'] = 0;
                } else {
                    if (Checkfornegative && (ExpensesLeft < 0)) {
                        entity['Enrollment'] = roundClone[item]['Enrollment'];
                    } else {
                        if (roundClone[item]['Check for dilution cap'] || roundClone[item]['Check for request cap']) {
                            entity['Enrollment'] = 0;
                        } else {
                            entity['Enrollment'] = roundClone[item]['Enrollment'];
                        }
                    }
                }
                // Allowance
                entity['Allowance'] = roundClone[item]['Final approvable'];
                roundTemp[item] = entity;
            }
            // Final Amount
            sumEnrollment = 0;
            weightSumAllowance = 0;
            for (const item in roundTemp) {
                sumEnrollment = sumEnrollment + roundTemp[item]['Enrollment'];
                weightSumAllowance = (weightSumAllowance + roundTemp[item]['Enrollment'] * roundTemp[item]['Allowance']);
                // console.log("sumEnrollment: " + sumEnrollment + "weightSumAllowance:  " + weightSumAllowance);
            }
            for (const item in roundClone) {
                if (roundTemp[item]['Enrollment'] === 0) {
                    roundTemp[item]['Final Amount'] = roundTemp[item]['Allowance'];

                } else {
                    // console.log(ExpensesLeft + weightSumAllowance);
                    roundTemp[item]['Final Amount'] = ((ExpensesLeft + weightSumAllowance) / sumEnrollment);
                    //  roundTemp[item]['Final Amount'] = ((ExpensesLeft + weightSumAllowance) / sumEnrollment).toFixed(2);

                }

                // B23=B22-B21 Amount added=Final Amount-Allowance
                roundTemp[item]['Amount added'] = (roundTemp[item]['Final Amount'] - roundTemp[item]['Allowance']);
                // roundTemp[item]['Amount added'] = (roundTemp[item]['Final Amount'] - roundTemp[item]['Allowance']).toFixed(2);

                // B24  Check for negative
                roundTemp[item]['Check for negative'] = (roundTemp[item]['Amount added'] < 0) ? true : false;
                // B25 Approvable 1 // 20230209

                roundTemp[item]['Approvable 1'] = !roundTemp[item]['Check for negative'] ? roundTemp[item]['Final Amount'] : ((parseFloat(roundTemp[item]['Final Amount']) > parseFloat(Round1[item]['Allowance'])) ? parseFloat(roundTemp[item]['Final Amount']) : Round1[item]['Allowance']);
                /* }*/
                // roundTemp[item]['Approvable 1'] = !roundTemp[item]['Check for negative'] ? roundTemp[item]['Final Amount'] : ((parseFloat(roundTemp[item]['Final Amount']) > parseFloat(Round1[item]['Allowance'])) ? parseFloat(roundTemp[item]['Final Amount']) : Round1[item]['Allowance']);
                // B26 Check for dilution cap
                roundTemp[item]['Check for dilution cap'] = ((parseFloat(roundTemp[item]['Final Amount']) > parseFloat(InitalCalculation[item]['Dilution Cap'])) && InitalCalculation_DilutionCap) ? true : false;
                // B27 Approvable 2
                roundTemp[item]['Approvable 2'] = !roundTemp[item]['Check for dilution cap'] ? roundTemp[item]['Approvable 1'] : InitalCalculation[item]['Dilution Cap'];
                // B28 Check for request cap  Request/Dilution Cap Result
                roundTemp[item]['Check for request cap'] = (parseFloat(roundTemp[item]['Final Amount']) > parseFloat(InitalCalculation[item]['Request/Dilution Cap Result'])) ? true : false;
                // B29 Final approvable 
                roundTemp[item]['Final approvable'] = !roundTemp[item]['Check for request cap'] ? roundTemp[item]['Approvable 2'] : InitalCalculation[item]['Request/Dilution Cap Result'];

                if (i === FacilityInfo.length - 1) {
                    // populate B9     // populate FINAL APPROVABLE to InitalCalculation from RoundArray
                    InitalCalculation[item]['FINAL APPROVABLE'] = (InitalCalculation_DilutionCap && (parseFloat(InitalCalculation[item]['Allowances']) > parseFloat(InitalCalculation[item]['Dilution Cap']))) ?
                        ((parseFloat(InitalCalculation[item]['Request']) < parseFloat(InitalCalculation[item]['Allowances'])) ? InitalCalculation[item]['Request'] : InitalCalculation[item]['Allowances']) : roundTemp[item]['Final approvable'];
                    //  Calculation!B12 Allowed Expense /category
                    InitalCalculation[item]['Allowed Expense /category'] = (parseFloat(InitalCalculation[item]['Allowances']) > parseFloat(InitalCalculation[item]['Request'])) ? 0 :
                        InitalCalculation[item]['Average Enrollment'] * (InitalCalculation[item]['FINAL APPROVABLE'] - InitalCalculation[item]['Allowances']);
                }
                // get  B31 Revenue allowed
                tempRevenueAllowed = tempRevenueAllowed + roundTemp[item]['Enrollment'] * (roundTemp[item]['Final approvable'] - roundTemp[item]['Allowance']);
                // console.log(roundTempRevenueAllowed);
            }
            // Get B31 Revenue allowed
            roundTemp['Revenue allowed'] = tempRevenueAllowed;
            // roundTemp['Revenue allowed'] = tempRevenueAllowed.toFixed(2);

            // get B32 Expenses Left 
            roundTemp['Expenses Left'] = (ExpensesLeft - roundTemp['Revenue allowed']);
            //roundTemp['Expenses Left'] = (ExpensesLeft - roundTemp['Revenue allowed']).toFixed(2);

            //console.log("Round1 end:" + JSON.stringify(Round1));
            // Round1 End
            //console.log("roundTemp:" + JSON.stringify(roundTemp));
            RoundArray.push(roundTemp);
            //console.log("RoundArry1:" + JSON.stringify(RoundArray));
            roundClone = {};
            roundClone = structuredClone(roundTemp);
        }
        // console.log("RoundArray:" + JSON.stringify(RoundArray));
        //console.log("Check Finnal Approval " + JSON.stringify(InitalCalculation));
    }
    console.log("RoundArray" + JSON.stringify(RoundArray));

    // Populate SUMMARY CALCULATIONS
    var ChangeEmptyCellstoZero = {};
    for (let i in FacilityInfo) {
        let temp = {};
        temp['No Fee Increase in Years'] = AllowancesOnCalculator[FacilityInfo[i]['CareCategory']]['3% Allowable Fee Increase'];
        temp['Median'] = 0;//  ='Stage 3 Calculator'!C15.  Black should no use
        temp['Historic'] = 0; // Stage 3 Calculator'!C14= Black should no use
        // below code doesn't work
        //ChangeEmptyCellstoZero[FacilityInfo[i]['CareCategory']]['No Fee Increase in Years'] = AllowancesOnCalculator[FacilityInfo[i]['CareCategory']]['3% Allowable Fee Increase'];
        //ChangeEmptyCellstoZero[FacilityInfo[i]['CareCategory']]['Median'] = 0;   //  ='Stage 3 Calculator'!C15.  Black should no use
        //ChangeEmptyCellstoZero[FacilityInfo[i]['CareCategory']]['Historic'] = 0; // Stage 3 Calculator'!C14= Black should no use
        ChangeEmptyCellstoZero[FacilityInfo[i]['CareCategory']] = temp;
        // console.log("ChangeEmptyCellstoZero" + JSON.stringify(ChangeEmptyCellstoZero));
    }
    // Populate B13 Total Allowed Expenses, B15:If less than, capped?;B16:Full Allowance Given? (if less than request);
    // var TotalIfLessThanCapped = true; var TotalFullAllowanceGiven = true; // Pass // B15:If less than, capped ?; B16:Full Allowance Given ? (if less than request)
    for (const item in InitalCalculation) {
        TotalAllowedExpenses = TotalAllowedExpenses + InitalCalculation[item]['Allowed Expense /category'];
        //  console.log("TotalAllowedExpenses(B13): " + TotalAllowedExpenses);
    }
    TotalAllowedExpenses = TotalAllowedExpenses.toFixed(2);     // fixed round number issue ticket 2840 // Oct 2, 2023
    for (const item in InitalCalculation) {
        // B15 If less than, capped?
        InitalCalculation[item]['If less than, capped'] = ((TotalAllowedExpenses < TotalMonthlyExpenses)
            && (parseFloat(InitalCalculation[item]['FINAL APPROVABLE']) < parseFloat(InitalCalculation[item]['Request']))
            && (parseFloat(InitalCalculation[item]['FINAL APPROVABLE']) < parseFloat(InitalCalculation[item]['Dilution Cap']))) ? false : true;
        // F15
        TotalIfLessThanCapped = TotalIfLessThanCapped && InitalCalculation[item]['If less than, capped'];
        // B16 Full Allowance Given? (if less than request)
        InitalCalculation[item]['Full Allowance Given'] = ((parseFloat(InitalCalculation[item]['FINAL APPROVABLE']) < parseFloat(InitalCalculation[item]['Allowances']))
            && (parseFloat(InitalCalculation[item]['FINAL APPROVABLE']) < parseFloat(InitalCalculation[item]['Request']))) ? false : true;
        // F16
        TotalFullAllowanceGiven = TotalFullAllowanceGiven && InitalCalculation[item]['Full Allowance Given'];
    }
    // Populate B14 Allowed Expenses less than or equal to expenses. TotalAllowedExpenses(B13), TotalMonthlyExpenses(B11)
    AllowedExpensesLessExpenses = (TotalAllowedExpenses <= TotalMonthlyExpenses) ? true : false;
    // Populate Stage 3 Calculator  Calculator Validation I12
    CalculatorValidation = AllowedExpensesLessExpenses && TotalIfLessThanCapped && TotalFullAllowanceGiven;

    // Populate row88 Total and Total Approved Exceptional Circumstances Direct Care Staff Wages etc.
    var SummaryCalculationsTotalApproved = {};
    SummaryCalculationsTotalApproved['Exceptional Circumstances'] = {};
    SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total'] = FacilityExpense['Exceptional Circumstances'];
    SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total Approved'] = (parseFloat(TotalAllowedExpenses) <= parseFloat(SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total'])) ?
        TotalAllowedExpenses : SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total'];
    SummaryCalculationsTotalApproved['Exceptional Circumstances']['Remainder'] = TotalAllowedExpenses - SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total Approved'];
    SummaryCalculationsTotalApproved['Direct Care Staff Wages'] = {};
    SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total'] = FacilityExpense['Direct Care Staff Wages'];
    SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total Approved'] = (parseFloat(SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total']) <= parseFloat(SummaryCalculationsTotalApproved['Exceptional Circumstances']['Remainder'])) ?
        SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total'] : SummaryCalculationsTotalApproved['Exceptional Circumstances']['Remainder'];
    SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Remainder'] = SummaryCalculationsTotalApproved['Exceptional Circumstances']['Remainder'] - SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total Approved'];

    SummaryCalculationsTotalApproved['Priority SE (Inclusive)'] = {};
    SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total'] = FacilityExpense['MTFI: Unused Expenses'];
    SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total Approved'] = (parseFloat(SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total']) <= parseFloat(SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Remainder'])) ?
        SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total'] : SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Remainder'];
    SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Remainder'] = SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Remainder'] - SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total Approved'];

    SummaryCalculationsTotalApproved['Priority SE (Extended Hours)'] = {};
    SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total'] = FacilityExpense['Priority Service Expansion'];
    SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total Approved'] = (parseFloat(SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total']) <= parseFloat(SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Remainder']))
        ? SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total'] : SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Remainder'];
    SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Remainder'] = SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Remainder'] - SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total Approved'];

    SummaryCalculationsTotalApproved['Priority SE (Indigenous)'] = {};
    SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total'] = FacilityExpense['Priority SE (Indigenous)'];
    SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total Approved'] = (parseFloat(SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total']) <= parseFloat(SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Remainder']))
        ? SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total'] : SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Remainder'];
    SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Remainder']
    console.log("SummaryCalculationsTotalApproved" + JSON.stringify(SummaryCalculationsTotalApproved));

    // Populate row 99 Policies Applied
    var PoliciesApplied = {};
    var stringPOLICIESUSED = " POLICIES USED:";
    PoliciesApplied['Policy Used'] = {};
    PoliciesApplied['Policy Used']['Nominal'] = 0;
    PoliciesApplied['Policy Used']['Priority SE (Indigenous)'] = 0;
    PoliciesApplied['Policy Used']['Priority SE (Extended Hours)'] = 0;
    PoliciesApplied['Policy Used']['MTFI Unused'] = 0;
    PoliciesApplied['Policy Used']['Direct Care Staff Wages'] = 0;
    PoliciesApplied['Policy Used']['Exceptional Circumstances'] = 0;
    PoliciesApplied['Policy Used']['Historic'] = 0;
    PoliciesApplied['Policy Used']['No Fee Increase in Years'] = 0;
    PoliciesApplied['Policy Used']['Median'] = 0;
    for (const item in InitalCalculation) {
        let entity = {};
        // B109
        // entity['Nominal'] = (InitalCalculation[item]['Request/Dilution Cap Result'] > 1) ? true : false;
        entity['Nominal'] = false;  // based meeting on Jan 09, 2023 with Brain
        if (entity['Nominal'] === true) PoliciesApplied['Policy Used']['Nominal'] = PoliciesApplied['Policy Used']['Nominal'] + 1;
        // B77 = RoundArray[FacilityInfo.length - 2][item]['Final approvable']
        // B108
        if (FacilityInfo.length > 1) {
            entity['Priority SE (Indigenous)'] = ((parseFloat(RoundArray[FacilityInfo.length - 2][item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (parseFloat(SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total']) > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Priority SE (Indigenous)'] === true) PoliciesApplied['Policy Used']['Priority SE (Indigenous)'] = PoliciesApplied['Policy Used']['Priority SE(Indigenous)'] + 1;
            // B107
            entity['Priority SE (Extended Hours)'] = ((parseFloat(RoundArray[FacilityInfo.length - 2][item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (parseFloat(SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total']) > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Priority SE (Extended Hours)'] === true) PoliciesApplied['Policy Used']['Priority SE (Extended Hours)'] = PoliciesApplied['Policy Used']['Priority SE (Extended Hours)'] + 1;
            // B106
            entity['MTFI Unused'] = ((parseFloat(RoundArray[FacilityInfo.length - 2][item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (parseFloat(SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total']) > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['MTFI Unused'] === true) PoliciesApplied['Policy Used']['MTFI Unused'] = PoliciesApplied['Policy Used']['MTFI Unused'] + 1;
            // B105
            entity['Direct Care Staff Wages'] = ((parseFloat(RoundArray[FacilityInfo.length - 2][item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (parseFloat(SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total']) > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Direct Care Staff Wages'] === true) PoliciesApplied['Policy Used']['Direct Care Staff Wages'] = PoliciesApplied['Policy Used']['Direct Care Staff Wages'] + 1;
            // B104
            entity['Exceptional Circumstances'] = ((parseFloat(RoundArray[FacilityInfo.length - 2][item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total'] > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Exceptional Circumstances'] === true) PoliciesApplied['Policy Used']['Exceptional Circumstances'] = PoliciesApplied['Policy Used']['Exceptional Circumstances'] + 1;

        } else { // ===1 
            entity['Priority SE (Indigenous)'] = ((parseFloat(Round1[item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total'] > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Priority SE (Indigenous)'] === true) PoliciesApplied['Policy Used']['Priority SE (Indigenous)'] = PoliciesApplied['Policy Used']['Priority SE(Indigenous)'] + 1;
            // B107
            entity['Priority SE (Extended Hours)'] = ((parseFloat(Round1[item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total'] > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Priority SE (Extended Hours)'] === true) PoliciesApplied['Policy Used']['Priority SE (Extended Hours)'] = PoliciesApplied['Policy Used']['Priority SE (Extended Hours)'] + 1;
            // B106
            entity['MTFI Unused'] = ((parseFloat(Round1[item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total'] > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['MTFI Unused'] === true) PoliciesApplied['Policy Used']['MTFI Unused'] = PoliciesApplied['Policy Used']['MTFI Unused'] + 1;
            // B105
            entity['Direct Care Staff Wages'] = ((parseFloat(Round1[item]['Final approvable']) > parseFloat(InitalCalculation[item]['Allowances']))
                && (SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total'] > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Direct Care Staff Wages'] === true) PoliciesApplied['Policy Used']['Direct Care Staff Wages'] = PoliciesApplied['Policy Used']['Direct Care Staff Wages'] + 1;
            // B104
            entity['Exceptional Circumstances'] = ((Round1[item]['Final approvable'] > InitalCalculation[item]['Allowances'])
                && (parseFloat(SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total']) > 0) && (entity['Nominal'] === false)) ? true : false;
            if (entity['Exceptional Circumstances'] === true) PoliciesApplied['Policy Used']['Exceptional Circumstances'] = PoliciesApplied['Policy Used']['Exceptional Circumstances'] + 1;
        }
        // B103 Historic
        entity['Historic'] = ((parseFloat(ChangeEmptyCellstoZero[item]['Historic']) >= (parseFloat(ChangeEmptyCellstoZero[item]['No Fee Increase in Years']) + parseFloat(ChangeEmptyCellstoZero[item]['Median'])))
            && (ChangeEmptyCellstoZero[item]['Historic'] > 0) && (entity['Nominal'] === false)) ? true : false;
        if (entity['Historic'] === true) PoliciesApplied['Policy Used']['Historic'] = PoliciesApplied['Policy Used']['Historic'] + 1;
        // B102 No Fee Increase in Years
        entity['No Fee Increase in Years'] = ((entity['Historic'] === false) && (ChangeEmptyCellstoZero[item]['No Fee Increase in Years'] > 0)
            && (parseFloat(InitalCalculation[item]['FINAL APPROVABLE']) > parseFloat(ChangeEmptyCellstoZero[item]['Median'])) && (entity['Nominal'] === false)) ? true : false;
        if (entity['No Fee Increase in Years'] === true) PoliciesApplied['Policy Used']['No Fee Increase in Years'] = PoliciesApplied['Policy Used']['No Fee Increase in Years'] + 1;
        // B101 Median
        entity['Median'] = ((entity['Historic'] === false)
            && (ChangeEmptyCellstoZero[item]['Median'] > 0) && (entity['Nominal'] === false)) ? true : false;
        if (entity['Median'] === true) PoliciesApplied['Policy Used']['Median'] = PoliciesApplied['Policy Used']['Median'] + 1;
        PoliciesApplied[item] = entity;

    }
    // changed % for Years => Fee Increase Limit based on ticket 1135
    stringPOLICIESUSED = stringPOLICIESUSED + ((PoliciesApplied['Policy Used']['Median'] > 0) ? " Median;" : "") + ((PoliciesApplied['Policy Used']['No Fee Increase in Years'] > 0) ? " Fee Increase Limit; " : "")
        + ((PoliciesApplied['Policy Used']['Historic'] > 0) ? "Historic;" : "") + ((PoliciesApplied['Policy Used']['Exceptional Circumstances'] > 0) ? " Exceptional;" : "")
        + ((PoliciesApplied['Policy Used']['Direct Care Staff Wages'] > 0) ? "  DCSW;" : "") + ((PoliciesApplied['Policy Used']['MTFI Unused'] > 0) ? " MTFI Unused;" : "")
        + ((PoliciesApplied['Policy Used']['Priority SE (Extended Hours)'] > 0) ? "  Extended Hours;" : "") + ((PoliciesApplied['Policy Used']['Priority SE (Indigenous)'] > 0) ? "  Indigenous;" : "")
        + ((PoliciesApplied['Policy Used']['Nominal'] > 0) ? " Nominal;" : "");
    console.log("stringPOLICIESUSED:" + stringPOLICIESUSED);
    // console.log(" Policies Applied:" + JSON.stringify(PoliciesApplied));
    //Populate Amount Approved Per Category
    var AmountApprovedPerCategory = {};
    for (const item in InitalCalculation) {
        let entity = {};
        // H101 Median // B9 InitalCalculation[item]['FINAL APPROVABLE']
        entity['Median'] = (PoliciesApplied[item]['Median']) ?
            ((ChangeEmptyCellstoZero[item]['Median'] > InitalCalculation[item]['FINAL APPROVABLE']) ? InitalCalculation[item]['FINAL APPROVABLE'] : ChangeEmptyCellstoZero[item]['Median']) : 0;
        // B102
        entity['No Fee Increase in Years'] = (PoliciesApplied[item]['No Fee Increase in Years'])
            ? (((InitalCalculation[item]['FINAL APPROVABLE'] - ChangeEmptyCellstoZero[item]['Median']) > ChangeEmptyCellstoZero[item]['Increase Allowed for Expenses'])
                ? ChangeEmptyCellstoZero[item]['Increase Allowed for Expenses'] : ((InitalCalculation[item]['FINAL APPROVABLE'] - ChangeEmptyCellstoZero[item]['Median']))) : 0;
        // H103 
        entity['Historic'] = (PoliciesApplied[item]['No Fee Increase in Years'])
            ? ((ChangeEmptyCellstoZero[item]['Historic'] > InitalCalculation[item]['FINAL APPROVABLE'])
                ? InitalCalculation[item]['FINAL APPROVABLE'] : ChangeEmptyCellstoZero[item]['Historic']) : 0;
        // H104
        if (FacilityInfo.length > 1) {
            entity['Increase Allowed for Expenses'] = (RoundArray[FacilityInfo.length - 2][item]['Final approvable'] - Round1[item]['Allowance']).toFixed(2);
        } else { /// ===1 only one care category
            entity['Increase Allowed for Expenses'] = (Round1[item]['Final approvable'] - Round1[item]['Allowance']).toFixed(2);
        }
        // H106 Exceptional Circumstances
        entity['Exceptional Circumstances'] = (TotalAllowedExpenses != 0) ? (SummaryCalculationsTotalApproved['Exceptional Circumstances']['Total Approved'] / TotalAllowedExpenses * entity['Increase Allowed for Expenses']).toFixed(2) : 0;
        // H107 Direct Care Staff Wages
        entity['Direct Care Staff Wages'] = (TotalAllowedExpenses != 0) ? (SummaryCalculationsTotalApproved['Direct Care Staff Wages']['Total Approved'] / TotalAllowedExpenses * entity['Increase Allowed for Expenses']).toFixed(2) : 0;
        // H108 MTFI Unused
        entity['MTFI Unused'] = (TotalAllowedExpenses != 0) ? (SummaryCalculationsTotalApproved['Priority SE (Inclusive)']['Total Approved'] / TotalAllowedExpenses * entity['Increase Allowed for Expenses']).toFixed(2) : 0;
        // H109 Priority SE (Extended Hours)
        entity['Priority SE(Extended Hours)'] = (TotalAllowedExpenses != 0) ? (SummaryCalculationsTotalApproved['Priority SE (Extended Hours)']['Total Approved'] / TotalAllowedExpenses * entity['Increase Allowed for Expenses']).toFixed(2) : 0;
        // H100 Priority SE (Indigenous)
        entity['Priority SE (Indigenous)'] = (TotalAllowedExpenses != 0) ? (SummaryCalculationsTotalApproved['Priority SE (Indigenous)']['Total Approved'] / TotalAllowedExpenses * entity['Increase Allowed for Expenses']).toFixed(2) : 0;
        // H113 Nominal
        if (FacilityInfo.length > 1) {
            entity['Nominal'] = PoliciesApplied[item]['Nominal'] ? RoundArray[FacilityInfo.length - 2][item]['Final approvable'] : 0;
        } else {
            entity['Nominal'] = PoliciesApplied[item]['Nominal'] ? Round1[item]['Final approvable'] : 0;
        }
        AmountApprovedPerCategory[item] = entity;
    }
    console.log(" Populate Amount Approved Per Category:" + JSON.stringify(AmountApprovedPerCategory));

    // Final Calculations of Stage 3 Calculator
    var FinalCalculations = {};
    var CheckFullRequestApprovable = true;
    var resultString = "";
    for (let i in FacilityInfo) {
        let Category = FacilityInfo[i]['CareCategory'];
        let entity = {};
        // I8
        entity['Requested Fee Increase'] = FacilityInfo[i]['RequestedFeeIncrease'];
        entity['FeeBeforeIncrease'] = FacilityInfo[i]['FeeBeforeIncrease'];
        //I9
        // entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'];
        if (InitialTotalAllowableStagePolicy == null && Initialstage2capammount == null) {
            entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'];
            entity['CummulativeAmount'] = entity['Max Approvable'].toFixed(2);
            entity['Full Request Approvable?'] = (entity['Requested Fee Increase'] <= entity['Max Approvable']) ? true : false;
            resultString = resultString + " " + FacilityInfo[i]['CareCategory'] + ": $" + entity['Max Approvable'].toFixed(2);
        }
        else {
            let TotalApprovable;
            // Updated Oct 19, 2023
            entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : parseFloat((InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] - InitialTotalAllowableStagePolicy[Category])).toFixed(2);
            //if ((newModifiedQCDecision == 100000001 || newModifiedQCDecision == 100000002) && TotalMonthlyExpenses == 0) {
            //    entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : 0;
            //}
            //else {
            //    // updated Oct 13,2023
            //    //entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] - InitialTotalAllowableStagePolicy[Category];
            //    entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] - InitialTotalAllowableStagePolicy[Category] >= 0 ? InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] - InitialTotalAllowableStagePolicy[Category] : 0;
            //}

            entity['CummulativeAmount'] = entity['Max Approvable'] + parseFloat(InitialTotalAllowableStagePolicy[Category].toFixed(2));
            TotalApprovable = (TotalMonthlyExpenses == 0 && (newModifiedQCDecision == 100000001 || newModifiedQCDecision == 100000002)) ? 0 : entity['CummulativeAmount'] <= parseFloat(Initialstage2capammount[Category].toFixed(2)) ? InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] - InitialTotalAllowableStagePolicy[Category] >= 0 ? InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] - InitialTotalAllowableStagePolicy[Category] : 0 : (parseFloat(Initialstage2capammount[Category].toFixed(2)) - parseFloat(InitialTotalAllowableStagePolicy[Category].toFixed(2)));
            entity['Full Request Approvable?'] = (entity['Requested Fee Increase'] <= TotalApprovable) ? true : false;
            resultString = resultString + " " + FacilityInfo[i]['CareCategory'] + ": $" + parseFloat(TotalApprovable).toFixed(2);
        }

        CheckFullRequestApprovable = CheckFullRequestApprovable && entity['Full Request Approvable?'];
        FinalCalculations[FacilityInfo[i]['CareCategory']] = entity;
    }
    console.log("resultString:" + resultString);
    console.log("Final Calculations:" + JSON.stringify(FinalCalculations));
    // A125 populate Dilution Cap or 70th percentile
    var DilutionCapor70thpercentile = {};
    var MEFIcappedcategories = " ";
    var string70cappedcategories = " ";
    for (let i in FacilityInfo) {
        let entity = {};
        // 127 
        entity['Dilution Cap'] = ((InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] < InitalCalculation[FacilityInfo[i]['CareCategory']]['Request'])
            && (InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'] === InitalCalculation[FacilityInfo[i]['CareCategory']]['Dilution Cap'])) ? true : false;
        //128 
        entity['70th percentile'] = entity['Dilution Cap'] ? ((Limitfeesto70Percentile && (NMFIncreaseCap[FacilityInfo[i]['CareCategory']]['Cap'] === InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'])) ? true : false) : false;
        // 133  populuate  MEFI capped categories: string
        MEFIcappedcategories = MEFIcappedcategories + " " + (FacilityInfo[i]['RequestedFeeIncrease'] > MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per10')] ? FacilityInfo[i]['CareCategory'] : " ");
        // 134 70 capped categories:
        string70cappedcategories = string70cappedcategories + (entity['70th percentile'] ? FacilityInfo[i]['CareCategory'] : " ");
        DilutionCapor70thpercentile[FacilityInfo[i]['CareCategory']] = entity;

    }
    console.log("MEFIcappedcategories:" + MEFIcappedcategories);
    console.log("70 capped categories:" + string70cappedcategories);
    console.log("DilutionCapor70thpercentile:" + JSON.stringify(DilutionCapor70thpercentile));

    // A118 Adjudicator Note: last result //70th % CAPPED => NMF Benchmark CAPPED based on ticket 1135
    var AdjudicatorNote = "S3CV: " + (CalculatorValidation ? "Pass;" : "Fail;") + " RESULT: " + ((!CheckFullRequestApprovable) ? "Not Approvable" : "Approvable") + "; MAX APPROVABLE: "
        + resultString + stringPOLICIESUSED + " SDA: " + SDA + "; PERCENTILE CAP: " + (Limitfeesto70Percentile ? "Yes" : "No") + "; UNUSED EXPENSES: $" + (TotalMonthlyExpenses - TotalAllowedExpenses).toFixed(2) + "\n"
        + "MEFI CAPPED: " + ((MEFIcappedcategories.trim() == "") ? "None" : MEFIcappedcategories) + "\n" + "NMF Benchmark CAPPED: " + ((string70cappedcategories.trim() == "") ? "None" : string70cappedcategories);
    console.log("AdjudicatorNote:" + AdjudicatorNote);

    var returnValue = {};
    returnValue['FinalCalculations'] = FinalCalculations;

    returnValue['AdjudicatorNote'] = AdjudicatorNote;  // as AdjudicatorNote include special chars. it will fail when function return.
    returnValue['AmountApprovedPerCategory'] = AmountApprovedPerCategory;
    // add return value UNUSED EXPENSES 20230823

    returnValue['MTFI:Unused Expenses'] = (TotalMonthlyExpenses - TotalAllowedExpenses);



    return returnValue;
}

function UpdateEntityRecord(entityname, entityId, data) {
    Xrm.WebApi.updateRecord(entityname, entityId, data).then(
        function success(result) {
            console.log("updated the record");

        },
        function (error) {
            console.log(error.message);
        }
    );
}


function PopulateSummaryApprovedAmount(returnValue, FacilityAmountAllowedRecords, entityname, FeeIncreaseDetails, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark, InitialTotalAllowableStagePolicy, Initialstage2capammount, newModifiedQCDecision) {
    // this is for MTFI
    debugger;
    var TotalAllowableStagePolicy = {};
    // To prepopulate the records in summary record.
    if (FacilityAmountAllowedRecords != null) {
        var TotalAllowableStagePolicyId;
        let arrayLength = FacilityAmountAllowedRecords.length;
        for (let i = 0; i < arrayLength; i++) {

            console.log(JSON.stringify(FacilityAmountAllowedRecords[i]));

            //Exceptional Circumstances
            if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Exceptional Circumstances") {

                var ExceptionalCircumstancesStagePolicy = {

                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Exceptional Circumstances']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Exceptional Circumstances']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Exceptional Circumstances']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Exceptional Circumstances']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Exceptional Circumstances']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Exceptional Circumstances']) : 0

                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amount_mtfiid'];
                UpdateEntityRecord(entityname, Id, ExceptionalCircumstancesStagePolicy)



            }
            //Direct care staff wages
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Direct Care Staff Wages") {
                var DirectCareStaffWagesStagePolicy = {
                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Direct Care Staff Wages']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Direct Care Staff Wages']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Direct Care Staff Wages']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Direct Care Staff Wages']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Direct Care Staff Wages']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Direct Care Staff Wages']) : 0

                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amount_mtfiid'];
                UpdateEntityRecord(entityname, Id, DirectCareStaffWagesStagePolicy);


            }

            //Priority Service Expansion
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Priority Service Expansion") {
                var PriorityServiceExpansionStagePolicy = {
                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Priority SE(Extended Hours)']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Priority SE(Extended Hours)']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Priority SE(Extended Hours)']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Priority SE(Extended Hours)']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Priority SE(Extended Hours)']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Priority SE(Extended Hours)']) : 0
                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amount_mtfiid'];
                UpdateEntityRecord(entityname, Id, PriorityServiceExpansionStagePolicy);


            }
            //ACCUP
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "ACCUP") {
                accup_0to18months = FacilityAmountAllowedRecords[i]['ccof_0to18months'];
                accup_18to36months = FacilityAmountAllowedRecords[i]['ccof_18to36months'];
                accup_preschol = FacilityAmountAllowedRecords[i]['ccof_preschool'];
                accup_outofschoolkindergarden = FacilityAmountAllowedRecords[i]['ccof_outofschoolcarekindergarten'];
                accup_3yearstokindergarden = FacilityAmountAllowedRecords[i]['ccof_3yearstokindergarten'];
                accup_outofschoolcaregrade1 = FacilityAmountAllowedRecords[i]['ccof_outofschoolcaregrade1'];

            }
            //Stage 2 - Fee Increase Limit (FIL)
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Stage 2 - Fee Increase Limit (FIL)") {
                stage2_0to18months = FacilityAmountAllowedRecords[i]['ccof_0to18months'];
                stage2_18to36months = FacilityAmountAllowedRecords[i]['ccof_18to36months'];
                stage2_preschol = FacilityAmountAllowedRecords[i]['ccof_preschool'];
                stage2_outofschoolkindergarden = FacilityAmountAllowedRecords[i]['ccof_outofschoolcarekindergarten'];
                stage2_3yearstokindergarden = FacilityAmountAllowedRecords[i]['ccof_3yearstokindergarten'];
                stage2_outofschoolcaregrade1 = FacilityAmountAllowedRecords[i]['ccof_outofschoolcaregrade1'];
            }
            //Used Carried Forward Amounts / or MTFI:Unused Expenses or MTFI Unused in Approved
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Unused Carried Forward Amounts") {
                var UsedCarriedForwardAmounts = {
                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['MTFI Unused']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['MTFI Unused']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['MTFI Unused']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['MTFI Unused']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['MTFI Unused']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['MTFI Unused']) : 0
                }

                Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amount_mtfiid'];
                UpdateEntityRecord(entityname, Id, UsedCarriedForwardAmounts);
            }
            //Total MTFI Allowable Amount
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Total MTFI Allowable Amount") {
                TotalAllowableStagePolicy = {
                    "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : 0,
                    "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : 0,
                    "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : 0
                }

                TotalAllowableStagePolicyId = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amount_mtfiid'];
                UpdateEntityRecord(entityname, TotalAllowableStagePolicyId, TotalAllowableStagePolicy);
            }

        }
        IndicateCap(FeeIncreaseDetails, TotalAllowableStagePolicy, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyId, InitialTotalAllowableStagePolicy, Initialstage2capammount, returnValue);
    }

    return TotalAllowableStagePolicy;
}

function PopulateCCFRIFacilitySummaryApprovedAmount(returnValue, FacilityAmountAllowedRecords, entityname, FeeIncreaseDetails, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark) {
    debugger;
    var TotalAllowableStagePolicy = {};
    // To prepopulate the records in summary record.
    if (FacilityAmountAllowedRecords != null) {
        var TotalAllowableStagePolicyId;
        let arrayLength = FacilityAmountAllowedRecords.length;
        for (let i = 0; i < arrayLength; i++) {

            console.log(JSON.stringify(FacilityAmountAllowedRecords[i]));

            //Exceptional Circumstances
            if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Exceptional Circumstances") {

                var ExceptionalCircumstancesStagePolicy = {

                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Exceptional Circumstances']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Exceptional Circumstances']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Exceptional Circumstances']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Exceptional Circumstances']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Exceptional Circumstances']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Exceptional Circumstances']) : 0

                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amountid'];
                UpdateEntityRecord(entityname, Id, ExceptionalCircumstancesStagePolicy)



            }
            //Direct care staff wages
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Direct Care Staff Wages") {
                var DirectCareStaffWagesStagePolicy = {
                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Direct Care Staff Wages']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Direct Care Staff Wages']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Direct Care Staff Wages']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Direct Care Staff Wages']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Direct Care Staff Wages']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Direct Care Staff Wages']) : 0

                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amountid'];
                UpdateEntityRecord(entityname, Id, DirectCareStaffWagesStagePolicy);


            }

            //Priority Service Expansion
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Priority Service Expansion") {
                var PriorityServiceExpansionStagePolicy = {
                    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Priority SE(Extended Hours)']) : 0,
                    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Priority SE(Extended Hours)']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Priority SE(Extended Hours)']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Priority SE(Extended Hours)']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Priority SE(Extended Hours)']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Priority SE(Extended Hours)']) : 0
                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amountid'];
                UpdateEntityRecord(entityname, Id, PriorityServiceExpansionStagePolicy);


            }
            //ACCUP
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "ACCUP") {
                accup_0to18months = FacilityAmountAllowedRecords[i]['ccof_0to18months'];
                accup_18to36months = FacilityAmountAllowedRecords[i]['ccof_18to36months'];
                accup_preschol = FacilityAmountAllowedRecords[i]['ccof_preschool'];
                accup_outofschoolkindergarden = FacilityAmountAllowedRecords[i]['ccof_outofschoolcarekindergarten'];
                accup_3yearstokindergarden = FacilityAmountAllowedRecords[i]['ccof_3yearstokindergarten'];
                accup_outofschoolcaregrade1 = FacilityAmountAllowedRecords[i]['ccof_outofschoolcaregrade1'];

            }
            //Stage 2 - Fee Increase Limit (FIL)
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Stage 2 - Fee Increase Limit (FIL)") {
                stage2_0to18months = FacilityAmountAllowedRecords[i]['ccof_0to18months'];
                stage2_18to36months = FacilityAmountAllowedRecords[i]['ccof_18to36months'];
                stage2_preschol = FacilityAmountAllowedRecords[i]['ccof_preschool'];
                stage2_outofschoolkindergarden = FacilityAmountAllowedRecords[i]['ccof_outofschoolcarekindergarten'];
                stage2_3yearstokindergarden = FacilityAmountAllowedRecords[i]['ccof_3yearstokindergarten'];
                stage2_outofschoolcaregrade1 = FacilityAmountAllowedRecords[i]['ccof_outofschoolcaregrade1'];
            }
            //Total Allowable Fee Increase - need to total
            // Considering unused amount from Initial that is Total allowable on MTFI is MTFI max approvable + initial max approvable
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Total Allowable Fee Increase") {
                TotalAllowableStagePolicy = {
                    "ccof_to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : 0,
                    "ccof_to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : 0,
                    "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : 0,
                    "ccof__3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : 0
                }




                TotalAllowableStagePolicyId = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amountid'];
                UpdateEntityRecord(entityname, TotalAllowableStagePolicyId, TotalAllowableStagePolicy);
            }
        }
        IndicateCapCCFRIFacility(FeeIncreaseDetails, TotalAllowableStagePolicy, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyId);
    }

    return TotalAllowableStagePolicy;
}
function IndicateCap(feeIncreaseDetails, TotalAllowableStagePolicy, regionInfo, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyID, InitialTotalAllowableStagePolicy, Initialstage2capammount, returnValue) {
    // set the indicator as pass or fail.
    var NMFCAPReached = false;
    var MEFICAPReached = false;
    var entityname = "ccof_change_request_mtfi";
    var ccfri_facility_allowable_amountEntityName = "ccof_ccfri_facility_allowable_amount_mtfi";
    var AllowableFeeIncreaseEntityName = "";
    var ReachedCap;
    var TotalAllowableStagePolicywithMEFICAP = {};
    var MediansFee = // 10 MEFI from Median table based on SDA,Org,OrgType and Program year.  3%  need to confirm. 
    {
        "0-18": regionInfo['ccof_Region3PctMedian']['ccof_0to18months'],
        "18-36": regionInfo['ccof_Region3PctMedian']['ccof_18to36months'],
        "3Y-K": regionInfo['ccof_Region3PctMedian']['ccof_3yearstokindergarten'],
        "OOSC-K": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcarekindergarten'],
        "OOSC-G": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcaregrade1'],
        "PRE": regionInfo['ccof_Region3PctMedian']['ccof_preschool'],
        "0-18_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof0to18'],
        "18-36_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof18to36'],
        "3Y-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof3ytok'],
        "OOSC-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctok'],
        "OOSC-G_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctog'],
        "PRE_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofpre'],
        "0-18_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof0to18'],
        "18-36_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof18to36'],
        "3Y-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof3ytok'],
        "OOSC-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctok'],
        "OOSC-G_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctog'],
        "PRE_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percenatgeofpre']
    }
    var SDA70thPercentileF = // 70th Percentile for SDA // comes from CRM NMF Benchmarks table based on SDA, Org,OrgType and Program year
    {
        "0-18": regionInfo['ccof_RegionNMFBenchmark']['ccof_0to18m'],
        "18-36": regionInfo['ccof_RegionNMFBenchmark']['ccof_18to36m'],
        "3Y-K": regionInfo['ccof_RegionNMFBenchmark']['ccof_3ytok'],
        "OOSC-K": regionInfo['ccof_RegionNMFBenchmark']['ccof_oosctok'],
        "OOSC-G": regionInfo['ccof_RegionNMFBenchmark']['ccof_oosctograde'],
        "PRE": regionInfo['ccof_RegionNMFBenchmark']['ccof_preschool'],
    }
    if (TotalAllowableStagePolicy != null) {

        if (mefiCAP == true) {
            //Compare MEFI(10% median) wit total approved amount
            if (MEFICAPReached == false) {
                (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? MEFICAPReached = parseFloat((returnValue['FinalCalculations']['0-18']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) > MediansFee['0-18_Per10'] ? true : false : false;
            }

            if (MEFICAPReached == false) { MEFICAPReached = (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? parseFloat((returnValue['FinalCalculations']['18-36']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) > MediansFee['18-36_Per10'] ? true : false : false; }

            if (MEFICAPReached == false) { MEFICAPReached = (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? parseFloat((returnValue['FinalCalculations']['PRE']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) > MediansFee['PRE_Per10'] ? true : false : false; }

            if (MEFICAPReached == false) { MEFICAPReached = (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? parseFloat((returnValue['FinalCalculations']['OOSC-K']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) > MediansFee['OOSC-K_Per10'] ? true : false : false; }

            if (MEFICAPReached == false) { MEFICAPReached = (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? parseFloat((returnValue['FinalCalculations']['3Y-K']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) > MediansFee['3Y-K_Per10'] ? true : false : false; }

            if (MEFICAPReached == false) { MEFICAPReached = (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? parseFloat((returnValue['FinalCalculations']['OOSC-G']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) > MediansFee['OOSC-G_Per10'] ? true : false : false; }

        }



        //compareNMF( NMF - Fee Before increase) with total approved amount
        if (limitfeestonmfbenchmark == true) {
            for (const i in feeIncreaseDetails) {
                if (NMFCAPReached == false) {

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "0-18") { (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? NMFCAPReached = (parseFloat(returnValue['FinalCalculations']['0-18']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) > (SDA70thPercentileF['0-18'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false : false; }
                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "18-36") { (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? NMFCAPReached = (parseFloat(returnValue['FinalCalculations']['18-36']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) > (SDA70thPercentileF['18-36'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false : false; }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "3Y-K") { (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? NMFCAPReached = (parseFloat(returnValue['FinalCalculations']['3Y-K']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) > (SDA70thPercentileF['3Y-K'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false : false; }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "OOSC-K") {
                        (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? NMFCAPReached = (parseFloat(returnValue['FinalCalculations']['OOSC-K']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) > (SDA70thPercentileF['OOSC-K'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false : false;
                    }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "OOSC-G']") {
                        (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? NMFCAPReached = (parseFloat(returnValue['FinalCalculations']['OOSC-G']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) > (SDA70thPercentileF['OOSC-G'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false : false;
                    }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "PRE") {
                        (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? NMFCAPReached = (parseFloat(returnValue['FinalCalculations']['PRE']['Requested Fee Increase']) + parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) > (SDA70thPercentileF['PRE'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false : false;
                    }
                }


            }

        }


        console.log("MEFI CAP true or false?" + MEFICAPReached);
        console.log("NMFCAPReached  true or false?" + NMFCAPReached);
        if (MEFICAPReached == true && NMFCAPReached == true) {
            ReachedCap = {
                "ccof_capsindicator": 100000002

            }
            TotalAllowableStagePolicywithMEFICAP = {

                "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['FinalCalculations']['0-18']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['0-18'])) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : (parseFloat(Initialstage2capammount['0-18']) - parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) : 0,
                "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['FinalCalculations']['18-36']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['18-36'])) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : (parseFloat(Initialstage2capammount['18-36']) - parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) : 0,
                "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['FinalCalculations']['PRE']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['PRE'])) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : (parseFloat(Initialstage2capammount['PRE']) - parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) : 0,
                "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-K']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['OOSC-K'])) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : (parseFloat(Initialstage2capammount['OOSC-K']) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) : 0,
                "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['FinalCalculations']['3Y-K']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['3Y-K'])) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : (parseFloat(Initialstage2capammount['3Y-K']) - parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) : 0,
                "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-G']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['OOSC-G'])) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : (parseFloat(Initialstage2capammount['OOSC-G']) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) : 0
            }

            //TotalAllowableStagePolicywithNMFCAP = {

            //    "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['FinalCalculations']['0-18']['CummulativeAmount']) <= (SDA70thPercentileF['0-18'] - parseFloat(returnValue['FinalCalculations']['0-18']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : ((SDA70thPercentileF['0-18'] - parseFloat(returnValue['FinalCalculations']['0-18']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) : 0,
            //    "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['FinalCalculations']['18-36']['CummulativeAmount']) <= (SDA70thPercentileF['18-36'] - parseFloat(returnValue['FinalCalculations']['18-36']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : ((SDA70thPercentileF['18-36'] - parseFloat(returnValue['FinalCalculations']['18-36']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) : 0,
            //    "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['FinalCalculations']['PRE']['CummulativeAmount']) <= (SDA70thPercentileF['PRE'] - parseFloat(returnValue['FinalCalculations']['PRE']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : ((SDA70thPercentileF['PRE'] - parseFloat(returnValue['FinalCalculations']['PRE']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) : 0,
            //    "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-K']['CummulativeAmount']) <= (SDA70thPercentileF['OOSC-K'] - parseFloat(returnValue['FinalCalculations']['OOSC-K']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : ((SDA70thPercentileF['OOSC-K'] - parseFloat(returnValue['FinalCalculations']['OOSC-K']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) : 0,
            //    "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['FinalCalculations']['3Y-K']['CummulativeAmount']) <= (SDA70thPercentileF['3Y-K'] - parseFloat(returnValue['FinalCalculations']['3Y-K']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : ((SDA70thPercentileF['3Y-K'] - parseFloat(returnValue['FinalCalculations']['3Y-K']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) : 0,
            //    "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-G']['CummulativeAmount']) <= (SDA70thPercentileF['OOSC-G'] - parseFloat(returnValue['FinalCalculations']['OOSC-G']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : ((SDA70thPercentileF['OOSC-G'] - parseFloat(returnValue['FinalCalculations']['OOSC-G']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) : 0
            //}

            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amountEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);
        }
        else if (MEFICAPReached == true && NMFCAPReached == false) {
            ReachedCap = {
                "ccof_capsindicator": 100000000

            }
            TotalAllowableStagePolicywithMEFICAP = {

                "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['FinalCalculations']['0-18']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['0-18'])) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : (parseFloat(Initialstage2capammount['0-18']) - parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) : 0,
                "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['FinalCalculations']['18-36']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['18-36'])) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : (parseFloat(Initialstage2capammount['18-36']) - parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) : 0,
                "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['FinalCalculations']['PRE']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['PRE'])) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : (parseFloat(Initialstage2capammount['PRE']) - parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) : 0,
                "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-K']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['OOSC-K'])) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : (parseFloat(Initialstage2capammount['OOSC-K']) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) : 0,
                "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['FinalCalculations']['3Y-K']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['3Y-K'])) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : (parseFloat(Initialstage2capammount['3Y-K']) - parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) : 0,
                "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-G']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['OOSC-G'])) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : (parseFloat(Initialstage2capammount['OOSC-G']) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) : 0
            }

            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amountEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);

        }
        else if (MEFICAPReached == false && NMFCAPReached == true) {
            ReachedCap = {
                "ccof_capsindicator": 100000001

            }
            TotalAllowableStagePolicywithMEFICAP = {

                "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['FinalCalculations']['0-18']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['0-18'])) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : (parseFloat(Initialstage2capammount['0-18']) - parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) : 0,
                "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['FinalCalculations']['18-36']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['18-36'])) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : (parseFloat(Initialstage2capammount['18-36']) - parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) : 0,
                "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['FinalCalculations']['PRE']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['PRE'])) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : (parseFloat(Initialstage2capammount['PRE']) - parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) : 0,
                "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-K']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['OOSC-K'])) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : (parseFloat(Initialstage2capammount['OOSC-K']) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) : 0,
                "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['FinalCalculations']['3Y-K']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['3Y-K'])) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : (parseFloat(Initialstage2capammount['3Y-K']) - parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) : 0,
                "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-G']['CummulativeAmount']) <= parseFloat(Initialstage2capammount['OOSC-G'])) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : (parseFloat(Initialstage2capammount['OOSC-G']) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) : 0
            }

            //TotalAllowableStagePolicywithNMFCAP = {

            //    "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['FinalCalculations']['0-18']['CummulativeAmount']) <= (SDA70thPercentileF['0-18'] - parseFloat(returnValue['FinalCalculations']['0-18']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : ((SDA70thPercentileF['0-18'] - parseFloat(returnValue['FinalCalculations']['0-18']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['0-18'].toFixed(2))) : 0,
            //    "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['FinalCalculations']['18-36']['CummulativeAmount']) <= (SDA70thPercentileF['18-36'] - parseFloat(returnValue['FinalCalculations']['18-36']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : ((SDA70thPercentileF['18-36'] - parseFloat(returnValue['FinalCalculations']['18-36']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['18-36'].toFixed(2))) : 0,
            //    "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['FinalCalculations']['PRE']['CummulativeAmount']) <= (SDA70thPercentileF['PRE'] - parseFloat(returnValue['FinalCalculations']['PRE']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : ((SDA70thPercentileF['PRE'] - parseFloat(returnValue['FinalCalculations']['PRE']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['PRE'].toFixed(2))) : 0,
            //    "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-K']['CummulativeAmount']) <= (SDA70thPercentileF['OOSC-K'] - parseFloat(returnValue['FinalCalculations']['OOSC-K']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : ((SDA70thPercentileF['OOSC-K'] - parseFloat(returnValue['FinalCalculations']['OOSC-K']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-K'].toFixed(2))) : 0,
            //    "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['FinalCalculations']['3Y-K']['CummulativeAmount']) <= (SDA70thPercentileF['3Y-K'] - parseFloat(returnValue['FinalCalculations']['3Y-K']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : ((SDA70thPercentileF['3Y-K'] - parseFloat(returnValue['FinalCalculations']['3Y-K']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['3Y-K'].toFixed(2))) : 0,
            //    "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['FinalCalculations']['OOSC-G']['CummulativeAmount']) <= (SDA70thPercentileF['OOSC-G'] - parseFloat(returnValue['FinalCalculations']['OOSC-G']['FeeBeforeIncrease']))) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : ((SDA70thPercentileF['OOSC-G'] - parseFloat(returnValue['FinalCalculations']['OOSC-G']['FeeBeforeIncrease'])) - parseFloat(InitialTotalAllowableStagePolicy['OOSC-G'].toFixed(2))) : 0
            //}
            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amountEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);
        }
        // ticket CCFRI-2126 fix. Jun 30, 2023
        else if (MEFICAPReached === false && NMFCAPReached === false) {
            ReachedCap = {
                "ccof_capsindicator": null
            }
            UpdateEntityRecord(entityname, entityId, ReachedCap)
        }

    }

}
function IndicateCapCCFRIFacility(feeIncreaseDetails, TotalAllowableStagePolicy, regionInfo, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyID) {
    // set the indicator as pass or fail.
    var NMFCAPReached = false;
    var MEFICAPReached = false;
    var entityname = "ccof_adjudication_ccfri_facility";
    var ccfri_facility_allowable_amountEntityName = "ccof_ccfri_facility_allowable_amount";
    var AllowableFeeIncreaseEntityName = "";
    var ReachedCap;
    var TotalAllowableStagePolicywithMEFICAP = {};
    var MediansFee = // 10 MEFI from Median table based on SDA,Org,OrgType and Program year.  3%  need to confirm. 
    {
        "0-18": regionInfo['ccof_Region3PctMedian']['ccof_0to18months'],
        "18-36": regionInfo['ccof_Region3PctMedian']['ccof_18to36months'],
        "3Y-K": regionInfo['ccof_Region3PctMedian']['ccof_3yearstokindergarten'],
        "OOSC-K": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcarekindergarten'],
        "OOSC-G": regionInfo['ccof_Region3PctMedian']['ccof_outofschoolcaregrade1'],
        "PRE": regionInfo['ccof_Region3PctMedian']['ccof_preschool'],
        "0-18_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof0to18'],
        "18-36_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof18to36'],
        "3Y-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageof3ytok'],
        "OOSC-K_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctok'],
        "OOSC-G_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofoosctog'],
        "PRE_Per3": regionInfo['ccof_Region3PctMedian']['ccof_3percentageofpre'],
        "0-18_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof0to18'],
        "18-36_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof18to36'],
        "3Y-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageof3ytok'],
        "OOSC-K_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctok'],
        "OOSC-G_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percentageofoosctog'],
        "PRE_Per10": regionInfo['ccof_Region3PctMedian']['ccof_10percenatgeofpre']
    }
    var SDA70thPercentileF = // 70th Percentile for SDA // comes from CRM NMF Benchmarks table based on SDA, Org,OrgType and Program year
    {
        "0-18": regionInfo['ccof_RegionNMFBenchmark']['ccof_0to18m'],
        "18-36": regionInfo['ccof_RegionNMFBenchmark']['ccof_18to36m'],
        "3Y-K": regionInfo['ccof_RegionNMFBenchmark']['ccof_3ytok'],
        "OOSC-K": regionInfo['ccof_RegionNMFBenchmark']['ccof_oosctok'],
        "OOSC-G": regionInfo['ccof_RegionNMFBenchmark']['ccof_oosctograde'],
        "PRE": regionInfo['ccof_RegionNMFBenchmark']['ccof_preschool'],
    }
    if (TotalAllowableStagePolicy != null) {

        if (mefiCAP == true) {
            //Compare MEFI(10% median) wit total approved amount
            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_0to18months'] > MediansFee['0-18_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_18to36months'] > MediansFee['18-36_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_preschool'] > MediansFee['PRE_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_outofschoolcarekindergarten'] > MediansFee['OOSC-K_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_3yearstokindergarten'] > MediansFee['3Y-K_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_outofschoolcaregrade1'] > MediansFee['OOSC-G_Per10'] ? true : false; }

        }
        //compareNMF( NMF - Fee Before increase) with total approved amount
        if (limitfeestonmfbenchmark == true) {
            for (const i in feeIncreaseDetails) {
                if (NMFCAPReached == false) {
                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "0-18") { NMFCAPReached = TotalAllowableStagePolicy['ccof_0to18months'] > (SDA70thPercentileF['0-18'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false; }
                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "18-36") { NMFCAPReached = TotalAllowableStagePolicy['ccof_18to36months'] > (SDA70thPercentileF['18-36'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false; }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "3Y-K") { NMFCAPReached = TotalAllowableStagePolicy['ccof_3yearstokindergarten'] > (SDA70thPercentileF['3Y-K'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false; }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "OOSC-K") {
                        NMFCAPReached = TotalAllowableStagePolicy['ccof_outofschoolcarekindergarten'] > (SDA70thPercentileF['OOSC-K'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false;
                    }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "OOSC-G']") {
                        NMFCAPReached = TotalAllowableStagePolicy['ccof_outofschoolcaregrade1'] > (SDA70thPercentileF['OOSC-G'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false;
                    }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "PRE") {
                        NMFCAPReached = TotalAllowableStagePolicy['ccof_preschool'] > (SDA70thPercentileF['PRE'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false;
                    }
                }

            }

        }

        console.log("MEFI CAP true or false?" + MEFICAPReached);
        console.log("NMFCAPReached  true or false?" + NMFCAPReached);
        if (MEFICAPReached == true && NMFCAPReached == true) {
            ReachedCap = {
                "ccof_capsindicator": 100000002
            }
            TotalAllowableStagePolicywithMEFICAP = {
                "ccof_0to18months": MediansFee['0-18_Per10'],
                "ccof_18to36months": MediansFee['18-36_Per10'],
                "ccof_preschool": MediansFee['PRE_Per10'],
                "ccof_outofschoolcarekindergarten": MediansFee['OOSC-K_Per10'],
                "ccof_3yearstokindergarten": MediansFee['3Y-K_Per10'],
                "ccof_outofschoolcaregrade1": MediansFee['OOSC-G_Per10']
            }

            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amountEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);
        }
        else if (MEFICAPReached == true && NMFCAPReached == false) {
            ReachedCap = {
                "ccof_capsindicator": 100000000
            }
            TotalAllowableStagePolicywithMEFICAP = {
                "ccof_0to18months": MediansFee['0-18_Per10'],
                "ccof_18to36months": MediansFee['18-36_Per10'],
                "ccof_preschool": MediansFee['PRE_Per10'],
                "ccof_outofschoolcarekindergarten": MediansFee['OOSC-K_Per10'],
                "ccof_3yearstokindergarten": MediansFee['3Y-K_Per10'],
                "ccof_outofschoolcaregrade1": MediansFee['OOSC-G_Per10']
            }

            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amountEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);

        }
        else if (MEFICAPReached == false && NMFCAPReached == true) {
            ReachedCap = {
                "ccof_capsindicator": 100000001
            }
            UpdateEntityRecord(entityname, entityId, ReachedCap)
        }
        // ticket CCFRI-2126 fix. Jun 30, 2023
        else if (MEFICAPReached === false && NMFCAPReached === false) {
            ReachedCap = {
                "ccof_capsindicator": null
            }
            UpdateEntityRecord(entityname, entityId, ReachedCap)
        }
    }

}

function convertToPlain(html) {

    // Create a new div element
    var tempDivElement = document.createElement("div");

    // Set the HTML content with the given value
    tempDivElement.innerHTML = html;

    // Retrieve the text property of the element 
    return tempDivElement.textContent || tempDivElement.innerText || "";
}