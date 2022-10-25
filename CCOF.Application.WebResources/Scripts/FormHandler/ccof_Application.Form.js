//the below function working only onLoad of the form
function onChangeOfApplicationType() {
    var formType = Xrm.Page.ui.getFormType();
    hideShow(formType);
}

//Make all fields read only on the application form
function hideShow(formTypeVal) {
    if (formTypeVal != 1) {
        Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(true);
    }
    else   //create
    {
        Xrm.Page.ui.tabs.get("tab_general").sections.get("fccp").setVisible(false);
    }
}