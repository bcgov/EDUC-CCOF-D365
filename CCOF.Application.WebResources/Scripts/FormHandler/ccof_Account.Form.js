// A webresource for AccountForm only

//Create Namespace Object if its defined
if (typeof (CCOF) === "undefined") {
    CCOF = {};
} else {

}

//Formload logic starts here
CCOF.Account.Form = {
    onLoad: function (executionContext) {
        let formContext = executionContext.getFormContext();
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate

            case 2: // update
               
                break;
            case 3: //readonly
                break;
            case 4: //disable
                break;
            case 6: //bulkedit
                break;
        }
    },

        let formContext = executionContext.getFormContext();
        const currentdate = new Date();
        formContext.getAttribute(destinationColumns).setValue(currentdate);
        formContext.data.entity.save();

    },
    //A function called on save
    onSave: function (executionContext) {
       
    }
};