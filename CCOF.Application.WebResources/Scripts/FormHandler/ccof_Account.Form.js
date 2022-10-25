// A webresource for BaseFunding only

//Create Namespace Object if its defined
var Account = Account || {};
Account.OrgFacility = Account.OrgFacility || {};
Account.OrgFacility.Form = Account.OrgFacility.Form || {};

//Formload logic starts here
Account.OrgFacility.Form = {
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
        var typeOfInfo = Xrm.Page.getAttribute("ccof_accounttype").getValue();  //Business Type is two options feild - Facility - 100,000,001(True) and Orgabization - 100,000,000(False)
        console.log("typeOfInfo" + typeOfInfo);
        var lblForm;
        if (typeOfInfo == 100000000) {
            lblForm = "Organization Information";
        }
        else {
            lblForm = "Facility Information";
        }

        // Current form's label
        var formLabel = Xrm.Page.ui.formSelector.getCurrentItem().getLabel();
        console.log("lblForm " + lblForm);
        console.log("CFL " + formLabel);
        //check if the current form is form need to be displayed based on the value
        if (Xrm.Page.ui.formSelector.getCurrentItem().getLabel() != lblForm) {
            var items = Xrm.Page.ui.formSelector.items.get();
            for (var i in items) {
                var item = items[i];
                var itemId = item.getId();
                var itemLabel = item.getLabel()
                if (itemLabel == lblForm) {
                    //Check the current form is the same form to be redirected.
                    if (itemLabel != formLabel) {
                        //navigate to the form
                        item.navigate();
                    } //endif
                }//endif
            } //end for
        }
    }
};