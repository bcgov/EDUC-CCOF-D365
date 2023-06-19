// A webresource for BaseFunding only

//Create Namespace Object if its defined
var CCOF = CCOF || {};
CCOF.BaseFunding = CCOF.BaseFunding || {};
CCOF.BaseFunding.Form = CCOF.BaseFunding.Form || {};

//Formload logic starts here
CCOF.BaseFunding.Form = {
    onLoad: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate

            case 2: // update                           
                this.hideShow();
                break;
            case 3: //readonly
                this.hideShow();
                break;
            case 4: //disable
                break;
            case 6: //bulkedit
                break;
        }
    },


    //A function called on save
    onSave: function (executionContext) {

    },

    hideShow: function () {
        debugger;
        var typeOfApp = Xrm.Page.getAttribute("ccof_providertype");
        if (typeOfApp != null) {
            //Get OptionSet Text
            typeOfAppText = typeOfApp.getText();
            console.log(typeOfAppText);
            //Get OptionSet Val
            typeOfAppValue = typeOfApp.getValue();
            console.log(typeOfAppValue);
        }

        //hide and show section based on the Type of Application field value
        if (typeOfAppValue == 100000001)  //Family Child Care Provider - 100000001
        {
            Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("License_Info").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("Preschool").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("GCC_Information").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("MaxSpace_forExtendedHours").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("gccp").setVisible(false);
        }
        else if (typeOfAppValue == 100000000)  //Group Child Care Provider - 100000000
        {
            Xrm.Page.ui.tabs.get("tab_general").sections.get("gccp").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("License_Info").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("Preschool").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("GCC_Information").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("MaxSpace_forExtendedHours").setVisible(true);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(false);
        }
        else {
            Xrm.Page.ui.tabs.get("tab_general").sections.get("gccp").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("License_Info").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("Preschool").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("GCC_Information").setVisible(false);
            Xrm.Page.ui.tabs.get("tab_general").sections.get("MaxSpace_forExtendedHours").setVisible(false);
        }
    }
};