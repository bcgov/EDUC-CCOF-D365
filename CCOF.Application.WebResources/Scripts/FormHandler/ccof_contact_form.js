var Contact = Contact || {};
Contact.BCeid = Contact.BCeid || {};
Contact.BCeid.Form = Contact.BCeid.Form || {};

Contact.BCeid.Form = {
    getTypeOfForm: function () {
        debugger;

        var UserName = Xrm.Page.getAttribute("ccof_username").getValue(); //Get user name  on the contact form, If user name present, display bceid form other wise contact only.
        console.log("UserName" + UserName);
        var lblForm;
        if (UserName != null) {
            lblForm = "BCeID";
        }
        else {
            lblForm = "CCOF Contact Only";
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
}