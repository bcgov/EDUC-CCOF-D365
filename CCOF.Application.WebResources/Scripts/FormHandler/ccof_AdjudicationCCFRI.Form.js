var AdjCCFRI = AdjCCFRI || {};
AdjCCFRI.Form = AdjCCFRI.Form || {};
//Formload logic starts here
AdjCCFRI.Form = {
    onSave: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate

            case 2: // update                           
                this.saveConfirm(executionContext);
                break;
            case 3: //readonly
                break;
            case 4: //disable
                break;
            case 6: //bulkedit
                break;
        }


    },

    onLoad: function (executionContext) {
        // removed it based on Ticket 1132
        //debugger;
        //let formContext = executionContext.getFormContext();
        //var roles = Xrm.Utility.getGlobalContext().userSettings.roles;
        //if (roles === null) return false;
        //var hasRole = false;
        //roles.forEach(function (item) {
        //    if (item.name == "CCOF - QC" || item.name == "CCOF - Leadership") {
        //        hasRole = true;
        //    }
        //});

        //if (hasRole === true) {
        //    formContext.ui.tabs.get("decisionemail").setVisible(true);
        //} else {
        //    formContext.ui.tabs.get("decisionemail").setVisible(false);
        //}

        // Jan 26,2026 ticket 6933
        debugger;
        let formContext = executionContext.getFormContext();
        let app = formContext.getAttribute("ccof_application").getValue();
        let appId = app[0].id;
        Xrm.WebApi.retrieveRecord("ccof_application", appId, "?$select=ccof_applicationtype,ccof_name").then(
            function success(results) {
                console.log(results);
                if (results["ccof_applicationtype"] != null) {
                    let appType = results["ccof_applicationtype"];
                    if (appType === 100000002) { // renewal application
                        formContext.getControl("ccof_unlock_ccof").setVisible(false);
                        formContext.getControl("ccof_unlockrenewal").setVisible(true);
                    }
                    else {
                        formContext.getControl("ccof_unlock_ccof").setVisible(true);
                        formContext.getControl("ccof_unlockrenewal").setVisible(false);
                    }
                }
            },
            function (error) {
                console.log(error.message);
            }
        );
    },

    saveConfirm: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var saveVCHA = formContext.getAttribute("ccof_vcha").getValue();  //Two options feild - Yes - 1(True) and No - 0(False)
        var confirmStrings = { text: "You have updated VCHA at organization level. This change will be cascaded to all child facilities. Are you sure you want to proceed?", title: "Confirmation Dialog" };
        var confirmOptions = { height: 200, width: 450 };
        var isDirtyVCHA = formContext.getAttribute("ccof_vcha").getIsDirty();
        if (isDirtyVCHA) {
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                        // formContext.data.entity.save();
                        console.log("Dialog closed using OK button.");
                    }
                    else {
                        formContext.getAttribute("ccof_vcha").setValue(!saveVCHA);
                        executionContext.getEventArgs().preventDefault();
                    }
                });
        }
    }
};



function openCCFRIUnlockCustomPage(entityName, recordId, primaryControl) {
    debugger;
    var formContext = primaryControl;

    Xrm.Navigation.navigateTo({
        pageType: "custom",
        name: "ccof_ccfriunlockform_6430d",
        entityName: entityName,
        recordId: recordId,


    }, {
        target: 2,
        width: 1400,
        height: 900
    }
    )
        .then(function () {
            formContext.data.refresh();
        })
        .catch(console.error);
};
function showHideCreateUnlockButton(primaryControl) {
    debugger;
    var formContext = primaryControl;


    var visible = false;
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    userRoles.forEach(function hasRole(item, index) {
        if (item.name === "CCOF - Sr. Adjudicator" || item.name === "CCOF - QC" || item.name === "CCOF - Adjudicator" || item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "System Administrator") {
            visible = true;
        }
    });
    return visible;
};


