var CCOF = CCOF || {};
CCOF.NoteFlag = CCOF.NoteFlag || {};
CCOF.NoteFlag.Form = CCOF.NoteFlag.Form || {};
CCOF.NoteFlag.Form = {
    onLoad: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        let orgId = formContext.getAttribute("ccof_organization").getValue()[0].id;
        let lookupFacilityControl = formContext.getControl("ccof_facility");
        let viewId = CCOF.NoteFlag.Form.generateGuid();
        formContext.getAttribute("ccof_defaultviewkey").setValue(viewId); // set ViewId
        formContext.data.entity.save();
        let entityName = "account";
        let viewDisplayName = "Facilities View";
        let fetchXml = [
            "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
            "  <entity name='account'>",
            "    <attribute name='accountnumber' />",
            "    <attribute name='accountid' />",
            "    <filter type='and'>",
            "      <condition attribute='statecode' operator='eq' value='0' />",
            "      <condition attribute='parentaccountid' operator='eq' value='", getCleanedGuid(orgId), "' />",
            "    </filter>",
            "  </entity>",
            "</fetch>"
        ].join("");
        let layoutXml = [
            "<grid name='resultset' object='1' jump='name' select='1' icon='1' preview='1'>",
            "  <row name='result' id='accountid'>",
            "    <cell name='accountnumber' width='300' />",
            "    <cell name='name' width='300' />",
            "  </row>",
            "</grid>"
        ].join("");

        lookupFacilityControl.addCustomView(viewId, entityName, viewDisplayName, fetchXml, layoutXml, true);
        lookupFacilityControl.setDefaultView(viewId);
        setTimeout(function () {
            console.log("Runs after 1 second");
        }, 1000);
        formContext.getAttribute("ccof_scope").addOnChange(onChange_ShowHideFacilitySubgrid);
        if (formContext.getAttribute("ccof_scope").getValue() && formContext.getAttribute("ccof_scope").getValue().includes(3)) {  // "Specific Facility"
            formContext.getControl("ccof_facilities").setVisible(true);
        } else {
            formContext.getControl("ccof_facilities").setVisible(false);
        }
    },
    HideNewOnlyforCCOFFlagForm: function (primaryControl) {
        try {
            debugger;
            // If not on a form, fail safe (return true to avoid hiding everywhere)
            if (!primaryControl || !primaryControl.ui || !primaryControl.ui.formSelector) {
                return true;
            }
            var formId = primaryControl.ui.formSelector.getCurrentItem().getId();
            return formId.toLowerCase() != "d78fb4fa-2503-f111-8407-7ced8d050b1a"; // CCOF Flag form form NotesFlag table 
        } catch (e) {
            // On any error, do not block the UI; default to showing/enabling
            return true;
        }
    },
    generateGuid: function () {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0;
            var v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}
function onChange_ShowHideFacilitySubgrid(executionContext) {
    debugger;
    let formContext = executionContext.getFormContext();
    if (formContext.getAttribute("ccof_scope").getValue() && formContext.getAttribute("ccof_scope").getValue().includes(3)) {  // "Specific Facility"
        formContext.getControl("ccof_facilities").setVisible(true);
    } else {
        formContext.getControl("ccof_facilities").setVisible(false);
    }
}
function getCleanedGuid(id) {
    return id.replace("{", "").replace("}", "");
}