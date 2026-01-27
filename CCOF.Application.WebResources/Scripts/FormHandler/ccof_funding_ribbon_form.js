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
    BulkApproveFA: function (selectedControl, selectedItemIds) {
        debugger;
        var gridContext = selectedControl;
        var pageInput = {
            pageType: "custom",
            name: "ccof_bulkapprovefundingagreements_a6eae",
            recordId: selectedItemIds
        };
        var navigationOptions = {
            target: 2,
            position: 1,
            height: 900,
            width: {value: 75, unit:"%"},
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    gridContext.refresh();
                    console.log("Success");
                }
            ).catch(
                function (error) {
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
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "CCOF - Super Awesome Mods Squad" || item.name === "CCOF - Mod QC" ||item.name === "System Administrator") {
                visible = true;
            }
        });

        //ACTIVE or APPROVED
        var showButton = (statusReason === 1 || statusReason === 101510001)
        if (!CreateMODButton.ready) return false; // default while loading
        return showButton && visible && CreateMODButton.noDrafted;
    },
    showHideGenerateFAPDF: function (primaryControl) {
        debugger;
        var formContext = primaryControl;
        var statusReason = formContext.getAttribute("statuscode").getValue();

        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" || item.name === "CCOF - Super Awesome Mods Squad" || item.name === "CCOF - Mod QC" ||item.name === "System Administrator") {
                visible = true;
            }
        });

        // "Drafted" | "Drafted – Provider Action Required" | "Drafted - with Ministry"      
        var showButton = (statusReason === 101510002 || statusReason === 101510003 || statusReason === 101510004)
        return showButton && visible;
    },
    showHideBulkApproveFA: function (selectedControl) {
        debugger;
        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Admin" || item.name === "CCOF - Leadership" ||item.name === "System Administrator") {
                visible = true;
            }
        });

        return visible;
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
    },
    filterFATemplate: function (executionContext) {
        
        var formContext = executionContext.getFormContext();

        formContext.getControl("ccof_fa_template_selected").addPreSearch(CCOF.Funding.Ribbon.addFATemplateFilter);
    },
    addFATemplateFilter: function (executionContext) {

        var formContext = executionContext.getFormContext();

        var orgID = formContext.getAttribute("ccof_organization_identifier")?.getValue();
        var programYear = formContext.getAttribute("ccof_programyear")?.getValue();
        var providerType = "";
        const string = '^G';
        const regexp = new RegExp(string);
        if (regexp.test(orgID)) {

            providerType = "100000000";
        }
        else {
            providerType = "100000001";
        }

        var filter = `<filter type="and">
            <condition attribute="ccof_providertype" operator="eq" value="${providerType}" />
            <condition attribute="ccof_program_year" operator="eq" value="${programYear[0]?.id}" />
        </filter >`;
        formContext.getControl("ccof_fa_template_selected")?.addCustomFilter(filter, "ccof_funding_agreement_template");
    }
}