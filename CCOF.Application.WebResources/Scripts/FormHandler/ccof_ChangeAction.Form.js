function hideFacilitiesSection(executionContext) {
    var formContext = executionContext.getFormContext();
    var changeType = Xrm.Page.getAttribute("ccof_changetype");

    if (typeof (changeType) != "undefined" && changeType.getValue() != null) {

        if (changeType.getValue() == 100000005) { //Value of "Add New Facility"
            //hide the document list and mtfi
            formContext.ui.tabs.get("general_tab").sections.get("attachments_section").setVisible(false);
            formContext.ui.tabs.get("general_tab").sections.get("mtfi_facilities_section").setVisible(false);

            //show the unlock section
            formContext.ui.tabs.get("general_tab").sections.get("org_unlock_section").setVisible(true);
            //hide the change request unlock and document unlock field
            formContext.getControl("ccof_unlock_change_request").setVisible(false);
            formContext.getControl("ccof_unlock_other_changes_document").setVisible(false);

        } else if (changeType.getValue() == 100000007) { //Value of "MTFI"
            //hide the document list and new facilities
            //formContext.ui.tabs.get("general_tab").sections.get("attachments_section").setVisible(false);
            formContext.ui.tabs.get("general_tab").sections.get("new_facilities_section").setVisible(false);
        }
        else if (changeType.getValue() == 100000015 || changeType.getValue() == 100000016 || changeType.getValue() == 100000017) { //Value of "NEW CLOSURE" "EDIT CLOSURE" "REMOVE CLOSURE"
            debugger;
            formContext.ui.tabs.get("general_tab").sections.get("change_action_closure_section").setVisible(true);
            formContext.ui.tabs.get("general_tab").sections.get("new_facilities_section").setVisible(false);
            formContext.ui.tabs.get("general_tab").sections.get("mtfi_facilities_section").setVisible(false);

        }
        else { //Change type is other changes
            //hide the mtfi and new facilities
            formContext.ui.tabs.get("general_tab").sections.get("new_facilities_section").setVisible(false);
            formContext.ui.tabs.get("general_tab").sections.get("mtfi_facilities_section").setVisible(false);

            //show the unlock section
            formContext.ui.tabs.get("general_tab").sections.get("org_unlock_section").setVisible(true);
            //hide the ecewe unlock, licence upload unlock and supporting document unlock field
            formContext.getControl("ccof_unlock_ccof").setVisible(false);
            formContext.getControl("ccof_unlock_ecewe").setVisible(false);
            formContext.getControl("ccof_unlock_licence_upload").setVisible(false);
            formContext.getControl("ccof_unlock_supporting_document").setVisible(false);
        }
    }
}

//Open Unlock Notes Custom Page On save
function openCRUnlockCustomPage(entityName, recordId) {
    var window_width = 1400;
    var window_height = 900;

    Xrm.WebApi.retrieveRecord(entityName, recordId).then(
        function success(result) {

            //if Change Action type is not MTFI and New Facility
            if (result.ccof_changetype != 100000005 && result.ccof_changetype != 100000007) {
                window_height = 417;
            }
            // console.log(result);
            Xrm.Navigation.navigateTo({
                pageType: "custom",
                name: "ccof_changerequestunlockform_f6704",
                entityName: entityName,
                recordId: recordId,

            }, {
                target: 2,
                width: window_width,
                height: window_height
            })
                .then(function () {
                    // formContext.data.refresh();
                })
                .catch(console.error);

        },
        function (error) {
            console.log(error.message);
            // handle error conditions
        }
    )

}
function showHideCreateUnlockButton(primaryControl) {
    debugger;
    var formContext = primaryControl;


    var visible = false;
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    userRoles.forEach(function hasRole(item, index) {
        if (item.name === "CCOF - Sr. Adjudicator" || item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "System Administrator") {
            visible = true;
        }
    });
    return visible;
};