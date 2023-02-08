//the below function working only onLoad of the form
function onLoafdOfRfiParentFeeIncrease() {
    var formType = Xrm.Page.ui.getFormType();
    ReadOnly(formType);
}
function parentFeesIncrease() {
    var feesValue = Xrm.Page.getAttribute("ccof_haveyouincreasedparentfeesbefore").getValue();
    console.log("s" + feesValue);
    if (feesValue == 0) {
        Xrm.Page.ui.tabs.get("general_tab").sections.get("feeHistoryDetails").setVisible(false);
    }
    else {
        Xrm.Page.ui.tabs.get("general_tab").sections.get("feeHistoryDetails").setVisible(true);
    }
}

function expenseCircumastances() {
    debugger;
    var excepVal = Xrm.Page.getAttribute("ccof_exceptionalcircumstanceoccurwithin6m").getValue();
    if (excepVal == 0) {
        Xrm.Page.ui.tabs.get("general_tab").sections.get("expenseInfo").setVisible(false);
    }
    else {
        Xrm.Page.ui.tabs.get("general_tab").sections.get("expenseInfo").setVisible(true);
    }
}

//Make all fields read only on the application form
function ReadOnly(formTypeVal) {
    if (formTypeVal != 1) {
        var cs = Xrm.Page.ui.controls.get();
        for (var i in cs) {
            var c = cs[i];
            if (c.getName() != "" && c.getName() != null) {
                if (!c.getDisabled()) { c.setDisabled(true); }
            }
        }
    }
    else   //create
    {
    }
}