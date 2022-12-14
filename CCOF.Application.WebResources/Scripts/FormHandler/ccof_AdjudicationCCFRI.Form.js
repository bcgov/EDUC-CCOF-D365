
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

    //A function called on save
    onLoad: function (executionContext) {
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
