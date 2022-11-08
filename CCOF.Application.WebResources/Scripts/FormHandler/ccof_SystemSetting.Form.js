// A webresource for BaseFunding only

//Create Namespace Object if its defined
var SystemSetting = SystemSetting || {};
SystemSetting.SS = SystemSetting.SS || {};
SystemSetting.SS.Form = SystemSetting.SS.Form || {};

//Formload logic starts here
SystemSetting.SS.Form = {
    onSave: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate

            case 2: // update                           
                this.getTypeOfForm();
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
    onLoad: function (executionContext){

    },

    getTypeOfForm: function () {
        debugger;
        var currentDate = new Date();
        var switchToBcSsaChapterVal = Xrm.Page.getAttribute("ccof_switchtobcssachapter").getValue();  //Two options feild - Yes - 1(True) and No - 0(False)
        console.log("switchToBcSsaChapterVal" + switchToBcSsaChapterVal);
        var confirmStrings = { text: "You have updated a system setting, are you sure you want to proceed?", title: "Confirmation Dialog" };
        var confirmOptions = { height: 200, width: 450 };
        var dateVal = Xrm.Page.getAttribute("ccof_bcssachapterdate").getValue();
        //if ((switchToBcSsaChapterVal == true) && (dateVal == null || dateVal == undefined)){
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                console.log("Success" + success);
                if (success.confirmed) {
                    console.log("Dialog closed using OK button.");
                    if (switchToBcSsaChapterVal == true) {
                        console.log("ValTrue" + switchToBcSsaChapterVal);
                        Xrm.Page.getAttribute("ccof_bcssachapterdate").setValue(currentDate);
                        Xrm.Page.getControl('ccof_switchtobcssachapter').setDisabled(true);
                        //Xrm.Page.getControl('ccof_bcssachapterdate').setDisabled(true);
                        formContext.data.entity.save();
                    }
                    else if (switchToBcSsaChapterVal == false) {
                        console.log("ValFalse" + switchToBcSsaChapterVal);
                        Xrm.Page.getAttribute("ccof_bcssachapterdate").setValue(null);
                        Xrm.Page.getControl('ccof_switchtobcssachapter').setDisabled(false);
                        //Xrm.Page.getControl('ccof_bcssachapterdate').setDisabled(false);
                    }
                }
                else {
                    console.log("Dialog closed using Cancel button or X.");
                    console.log("switchToBcSsaChapVal" + switchToBcSsaChapVal);
                    if (switchToBcSsaChapterVal == true) {
                        console.log("Val" + switchToBcSsaChapterVal);
                        Xrm.Page.getAttribute("ccof_switchtobcssachapter").setValue(true);
                    }
                    else if (switchToBcSsaChapterVal == false) {
                        console.log("Val" + switchToBcSsaChapterVal);
                        Xrm.Page.getAttribute("ccof_switchtobcssachapter").setValue(false);
                    }
                }
            });
        //}
    }
};


function switchToBcSsaChapValReadOnly() //OnLoad of Form
{
    var switchToBcSsaChapter = Xrm.Page.getAttribute("ccof_switchtobcssachapter").getValue();
    var date = Xrm.Page.getAttribute("ccof_bcssachapterdate").getValue();
    if ((switchToBcSsaChapter == true) && (date != null || date != undefined)) {
        Xrm.Page.getControl('ccof_switchtobcssachapter').setDisabled(true);
    }
    else {
        Xrm.Page.getControl('ccof_switchtobcssachapter').setDisabled(false);
    }
}