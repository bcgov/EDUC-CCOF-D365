"use strict";

var Account = Account || {};
Account.OrgFacility = Account.OrgFacility || {};
Account.OrgFacility.Ribbon = Account.OrgFacility.Ribbon || {};

Account.OrgFacility.Ribbon = {
    ShowFlagOnlyforOverviewForm: function (primaryControl) {
        try {
            debugger;
            // If not on a form, fail safe (return true to avoid hiding everywhere)
            if (!primaryControl || !primaryControl.ui || !primaryControl.ui.formSelector) {
                return true;
            }
            var formId = primaryControl.ui.formSelector.getCurrentItem().getId();
            return formId.toLowerCase() === "afea03af-62c6-f011-bbd3-7c1e52063ca6"; // Organization Overview form of Account 
        } catch (e) {
            // On any error, do not block the UI; default to showing/enabling
            return true;
        }
    },
    hideSub: function (primaryControl) {
        try {
            debugger;

            // If not on a form, fail safe (return true to avoid hiding everywhere)
            if (!primaryControl || !primaryControl.ui || !primaryControl.ui.formSelector) {
                return true;
            }

            var formSelector = primaryControl.ui.formSelector;
            var currentItem = formSelector.getCurrentItem && formSelector.getCurrentItem();
            if (!currentItem) {
                return true;
            }

            var label = currentItem.getLabel && currentItem.getLabel();
            // If on "Organization Overview" form → return false to HIDE / DISABLE (depending on rule type)
            if (label === "Organization Overview") {
                return false;
            }
            return true;
        } catch (e) {
            // On any error, do not block the UI; default to showing/enabling
            return true;
        }
    },
    openNewChildFromParent: function (primaryControl) {
        debugger;
        var formContext = primaryControl;
        var parentId = formContext.data.entity.getId().replace("{", "").replace("}", "");
        var formLabel = formContext.ui.formSelector.getCurrentItem().getLabel();
        var pageName;

        if (!parentId) {
            Xrm.Navigation.openAlertDialog({ text: "No parent record loaded." });
            return;
        }

        var createFromEntity = {
            entityType: formContext.data.entity.getEntityName(), // e.g., "account"
            id: parentId.replace(/[{}]/g, ""),
            name: formContext.getAttribute("name")?.getValue() || "" // optional
        };

        var navigationOptions = {
            target: 2,          // 2 = open as dialog
            position: 1,        // 1 = center
            width: { value: 900, unit: "px" },  // or { value: 60, unit: "%" }
            height: { value: 650, unit: "px" },
            // allowBack: false  // optional: remove back button
        };


        var entityFormOptions = {
            pageType: "entityrecord",
            entityName: "ccof_notesflag", // child entity
            // useQuickCreateForm: true, // optional
            formId: "d78fb4fa-2503-f111-8407-7ced8d050b1a",
            cmdbar: true,   // keep command bar so Save/Close remain visible
            navbar: "off",
            createFromEntity: createFromEntity,
        };


        Xrm.Navigation.navigateTo(entityFormOptions, navigationOptions).then(
            function (result) {
                // result is undefined when closed; you can optionally refresh a subgrid here
                // e.g., formContext.getControl("Subgrid_NotesFlags")?.refresh();
            },
            function (error) {
                console.error("navigateTo (dialog) error:", error && error.message ? error.message : error);
            }
        );
    }
}
