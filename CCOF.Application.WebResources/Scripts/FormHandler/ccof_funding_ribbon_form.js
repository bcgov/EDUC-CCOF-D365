"use strict";

// module/global cache
var CreateMODButton = { noDrafted: false, ready: false };
var CCOF = CCOF || {};
CCOF.Funding = CCOF.Funding || {};
CCOF.Funding.Ribbon = CCOF.Funding.Ribbon || {};

//Formload logic starts here
CCOF.Funding.Ribbon = {

    //A function called on save
    CreateMOD: function (primaryControl, recordId) {
        debugger;
        var formContext = primaryControl;
        //var applicationID = formContext.getAttribute("ccof_facility").getValue();
        var pageInput = {
            pageType: "custom",
            name: "ccof_ccoffundingmod_8f814",
            recordId: recordId.replace("{", "").replace("}", "")
        };
        var navigationOptions = {
            target: 2,
            position: 1,
            height: 1200,
            width: 1500,
            title: "Funding Modification"
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    formContext.data.refresh();
                    console.log("Success");
                }
            ).catch(
                function (error) {
                    console.log(Error);
                }
            );
    },
    GenerateFundingPDF: function (primaryControl) {
        var formContext = primaryControl;
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
        var window_width = 400;
        var window_height = 300;
        var pageInput = {
            pageType: "custom",
            name: "ccof_generatefundingagreementpdf_efe89",
            entityName: "ccof_funding_agreement",
            recordId: recordId,
        };
        var navigationOptions = {
            target: 2,
            width: window_width,
            height: window_height
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    setTimeout(function () {
                        Xrm.Navigation.openForm({
                            entityName: formContext.data.entity.getEntityName(),
                            entityId: formContext.data.entity.getId()
                        });
                    }, 1000)

                }
            ).catch(
                function () {
                    console.log(Error);
                }
            );
    },

    showHideCreateMOD: function (primaryControl) {
        debugger;
        var formContext = primaryControl;
        var statusReason = formContext.getAttribute("statuscode").getValue();

        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "System Administrator") {
                visible = true;
            }
        });

        //ACTIVE or APPROVED
        var showButton = (statusReason === 1 || statusReason === 101510001)
        if (!CreateMODButton.ready) return false; // default while loading
        return showButton && visible && CreateMODButton.noDrafted;
    },

    initCreateMODButton: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var organizationId = formContext.getAttribute("ccof_organization").getValue()[0].id.replace('{', '').replace('}', '');
        var programYearId = formContext.getAttribute("ccof_programyear").getValue()[0].id.replace('{', '').replace('}', '');
        var query = `?$select=ccof_funding_agreementid&$filter=_ccof_programyear_value eq ${programYearId} and _ccof_organization_value eq ${organizationId} and Microsoft.Dynamics.CRM.In(PropertyName='statuscode',PropertyValues=['101510002','101510003','101510004'])&$orderby=_ccof_programyear_value desc,ccof_version desc`;

        Xrm.WebApi.retrieveMultipleRecords("ccof_funding_agreement", query).then(function (res) {
            CreateMODButton.noDrafted = (res.entities.length === 0);
            CreateMODButton.ready = true;
            formContext.ui.refreshRibbon();  // re-evaluate enable/display rules
        }).catch(function (err) {
            console.error("FA fetch failed:", err);
            CreateMODButton.noDrafted = false;
            CreateMODButton.ready = true;
            formContext.ui.refreshRibbon();
        });
    }
}