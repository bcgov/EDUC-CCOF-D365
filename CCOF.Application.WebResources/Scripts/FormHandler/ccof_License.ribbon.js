"use strict";

var CCOF = CCOF || {};
CCOF.License = CCOF.License || {};
CCOF.License.Ribbon = CCOF.License.Ribbon || {};

//Formload logic starts here
CCOF.License.Ribbon = {
    //A function called on save
    openCustomPage: function (primaryControl) {
        formContext.data.save();
        var formContext = primaryControl;
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
        var window_width = 400;
        var window_height = 300;
        var pageInput = {
            pageType: "custom",
            name: "ccof_createlicenseversion_1e373",
            recordId: recordId
        };
        var navigationOptions = {
            target: 2,
            width: window_width,
            height: window_height
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    console.log("Page closed");
                }
            ).catch(
                function (error) {
                    console.log(error);
                    selectedControl.refresh();
                }
            );
    }
}
