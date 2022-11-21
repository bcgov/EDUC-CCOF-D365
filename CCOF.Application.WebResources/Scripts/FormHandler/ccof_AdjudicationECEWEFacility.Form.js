// A webresource for BaseFunding only

//Create Namespace Object if its defined
var AdjECEWEFacility = AdjECEWEFacility || {};
AdjECEWEFacility.ECEWEFac = AdjECEWEFacility.ECEWEFac || {};
AdjECEWEFacility.ECEWEFac.Form = AdjECEWEFacility.ECEWEFac.Form || {};

//Formload logic starts here
AdjECEWEFacility.ECEWEFac.Form = {
    onLoad: function (executionContext) {
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
    onSave: function (executionContext) {

    },

    getTypeOfForm: function () {
        debugger;
        var optInSate = Xrm.Page.getAttribute("ccof_optinstartdate").getValue();
        console.log("optInSate " + optInSate);
        if (optInSate !== null) {
            var yearDOB = optInSate.getFullYear().toString();
            var monthDOB = (optInSate.getMonth() + 1);
            console.log("monthDOB " + monthDOB);
            var dayDOB = optInSate.getDate().toString();
            Xrm.Page.getAttribute("ccof_optinstartmonth").setValue(monthDOB);
            //formContext.getAttribute("ccof_optinstartmonth").setValue(monthDOB);
        }
        else { }
    }
};
