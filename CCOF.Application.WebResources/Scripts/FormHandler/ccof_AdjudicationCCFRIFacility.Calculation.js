var CCOF = CCOF || {};
CCOF.AdjudicationCCFRIFacility = CCOF.AdjudicationCCFRIFacility || {};
CCOF.AdjudicationCCFRIFacility.Calculation = CCOF.AdjudicationCCFRIFacility.Calculation || {};
CCOF.AdjudicationCCFRIFacility.Calculation = {
    Calculation: function (primaryControl) {
        try {
            debugger;
            console.log("Initial Adjudication Calculattion....");
            var formContext = primaryControl;
            var ccfri_facility_allowable_amountEntityName = "ccof_ccfri_facility_allowable_amount";
            var ccfri_facility_allowable_amount_24MonthEntityName = "ccof_ccfrifacilityallowableamount_24month";
            var entityId = formContext.data.entity.getId(); // get parent record id
            var appCCFRI = formContext.getAttribute("ccof_applicationccfri").getValue()[0].id;
            // Facility Info; Region,Median, NMF, SDA , expense, MEFI Capâ€™, Limit Fees to NMF Benchmarkâ€™ ,orgType
            // Get expense Info
            var FeeIncreaseDetails = getSyncMultipleRecord("ccof_ccfri_facility_parent_fees?$select=_ccof_adjudicationccfrifacility_value,ccof_averageenrolment,_ccof_childcarecategory_value,ccof_cumulativefeeincrease,ccof_feebeforeincrease,ccof_feeincreasetype,ccof_name,_ccof_programyear_value&$filter=(_ccof_adjudicationccfrifacility_value eq " + getCleanedGuid(entityId) + " and statecode eq 0 and ccof_feebeforeincrease ne 'N/A')&$orderby=_ccof_childcarecategory_value asc");

            if (FeeIncreaseDetails.length > 0) {
                var ExpenseInfo = {};
                ExpenseInfo['Exceptional Circumstances'] = formContext.getAttribute("ccof_totalexpenses_exceptionalcircumstances").getValue();
                ExpenseInfo['Direct Care Staff Wages'] = formContext.getAttribute("ccof_totalexpenses_wageincrease").getValue();
                ExpenseInfo['Priority Service Expansion'] = formContext.getAttribute("ccof_totalexpenses_priorityserviceexpansion").getValue();
                ExpenseInfo['Total Monthly Expenses'] = ExpenseInfo['Exceptional Circumstances'] + ExpenseInfo['Direct Care Staff Wages'] + ExpenseInfo['Priority Service Expansion'];
                ExpenseInfo['MEFI Cap'] = formContext.getAttribute("ccof_meficap").getValue();
                ExpenseInfo['Limit Fees to NMF Benchmark'] = formContext.getAttribute("ccof_limitfeestonmfbenchmark").getValue();
                //ccof_meficap24month
                //ccof_meficap
                //ccof_limitfeestonmfbenchmark_24month
                // ccof_limitfeestonmfbenchmark
                var mefiCAP = formContext.getAttribute("ccof_meficap").getValue();
                var limitfeestonmfbenchmark = formContext.getAttribute("ccof_limitfeestonmfbenchmark").getValue();
                // Validate expense info removed validation 20230213
                //if (ExpenseInfo['Total Monthly Expenses'] === 0) {
                //    Xrm.Navigation.openAlertDialog("Initial Adjudiction Expense fees are required!");
                //    return;
                //}
                // Get Region, Median, NMF
                var RegionInfos = getSyncSingleRecord("ccof_applicationccfris(" + getCleanedGuid(appCCFRI) + ")?$select=ccof_applicationccfriid,_ccof_region_value&$expand=ccof_Application($select=ccof_applicationid,ccof_name,_ccof_programyear_value,ccof_providertype),ccof_Region3PctMedian($select=ccof_0to18months,ccof_10percentageof0to18,ccof_10percentageof18to36,ccof_10percentageof3ytok,ccof_10percentageofoosctog,ccof_10percentageofoosctok,ccof_10percenatgeofpre,ccof_18to36months,ccof_3percentageof0to18,ccof_3percentageof18to36,ccof_3percentageof3ytok,_ccof_3percentmedian_value,ccof_3percentageofoosctog,ccof_3percentageofoosctok,ccof_3percentageofpre,ccof_3yearstokindergarten,ccof_name,ccof_outofschoolcaregrade1,ccof_outofschoolcarekindergarten,ccof_preschool),ccof_RegionNMFBenchmark($select=ccof_fee_benchmark_sdaid,ccof_0to18m,ccof_18to36m,ccof_3ytok,ccof_name,ccof_oosctograde,ccof_oosctok,ccof_preschool)");
                //  Validate FeeIncrease info
                var ifCalculateInitial = true;
                if (FeeIncreaseDetails.length === 0) {
                    //Xrm.Navigation.openAlertDialog("Initial Fee Increase is required! Will not continue calculate Initial");
                    ifCalculateInitial = false;
                    //  return;
                } else {
                    for (let i in FeeIncreaseDetails) {
                        if (FeeIncreaseDetails[i]['ccof_averageenrolment'] === null || FeeIncreaseDetails[i]['ccof_cumulativefeeincrease'] === null || FeeIncreaseDetails[i]['ccof_feebeforeincrease'] === null) {
                            //Xrm.Navigation.openAlertDialog("One or more Initial Fee Increase Amount are missing! Will not continue calculate Initial tab");
                            ifCalculateInitial = false;
                            // return;
                        }
                    }

                }
                if (ifCalculateInitial) {
                    var FacilityAmountAllowedRecords = getSyncMultipleRecord("ccof_ccfri_facility_allowable_amounts?$select=ccof_3yearstokindergarten,ccof_outofschoolcarekindergarten,ccof_preschool,ccof_18to36months,ccof_0to18months,ccof_outofschoolcaregrade1,ccof_stage3policy,ccof_displayorder&$filter=(_ccof_ccfrifacility_value eq " + entityId + ")&$top=50&$orderby=ccof_displayorder asc");
                    console.log("FeeIncreaseDetails" + JSON.stringify(FacilityAmountAllowedRecords));
                    var returnValue = Calculator(RegionInfos, FeeIncreaseDetails, ExpenseInfo);
                    //need to pass values of Initial adjudication
                    var TotalAllowableStagePolicy = PopulateSummaryApprovedAmount(returnValue, FacilityAmountAllowedRecords, ccfri_facility_allowable_amountEntityName, FeeIncreaseDetails, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark);
                    //IndicateCap(FeeIncreaseDetails, TotalAllowableStagePolicy, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark);
                    //let currentDate = new Date();
                    //let historyValue = formContext.getAttribute("ccof_adjudicatornotes").getValue();
                    //let plainValue = convertToPlain(historyValue);
                    //formContext.getAttribute("ccof_adjudicatornotes").setValue(plainValue+'\n'+returnValue['AdjudicatorNote']+'\n');
                    formContext.getAttribute("ccof_stage3calculatornotesinitial").setValue(returnValue['AdjudicatorNote']);
                    console.log("End Initial Adjudication Calculation");
                    //refresh the summary grid
                    formContext.getControl("AllowableAmount").refresh();
                    // Xrm.Navigation.openAlertDialog(" Initial Adjudication Completed Calculation!");
                }

            }
            // Xrm.Navigation.openAlertDialog(JSON.stringify(returnValue));
            // Check if it has 24 Months // Update logic based on ticket 1135. will check if Fee Increase in 24 Months have records
            //let appCCFRIReq = "ccof_applicationccfris(" + getCleanedGuid(appCCFRI) + ")?$select=ccof_feecorrectccfri";
            //let appCCFRIResponse = getSyncSingleRecord(appCCFRIReq);
            // 100000000  Yes 100000001 No
            var FeeIncreaseDetails24Months = getSyncMultipleRecord("ccof_ccfrifacilityfeeincrease24months?$select=_ccof_adjudicationccfrifacility_value, ccof_averageenrolment, _ccof_childcarecategory_value, ccof_cumulativefeeincrease, ccof_feebeforeincrease, ccof_feeincreasetype, ccof_name, _ccof_programyear_value, ccof_reasonfor24monthsadj, ccof_cumulativefeeincreasepercent, _ccof_applicationccfrichildcarecategory_value&$filter=(_ccof_adjudicationccfrifacility_value eq " + getCleanedGuid(entityId) + " and statecode eq 0 and ccof_feebeforeincrease ne 'N/A')&$orderby=_ccof_childcarecategory_value asc");
            if (FeeIncreaseDetails24Months.length > 0) {
                //if (appCCFRIResponse["ccof_feecorrectccfri"] === 100000001) {  // 24 Months tab and Initial Adjudication tab
                // Facility Info; Region,Median, NMF, SDA , expense, MEFI Capâ€™, Limit Fees to NMF Benchmarkâ€™ ,orgType
                // Get expense Info
                console.log("Begin 24 Months Calculation");
                var ExpenseInfo24Months = {};
                ExpenseInfo24Months['Exceptional Circumstances'] = formContext.getAttribute("ccof_totalmonthlycostofexpensesec2_24month").getValue();
                ExpenseInfo24Months['Direct Care Staff Wages'] = formContext.getAttribute("ccof_totalexpenses_dcsw_24month").getValue();
                ExpenseInfo24Months['Priority Service Expansion'] = formContext.getAttribute("ccof_totalmonthlycostofexpensespriorityservice").getValue();
                ExpenseInfo24Months['Total Monthly Expenses'] = ExpenseInfo24Months['Exceptional Circumstances'] + ExpenseInfo24Months['Direct Care Staff Wages'] + ExpenseInfo24Months['Priority Service Expansion'];
                ExpenseInfo24Months['MEFI Cap'] = formContext.getAttribute("ccof_meficap24month").getValue();
                ExpenseInfo24Months['Limit Fees to NMF Benchmark'] = formContext.getAttribute("ccof_limitfeestonmfbenchmark_24month").getValue();
                // Validate expense info removed validation 20230213
                //if (ExpenseInfo24Months['Total Monthly Expenses'] === 0) {
                //    Xrm.Navigation.openAlertDialog("24 Month Expense fees  are required!");
                //    return;
                //}
                // Get Region, Median, NMF for  24 Months. it same as Initial Adjudication
                // get Fee Increase details
                var RegionInfos24Months = getSyncSingleRecord("ccof_applicationccfris(" + getCleanedGuid(appCCFRI) + ")?$select=ccof_applicationccfriid,_ccof_region_value&$expand=ccof_Application($select=ccof_applicationid,ccof_name,_ccof_programyear_value,ccof_providertype),ccof_Region3PctMedian($select=ccof_0to18months,ccof_10percentageof0to18,ccof_10percentageof18to36,ccof_10percentageof3ytok,ccof_10percentageofoosctog,ccof_10percentageofoosctok,ccof_10percenatgeofpre,ccof_18to36months,ccof_3percentageof0to18,ccof_3percentageof18to36,ccof_3percentageof3ytok,_ccof_3percentmedian_value,ccof_3percentageofoosctog,ccof_3percentageofoosctok,ccof_3percentageofpre,ccof_3yearstokindergarten,ccof_name,ccof_outofschoolcaregrade1,ccof_outofschoolcarekindergarten,ccof_preschool),ccof_RegionNMFBenchmark($select=ccof_fee_benchmark_sdaid,ccof_0to18m,ccof_18to36m,ccof_3ytok,ccof_name,ccof_oosctograde,ccof_oosctok,ccof_preschool)");
                // var FeeIncreaseDetails24Months = getSyncMultipleRecord("ccof_ccfrifacilityfeeincrease24months?$select=_ccof_adjudicationccfrifacility_value, ccof_averageenrolment, _ccof_childcarecategory_value, ccof_cumulativefeeincrease, ccof_feebeforeincrease, ccof_feeincreasetype, ccof_name, _ccof_programyear_value, ccof_reasonfor24monthsadj, ccof_cumulativefeeincreasepercent, _ccof_applicationccfrichildcarecategory_value&$filter=(_ccof_adjudicationccfrifacility_value eq " + getCleanedGuid(entityId) + " and statecode eq 0 and ccof_feebeforeincrease ne 'N/A')&$orderby=_ccof_childcarecategory_value asc");
                // Validate FeeIncrease info
                var mefiCAP24month = formContext.getAttribute("ccof_meficap24month").getValue();
                var limitfeestonmfbenchmark24month = formContext.getAttribute("ccof_limitfeestonmfbenchmark_24month").getValue();
                var ifCalculate24Months = true;
                if (FeeIncreaseDetails24Months.length === 0) {
                    //  Xrm.Navigation.openAlertDialog("24-Month Fee Increase is required! Will no Calculate 24 Months tab");
                    ifCalculate24Months = false;
                    //return;
                } else {
                    for (let i in FeeIncreaseDetails24Months) {
                        if (FeeIncreaseDetails24Months[i]['ccof_averageenrolment'] === null || FeeIncreaseDetails24Months[i]['ccof_cumulativefeeincrease'] === null || FeeIncreaseDetails24Months[i]['ccof_feebeforeincrease'] === null) {
                            // Xrm.Navigation.openAlertDialog("One or more 24-Month Fee Increase Amount are missing!");
                            ifCalculate24Months = false;
                            // return;
                        }
                    }
                }
                if (ifCalculate24Months) {
                    var FacilityAmountAllowedRecords24Months = getSyncMultipleRecord("ccof_ccfrifacilityallowableamount_24months?$select=ccof__3yearstokindergarten,ccof_outofschoolcarekindergarten,ccof_preschool,ccof_to36months,ccof_to18months,ccof_stage3policy,ccof_outofschoolcaregrade1,ccof_displayorder&$filter=(_ccof_ccfrifacility_value eq " + entityId + ")&$orderby=ccof_displayorder asc");
                    console.log("FeeIncreaseDetails24Months" + JSON.stringify(FacilityAmountAllowedRecords24Months));
                    var returnValue24Months = Calculator(RegionInfos24Months, FeeIncreaseDetails24Months, ExpenseInfo24Months);
                    // Xrm.Navigation.openAlertDialog("24 Months"+JSON.stringify(returnValue24Months));
                    // formContext.getAttribute("ccof_monthadjudicatornotes").setValue(returnValue24Months['AdjudicatorNote']);
                    //need to pass values of 24 month adjudication as well 
                    var TotalAllowableStagePolicy24Months = Populate24MonthSummaryApprovedAmount(returnValue24Months, FacilityAmountAllowedRecords24Months, ccfri_facility_allowable_amount_24MonthEntityName, FeeIncreaseDetails24Months, RegionInfos24Months, entityId, mefiCAP24month, limitfeestonmfbenchmark24month);
                    //  IndicateCap24Month(FeeIncreaseDetails24Months, TotalAllowableStagePolicy24Months, RegionInfos, entityId, mefiCAP24month, limitfeestonmfbenchmark24month);
                    //IndicateCap(FeeIncreaseDetails, TotalAllowableStagePolicy, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyId);
                    //var currentDate = new Date();
                    //let historyValue = formContext.getAttribute("ccof_monthadjudicatornotes").getValue();
                    //let plainValue = convertToPlain(historyValue);
                    //formContext.getAttribute("ccof_monthadjudicatornotes").setValue(plainValue + '\n' + returnValue24Months['AdjudicatorNote']);
                    formContext.getAttribute("ccof_stage3calculatornotes24month").setValue(returnValue24Months['AdjudicatorNote']);
                    console.log("End 24 Months Calculation");
                    formContext.getControl("AllowableAmount24Months").refresh();
                    // Xrm.Utility.closeProgressIndicator();
                    //Xrm.Navigation.openAlertDialog(" 24 Months Completed Calculation!");
                }
            }
            var alertOptions = { height: 240, width: 320 };
            if (ifCalculateInitial) {
                if (ifCalculate24Months) {
                    formContext.data.save().then(function () {
                        // Save completed
                        formContext.data.refresh(true); // Refresh the form
                    }).catch(function (error) {
                        // Handle save error
                    });

                    Xrm.Navigation.openAlertDialog(" Initial Adjudication Calculation is complete! \n 24 Months Adjudication is complete!", alertOptions);
                } else {
                    formContext.data.save().then(function () {
                        // Save completed
                        formContext.data.refresh(true); // Refresh the form
                    }).catch(function (error) {
                        // Handle save error
                    });

                    Xrm.Navigation.openAlertDialog(" Initial Adjudication Calculation is complete!\n 24 Months Adjudication will not be calculated as Fee Increase Amount is missing or No Fee Increase Amount data need to be calculated!", alertOptions);
                }
            } else {
                if (ifCalculate24Months) {
                    formContext.data.save().then(function () {
                        // Save completed
                        formContext.data.refresh(true); // Refresh the form
                    }).catch(function (error) {
                        // Handle save error
                    });
                    Xrm.Navigation.openAlertDialog(" Initial Adjudication will not be calculated as Fee Increase Amount is missing! or No Fee Increase Amount data need to be calculated! \n 24 Months Adjudication Calculation is complete", alertOptions);
                } else {
                    Xrm.Navigation.openAlertDialog(" Initial Adjudication will not be calculated as Fee Increase Amount is missing! or No Fee Increase Amount data need to be calculated!\n 24 Months Adjudication will not be calculated as Fee Increase Amount is missing or No Fee Increase Amount data need to be calculated!", alertOptions);
                }
            }
        }
        catch (err) {
            alert("There are some exceptional errors happened, please contact Administrator!" + err);
        }
    },
    OnMEFICap: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        CCOF.AdjudicationCCFRIFacility.Calculation.Calculation(formContext);
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

function Calculator(regionInfo, feeIncreaseDetails, expenseInfo) {
    debugger;

    var OrgType = regionInfo['ccof_Application']['ccof_providertype@OData.Community.Display.V1.FormattedValue'];   //"Group" or Family
    var SDA = regionInfo['_ccof_region_value@OData.Community.Display.V1.FormattedValue']; //"North Fraser";
    var Programyear = regionInfo['ccof_Application']['_ccof_programyear_value@OData.Community.Display.V1.FormattedValue']; //"2022/23";
    var Limitfeesto70Percentile = expenseInfo['Limit Fees to NMF Benchmark'];  // C33 of Stage 3 Calculator is from CRM Limit Fees to NMF Benchmark  (toggle) 
    var DilutionCap = expenseInfo['MEFI Cap']; // CRM MEFI Cap (toggle) //B7  ?? need confirm 
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
    var TotalMonthlyExpenses = expenseInfo['Total Monthly Expenses'];  // B11 ? comes from CRM 
    var FacilityExpense =   // comes from CRM 
    {
        "Exceptional Circumstances": expenseInfo['Exceptional Circumstances'],
        "Direct Care Staff Wages": expenseInfo['Direct Care Staff Wages'],
        "MTFI: Unused Expenses": 0,
        "Priority Service Expansion": expenseInfo['Priority Service Expansion'],
        "Priority SE (Indigenous)": 0,
        "Total Monthly Expenses": 0  // no use now.
    }
    var FacilityInfo = [];
    for (let i in feeIncreaseDetails) {
        let entity = {};
        entity['CareCategory'] = feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'];
        entity['AverageEnrollment'] = parseFloat(feeIncreaseDetails[i]['ccof_averageenrolment']);
        entity['RequestedFeeIncrease'] = feeIncreaseDetails[i]['ccof_cumulativefeeincrease'];
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
        tempAllowances['3% Allowable Fee Increase'] = MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per3')];
        AllowancesOnCalculator[FacilityInfo[i]['CareCategory']] = tempAllowances;
        // Populate Inital Calculation B3 Average Enrollment
        tempInital['Average Enrollment'] = FacilityInfo[i]['AverageEnrollment'];
        // Calculation!B4
        tempInital['Allowances'] = (tempCap['Cap'] === null) ? tempAllowances['3% Allowable Fee Increase'] : ((tempCap['Cap'] < tempAllowances['3% Allowable Fee Increase']) ? tempCap['Cap'] : tempAllowances['3% Allowable Fee Increase']);
        // Calculation!B5
        tempInital['Request'] = FacilityInfo[i]['RequestedFeeIncrease'];
        // Calculation!B6. K3=Limit fees to 70 Percentile
        tempInital['Dilution Cap'] = Limitfeesto70Percentile ? tempCap['Lesser'] : MediansFee[FacilityInfo[i]['CareCategory'].concat('_Per10')];
        // Calculation!B7
        InitalCalculation_DilutionCap = Limitfeesto70Percentile;
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
        entity['Allowance'] = (InitalCalculation[item]['Allowances'] < InitalCalculation[item]['Request/Dilution Cap Result']) ? InitalCalculation[item]['Allowances'] : InitalCalculation[item]['Request/Dilution Cap Result'];
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
            //Round1[item]['Final Amount'] = ((TotalMonthlyExpenses + weightSumAllowance) / sumEnrollment).toFixed(2);
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
                //roundTemp[item]['Approvable 1'] = !roundTemp[item]['Check for negative'] ? roundTemp[item]['Final Amount'] : ((parseFloat(roundTemp[item]['Final Amount']) > parseFloat(roundTemp[item]['Allowance'])) ? parseFloat(roundTemp[item]['Final Amount']) : roundTemp[item]['Allowance']);
                roundTemp[item]['Approvable 1'] = !roundTemp[item]['Check for negative'] ? roundTemp[item]['Final Amount'] : ((parseFloat(roundTemp[item]['Final Amount']) > parseFloat(Round1[item]['Allowance'])) ? parseFloat(roundTemp[item]['Final Amount']) : Round1[item]['Allowance']);
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
        let entity = {};
        // I8
        entity['Requested Fee Increase'] = FacilityInfo[i]['RequestedFeeIncrease'];
        //I9
        entity['Max Approvable'] = (FacilityInfo[i]['AverageEnrollment'] === 0) ? 0 : InitalCalculation[FacilityInfo[i]['CareCategory']]['FINAL APPROVABLE'];
        resultString = resultString + " " + FacilityInfo[i]['CareCategory'] + ": $" + entity['Max Approvable'].toFixed(2);
        entity['Full Request Approvable?'] = (entity['Requested Fee Increase'] <= entity['Max Approvable']) ? true : false;
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
    //var readline = require('readline');
    //var r1 = readline.createInterface(process.stdin, process.stdout);
    //return AdjudicatorNote;
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


function PopulateSummaryApprovedAmount(returnValue, FacilityAmountAllowedRecords, entityname, FeeIncreaseDetails, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark) {
    debugger;
    var TotalAllowableStagePolicy = {};
    // To prepopulate the records in summary record.
    if (FacilityAmountAllowedRecords != null) {
        debugger;
        var accup_0to18months;
        var accup_18to36months;
        var accup_preschol;
        var accup_outofschoolkindergarden;
        var accup_3yearstokindergarden;
        var accup_outofschoolcaregrade1;
        var stage2_0to18months;
        var stage2_18to36months;
        var stage2_preschol;
        var stage2_outofschoolkindergarden;
        var stage2_3yearstokindergarden;
        var stage2_outofschoolcaregrade1;
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
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Total Allowable Fee Increase") {
                TotalAllowableStagePolicy = {
                    "ccof_0to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : 0,
                    "ccof_18to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : 0,
                    "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : 0,
                    "ccof_3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : 0
                }
                // commented this logic based on tickets 1135
                //TotalAllowableStagePolicy = {
                //    "ccof_0to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Direct Care Staff Wages']) + parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Exceptional Circumstances']) + accup_0to18months + stage2_0to18months) : accup_0to18months + stage2_0to18months,
                //    "ccof_18to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Direct Care Staff Wages']) + stage2_18to36months) : accup_18to36months + stage2_18to36months,
                //    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Direct Care Staff Wages']) + accup_preschol + stage2_preschol) : accup_preschol + stage2_preschol,
                //    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Direct Care Staff Wages']) + accup_outofschoolkindergarden + stage2_outofschoolkindergarden) : accup_outofschoolkindergarden + stage2_outofschoolkindergarden,
                //    "ccof_3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Direct Care Staff Wages']) + accup_3yearstokindergarden + stage2_3yearstokindergarden) : accup_3yearstokindergarden + stage2_3yearstokindergarden,
                //    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Direct Care Staff Wages']) + accup_outofschoolcaregrade1 + stage2_outofschoolcaregrade1) : accup_outofschoolcaregrade1 + stage2_outofschoolcaregrade1
                //}
                TotalAllowableStagePolicyId = FacilityAmountAllowedRecords[i]['ccof_ccfri_facility_allowable_amountid'];
                debugger;

                UpdateEntityRecord(entityname, TotalAllowableStagePolicyId, TotalAllowableStagePolicy);
                Xrm.Page.ui.controls.get("AllowableAmount").refresh();
                //Xrm.Page.ui.tabs.get("Adjudication_section_7").refresh();
            }
        }
        IndicateCap(FeeIncreaseDetails, TotalAllowableStagePolicy, RegionInfos, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyId);
    }

    return TotalAllowableStagePolicy;
}

function Populate24MonthSummaryApprovedAmount(returnValue, FacilityAmountAllowedRecords, entityname, FeeIncreaseDetails24Months, RegionInfos, entityId, mefiCAP24month, limitfeestonmfbenchmark24month) {
    debugger;
    var TotalAllowableStagePolicy = {};

    // To prepopulate the records in summary record.
    if (FacilityAmountAllowedRecords != null) {
        debugger;
        var accup_0to18months;
        var accup_18to36months;
        var accup_preschol;
        var accup_outofschoolkindergarden;
        var accup_3yearstokindergarden;
        var accup_outofschoolcaregrade1;
        var stage2_0to18months;
        var stage2_18to36months;
        var stage2_preschol;
        var stage2_outofschoolkindergarden;
        var stage2_3yearstokindergarden;
        var stage2_outofschoolcaregrade1;
        var TotalAllowableStagePolicyId;

        let arrayLength = FacilityAmountAllowedRecords.length;
        for (let i = 0; i < arrayLength; i++) {

            console.log(JSON.stringify(FacilityAmountAllowedRecords[i]));

            //Exceptional Circumstances
            if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Exceptional Circumstances") {

                var ExceptionalCircumstancesStagePolicy = {

                    "ccof_to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Exceptional Circumstances']) : 0,
                    "ccof_to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Exceptional Circumstances']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Exceptional Circumstances']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Exceptional Circumstances']) : 0,
                    "ccof__3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Exceptional Circumstances']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Exceptional Circumstances']) : 0

                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfrifacilityallowableamount_24monthid'];
                UpdateEntityRecord(entityname, Id, ExceptionalCircumstancesStagePolicy)



            }
            //Direct care staff wages
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Direct Care Staff Wages") {
                var DirectCareStaffWagesStagePolicy = {
                    "ccof_to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Direct Care Staff Wages']) : 0,
                    "ccof_to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Direct Care Staff Wages']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Direct Care Staff Wages']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Direct Care Staff Wages']) : 0,
                    "ccof__3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Direct Care Staff Wages']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Direct Care Staff Wages']) : 0

                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfrifacilityallowableamount_24monthid'];
                UpdateEntityRecord(entityname, Id, DirectCareStaffWagesStagePolicy);


            }

            //Priority Service Expansion
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Priority Service Expansion") {
                var PriorityServiceExpansionStagePolicy = {
                    "ccof_to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Priority SE(Extended Hours)']) : 0,
                    "ccof_to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Priority SE(Extended Hours)']) : 0,
                    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Priority SE(Extended Hours)']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Priority SE(Extended Hours)']) : 0,
                    "ccof__3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Priority SE(Extended Hours)']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Priority SE(Extended Hours)']) : 0
                }
                var Id = FacilityAmountAllowedRecords[i]['ccof_ccfrifacilityallowableamount_24monthid'];
                UpdateEntityRecord(entityname, Id, PriorityServiceExpansionStagePolicy);


            }
            //ACCUP
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "ACCUP") {
                accup_0to18months = FacilityAmountAllowedRecords[i]['ccof_to18months'];
                accup_18to36months = FacilityAmountAllowedRecords[i]['ccof_to36months'];
                accup_preschol = FacilityAmountAllowedRecords[i]['ccof_preschool'];
                accup_outofschoolkindergarden = FacilityAmountAllowedRecords[i]['ccof_outofschoolcarekindergarten'];
                accup_3yearstokindergarden = FacilityAmountAllowedRecords[i]['ccof__3yearstokindergarten'];
                accup_outofschoolcaregrade1 = FacilityAmountAllowedRecords[i]['ccof_outofschoolcaregrade1'];

            }
            //Stage 2 - Fee Increase Limit (FIL)
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Stage 2 - Fee Increase Limit (FIL)") {
                stage2_0to18months = FacilityAmountAllowedRecords[i]['ccof_to18months'];
                stage2_18to36months = FacilityAmountAllowedRecords[i]['ccof_to36months'];
                stage2_preschol = FacilityAmountAllowedRecords[i]['ccof_preschool'];
                stage2_outofschoolkindergarden = FacilityAmountAllowedRecords[i]['ccof_outofschoolcarekindergarten'];
                stage2_3yearstokindergarden = FacilityAmountAllowedRecords[i]['ccof__3yearstokindergarten'];
                stage2_outofschoolcaregrade1 = FacilityAmountAllowedRecords[i]['ccof_outofschoolcaregrade1'];
            }
            //Total Allowable Fee Increase - need to total // changed based on ticket 1135
            else if (FacilityAmountAllowedRecords[i]['ccof_stage3policy@OData.Community.Display.V1.FormattedValue'] == "Total Allowable Fee Increase") {
                TotalAllowableStagePolicy = {
                    "ccof_to18months": (returnValue['FinalCalculations'].hasOwnProperty('0-18') == true) ? parseFloat(returnValue['FinalCalculations']['0-18']['Max Approvable']) : 0,
                    "ccof_to36months": (returnValue['FinalCalculations'].hasOwnProperty('18-36') == true) ? parseFloat(returnValue['FinalCalculations']['18-36']['Max Approvable']) : 0,
                    "ccof_preschool": (returnValue['FinalCalculations'].hasOwnProperty('PRE') == true) ? parseFloat(returnValue['FinalCalculations']['PRE']['Max Approvable']) : 0,
                    "ccof_outofschoolcarekindergarten": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-K') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-K']['Max Approvable']) : 0,
                    "ccof__3yearstokindergarten": (returnValue['FinalCalculations'].hasOwnProperty('3Y-K') == true) ? parseFloat(returnValue['FinalCalculations']['3Y-K']['Max Approvable']) : 0,
                    "ccof_outofschoolcaregrade1": (returnValue['FinalCalculations'].hasOwnProperty('OOSC-G') == true) ? parseFloat(returnValue['FinalCalculations']['OOSC-G']['Max Approvable']) : 0
                }

                // commented based on ticket 1135
                //TotalAllowableStagePolicy = {
                //    "ccof_to18months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('0-18') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Direct Care Staff Wages']) + parseFloat(returnValue['AmountApprovedPerCategory']['0-18']['Exceptional Circumstances']) + accup_0to18months + stage2_0to18months) : accup_0to18months + stage2_0to18months,
                //    "ccof_to36months": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('18-36') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['18-36']['Direct Care Staff Wages']) + stage2_18to36months) : accup_18to36months + stage2_18to36months,
                //    "ccof_preschool": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('PRE') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['PRE']['Direct Care Staff Wages']) + accup_preschol + stage2_preschol) : accup_preschol + stage2_preschol,
                //    "ccof_outofschoolcarekindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-K') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-K']['Direct Care Staff Wages']) + accup_outofschoolkindergarden + stage2_outofschoolkindergarden) : accup_outofschoolkindergarden + stage2_outofschoolkindergarden,
                //    "ccof__3yearstokindergarten": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('3Y-K') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['3Y-K']['Direct Care Staff Wages']) + accup_3yearstokindergarden + stage2_3yearstokindergarden) : accup_3yearstokindergarden + stage2_3yearstokindergarden,
                //    "ccof_outofschoolcaregrade1": (returnValue['AmountApprovedPerCategory'].hasOwnProperty('OOSC-G') == true) ? (parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Priority SE(Extended Hours)']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Exceptional Circumstances']) + parseFloat(returnValue['AmountApprovedPerCategory']['OOSC-G']['Direct Care Staff Wages']) + accup_outofschoolcaregrade1 + stage2_outofschoolcaregrade1) : accup_outofschoolcaregrade1 + stage2_outofschoolcaregrade1
                //}
                TotalAllowableStagePolicyId = FacilityAmountAllowedRecords[i]['ccof_ccfrifacilityallowableamount_24monthid'];
                UpdateEntityRecord(entityname, TotalAllowableStagePolicyId, TotalAllowableStagePolicy);
            }
        }
        IndicateCap24Month(FeeIncreaseDetails24Months, TotalAllowableStagePolicy, RegionInfos, entityId, mefiCAP24month, limitfeestonmfbenchmark24month, TotalAllowableStagePolicyId);
    }

    return TotalAllowableStagePolicy;
}
function IndicateCap(feeIncreaseDetails, TotalAllowableStagePolicy, regionInfo, entityId, mefiCAP, limitfeestonmfbenchmark, TotalAllowableStagePolicyID) {
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
            //TotalAllowableStagePolicywithMEFICAP = {
            //    "ccof_to18months": MediansFee['0-18_Per10'],
            //    "ccof_to36months": MediansFee['18-36_Per10'],
            //    "ccof_preschool": MediansFee['PRE_Per10'],
            //    "ccof_outofschoolcarekindergarten": MediansFee['OOSC-K_Per10'],
            //    "ccof__3yearstokindergarten": MediansFee['3Y-K_Per10'],
            //    "ccof_outofschoolcaregrade1": MediansFee['OOSC-G_Per10']
            //}
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
            //TotalAllowableStagePolicywithMEFICAP = {
            //    "ccof_to18months": MediansFee['0-18_Per10'],
            //    "ccof_to36months": MediansFee['18-36_Per10'],
            //    "ccof_preschool": MediansFee['PRE_Per10'],
            //    "ccof_outofschoolcarekindergarten": MediansFee['OOSC-K_Per10'],
            //    "ccof__3yearstokindergarten": MediansFee['3Y-K_Per10'],
            //    "ccof_outofschoolcaregrade1": MediansFee['OOSC-G_Per10']
            //}
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

function IndicateCap24Month(feeIncreaseDetails, TotalAllowableStagePolicy, regionInfo, entityId, mefiCAP24month, limitfeestonmfbenchmark24month, TotalAllowableStagePolicyID) {
    // set the indicator as pass or fail.
    var NMFCAPReached = false;
    var MEFICAPReached = false;
    var entityname = "ccof_adjudication_ccfri_facility";

    var ccfri_facility_allowable_amount_24MonthEntityName = "ccof_ccfrifacilityallowableamount_24month";
    var ReachedCap;
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

        if (mefiCAP24month == true) {
            //Compare MEFI(10% median) wit total approved amount
            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_to18months'] > MediansFee['0-18_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_to36months'] > MediansFee['18-36_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_preschool'] > MediansFee['PRE_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_outofschoolcarekindergarten'] > MediansFee['OOSC-K_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof__3yearstokindergarten'] > MediansFee['3Y-K_Per10'] ? true : false; }

            if (MEFICAPReached == false) { MEFICAPReached = TotalAllowableStagePolicy['ccof_outofschoolcaregrade1'] > MediansFee['OOSC-G_Per10'] ? true : false; }
        }

        //compareNMF( NMF - Fee Before increase) with total approved amount
        if (limitfeestonmfbenchmark24month == true) {
            for (const i in feeIncreaseDetails) {
                if (NMFCAPReached == false) {

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "0-18") { NMFCAPReached = TotalAllowableStagePolicy['ccof_to18months'] > (SDA70thPercentileF['0-18'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false; }
                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "18-36") { NMFCAPReached = TotalAllowableStagePolicy['ccof_to36months'] > (SDA70thPercentileF['18-36'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false; }

                    if (feeIncreaseDetails[i]['_ccof_childcarecategory_value@OData.Community.Display.V1.FormattedValue'] == "3Y-K") { NMFCAPReached = TotalAllowableStagePolicy['ccof__3yearstokindergarten'] > (SDA70thPercentileF['3Y-K'] - feeIncreaseDetails[i]['ccof_feebeforeincrease']) ? true : false; }

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
                "ccof_reachedcaps_24month": 100000002
            }
            TotalAllowableStagePolicywithMEFICAP = {
                "ccof_to18months": MediansFee['0-18_Per10'],
                "ccof_to36months": MediansFee['18-36_Per10'],
                "ccof_preschool": MediansFee['PRE_Per10'],
                "ccof_outofschoolcarekindergarten": MediansFee['OOSC-K_Per10'],
                "ccof__3yearstokindergarten": MediansFee['3Y-K_Per10'],
                "ccof_outofschoolcaregrade1": MediansFee['OOSC-G_Per10']
            }
            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amount_24MonthEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);
        }
        else if (MEFICAPReached == true && NMFCAPReached == false) {
            ReachedCap = {
                "ccof_reachedcaps_24month": 100000000

            }
            TotalAllowableStagePolicywithMEFICAP = {
                "ccof_to18months": MediansFee['0-18_Per10'],
                "ccof_to36months": MediansFee['18-36_Per10'],
                "ccof_preschool": MediansFee['PRE_Per10'],
                "ccof_outofschoolcarekindergarten": MediansFee['OOSC-K_Per10'],
                "ccof__3yearstokindergarten": MediansFee['3Y-K_Per10'],
                "ccof_outofschoolcaregrade1": MediansFee['OOSC-G_Per10']
            }

            UpdateEntityRecord(entityname, entityId, ReachedCap);
            UpdateEntityRecord(ccfri_facility_allowable_amount_24MonthEntityName, TotalAllowableStagePolicyID, TotalAllowableStagePolicywithMEFICAP);
        }
        else if (MEFICAPReached == false && NMFCAPReached == true) {
            ReachedCap = {
                "ccof_reachedcaps_24month": 100000001

            }
            UpdateEntityRecord(entityname, entityId, ReachedCap)
        }
            // ticket CCFRI-2126 fix. Jun 30, 2023
        else if (MEFICAPReached == false && NMFCAPReached == true) {
            ReachedCap = {
                "ccof_reachedcaps_24month": null
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