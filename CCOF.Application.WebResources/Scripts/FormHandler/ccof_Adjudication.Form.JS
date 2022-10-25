var CCOF = CCOF || {};
CCOF.Adjudication = CCOF.Adjudication || {};
CCOF.Adjudication.Form = CCOF.Adjudication.Form || {};

//show and hide checklist section
function checklist() {
    debugger;

    var newProviderValue = Xrm.Page.getAttribute("ccof_newprovider").getValue();
    if (newProviderValue == false)//No-100,000,000
    {
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("RenewChecklistSection").setVisible(true);
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("NewChecklistSection").setVisible(false);
    }
    else if (newProviderValue == true)  //Yes  -100000001
    {
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("NewChecklistSection").setVisible(true);
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("RenewChecklistSection").setVisible(false);
    }
    else {
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("NewChecklistSection").setVisible(false);
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("RenewChecklistSection").setVisible(false);

    }
}


CCOF.Adjudication.Form = {
    onLoad: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        let appCCFRI = formContext.getAttribute("ccof_applicationccfri").getValue();
        if (appCCFRI !== null) {
            let appCCFRIid = appCCFRI[0].id;
            let appCCFRIReq = "ccof_applicationccfris(" + getCleanedGuid(appCCFRIid) + ")?$select=ccof_feecorrectccfri";
            let appCCFRIResponse = getSyncSingleRecord(appCCFRIReq);
            // 100000000  Yes 100000001 No
            if (appCCFRIResponse["ccof_feecorrectccfri"] === 100000000) {
                formContext.ui.tabs.get("Adjudication").setVisible(true);
                formContext.ui.tabs.get("MonthAdjudication").setVisible(false);

            }
            else {
                formContext.ui.tabs.get("Adjudication").setVisible(false);
                formContext.ui.tabs.get("MonthAdjudication").setVisible(true);

            }
        }
    },
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
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
    return results;
}