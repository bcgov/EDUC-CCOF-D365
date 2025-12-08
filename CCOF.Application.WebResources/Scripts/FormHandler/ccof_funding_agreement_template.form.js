"use strict";

var CCOF = CCOF || {};
CCOF.FATemplate = CCOF.FATemplate || {};
CCOF.FATemplate.Form = CCOF.FATemplate.Form || {};

//Formload logic starts here
CCOF.FATemplate.Form = {
    //A function called on save
    redirectCustomPage: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var pageInput = {
            pageType: "custom",
            name: "ccof_managefatemplates_5d936",
            recordId: formContext.data.entity?.getId().replace("{", "").replace("}", ""),
            entityName: "ccof_funding_agreement_template"
        };
        var navigationOptions = {
            target: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    formContext.entity.data.refresh();
                }
            ).catch(
                function (error) {
                    console.log(error);
                    formContext.entity.data.refresh();
                }
            );
    },
    openCustomPageExisting: function (formContext) {
        var pageInput = {
            pageType: "custom",
            name: "ccof_managefatemplates_5d936",
            recordId: formContext.data.entity?.getId().replace("{", "").replace("}", ""),
            entityName: "ccof_funding_agreement_template"
        };
        var navigationOptions = {
            target: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    formContext.entity.data.refresh();
                }
            ).catch(
                function (error) {
                    console.log(error);
                    formContext.entity.data.refresh();
                }
            );
    },
    openCustomPageNew: function (formContext) {
        var pageInput = {
            pageType: "custom",
            name: "ccof_managefatemplates_5d936",
            //recordId: firstPrimaryItemId.replace("{", "").replace("}", ""),
            entityName: "ccof_funding_agreement_template"
        };
        var navigationOptions = {
            target: 1
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    formContext.entity.data.refresh();
                }
            ).catch(
                function (error) {
                    console.log(error);
                    formContext.entity.data.refresh();
                }
            );
    }
}
