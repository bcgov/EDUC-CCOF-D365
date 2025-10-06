"use strict";

var CCOF = CCOF || {};
CCOF.FATemplate = CCOF.FATemplate || {};
CCOF.FATemplate.Ribbon = CCOF.FATemplate.Ribbon || {};

//Formload logic starts here
CCOF.FATemplate.Ribbon = {
    //A function called on save
    openCustomPageExisting: function (selectedItems, selectedControl) {
        var selectedRecord = selectedItems[0];
        var pageInput = {
            pageType: "custom",
            name: "ccof_managefatemplates_5d936",
            recordId: selectedRecord.Id.replace("{", "").replace("}", ""),
            entityName: selectedRecord.TypeName
        };
        var navigationOptions = {
            target: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    selectedControl.refresh();
                }
            ).catch(
                function (error) {
                    console.log(error);
                    selectedControl.refresh();
                }
            );
    },
    openCustomPageNew: function (selectedEntityName, primaryEntityName, firstPrimaryItemId, primaryControl, selectedControl) {
        var pageInput = {
            pageType: "custom",
            name: "ccof_managefatemplates_5d936",
            recordId: firstPrimaryItemId.replace("{", "").replace("}", ""),
            entityName: primaryEntityName
        };
        var navigationOptions = {
            target: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    selectedControl.refresh();
                }
            ).catch(
                function (error) {
                    console.log(error);
                    selectedControl.refresh();
                }
            );
    }
}
