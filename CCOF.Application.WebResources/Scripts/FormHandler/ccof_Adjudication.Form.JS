//show and hide checklist section
function checklist() {
    debugger;

    var newProviderValue = Xrm.Page.getAttribute("ccof_newprovider").getValue();
    if (newProviderValue == false)//No-100,000,000
    {
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("RenewChecklistSection").setVisible(true);
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("NewChecklistSection").setVisible(false);
    }
    else if (newProviderValue == true)  //Yes  -100000001
    {
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("NewChecklistSection").setVisible(true);
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("RenewChecklistSection").setVisible(false);
    }
    else {
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("NewChecklistSection").setVisible(false);
        Xrm.Page.ui.tabs.get("ADJ_CCOF_tab").sections.get("RenewChecklistSection").setVisible(false);

    }
}
