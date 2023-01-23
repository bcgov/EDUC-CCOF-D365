
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
