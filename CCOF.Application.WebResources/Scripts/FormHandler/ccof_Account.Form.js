// A webresource for BaseFunding only

//Create Namespace Object if its defined
var Account = Account || {};
Account.OrgFacility = Account.OrgFacility || {};
Account.OrgFacility.Form = Account.OrgFacility.Form || {};

//Formload logic starts here
Account.OrgFacility.Form = {
    onLoad: function (executionContext) {
        debugger;
        let formContext = executionContext.getFormContext();
        var webResourceNames = ["WebResource_mailing_address", "WebResource_street_address", "WebResource_facility_address"];
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate
                webResourceNames.forEach(function (item, index) {
                    Account.OrgFacility.Form.passFormContextToHTML(formContext, item);
                });
                this.disableHTMLWebResource(executionContext);
            case 2: // update                           
                this.getTypeOfForm();
                this.setFilterXml_ParentFeeGrid(executionContext);
                webResourceNames.forEach(function (item, index) {
                    Account.OrgFacility.Form.passFormContextToHTML(formContext, item);
                });
                this.disableHTMLWebResource(executionContext);
                this.showHidePartnershipTab(executionContext);
                //   Delay to allow subgrid to load
                setTimeout(function () {
                    var subgrid = formContext.getControl("Subgrid_FacStatus");
                    if (subgrid) {
                        Account.OrgFacility.Form.setFilterXmlFacility(executionContext);
                        subgrid.setShowCommandBar(false);
                    }
                }, 2000); // adjust delay as needed

                break;
            case 3: //readonly
                break;
            case 4: //disable
                break;
            case 6: //bulkedit
                break;
        }
    },


    //A function called on save
    onSave: function (executionContext) {

    },



    getTypeOfForm: function () {
        debugger;
        var typeOfInfo = Xrm.Page.getAttribute("ccof_accounttype").getValue();  //Business Type is two options feild - Facility - 100,000,001(True) and Orgabization - 100,000,000(False)
        console.log("typeOfInfo" + typeOfInfo);
        var lblForm;
        if (typeOfInfo == 100000000) {
            allowedForms = ["Organization Information", "Organization Overview"];

            // lblForm = "Organization Information";
        }
        else {
            allowedForms = ["Facility Information"];
            // lblForm = "Facility Information";
        }

        // Current form's label
        var formLabel = Xrm.Page.ui.formSelector.getCurrentItem().getLabel();
        console.log("Allowed Forms: " + allowedForms);
        console.log("Current Form: " + formLabel);

        //check if the current form is form need to be displayed based on the value
        // if (Xrm.Page.ui.formSelector.getCurrentItem().getLabel() != lblForm) {
        if (allowedForms.indexOf(formLabel) === -1) {
            var items = Xrm.Page.ui.formSelector.items.get();
            for (var i in items) {
                var item = items[i];
                var itemId = item.getId();
                var itemLabel = item.getLabel()
                //if (itemLabel == lblForm) {
                //Check the current form is the same form to be redirected.
                //  if (itemLabel != formLabel) {
                if (allowedForms.indexOf(itemLabel) !== -1) {
                    //navigate to the form
                    item.navigate();
                    break;
                } //endif
                //}//endif
            } //end for
        }
    },


    setFilterXml_ParentFeeGrid: function (executionContext) {

        var formContext = executionContext.getFormContext();
        var subgrid_A = formContext.getControl("Subgrid_EstimatorParentFees");
        var subgrid_B = formContext.getControl("Subgrid_InternalParentFeesHistory");

        var currentRecordId = formContext.data.entity.getId();

        //set up the query to retrieve record IDs of Program Year entity (for last 2 Fiscal)
        var programYearFetchXml = [
            "<fetch top='3' version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
            " <entity name='ccof_program_year'>",
            "	<attribute name='ccof_program_yearid' />",
            "	<attribute name='ccof_name' />",
            "	<attribute name='statuscode' />",
            "	<attribute name='ccof_previousyear' />",
            "	<attribute name='ccof_intakeperiodstart' />",
            "	<attribute name='ccof_programyearnumber' />",
            "	<order attribute='ccof_name' descending='true' />",
            "	<filter type='and'>",
            "	  <condition attribute='statecode' operator='eq' value='0' />",
            "	  <condition attribute='statuscode' operator='in'>",
            "		<value>1</value>",
            "		<value>3</value>",
            "		<value>4</value>",
            "	  </condition>",
            "	</filter>",
            "  </entity>",
            "</fetch>"
        ].join("");

        //query the records of Program Year
        Xrm.WebApi.retrieveMultipleRecords("ccof_program_year", "?fetchXml=" + encodeURIComponent(programYearFetchXml)).then(
            function (results) {
                var FilteredList_ProgramYear = results.entities.map(
                    function (r) {
                        return ("<value>" + r.ccof_program_yearid + "</value>");
                    }).join("");

                //compose query and refresh the grid - "Estimator Parent Fees"
                var fetchXml_A = [
                    "<fetch version='1.0' mapping='logical' distinct='true' no-lock='false' >",
                    "  <entity name='ccof_parent_fees' >",
                    "	<filter type='and' >",
                    "	  <condition attribute='statecode' operator='eq' value='0' />",
                    "	  <condition attribute='ccof_availability' operator='in'>",
                    "		 <value>100000001</value>",
                    "		 <value>100000002</value>",
                    "	  </condition>",
                    "	  <condition attribute='statuscode' operator='eq' value='1' />",
                    "     <condition attribute='ccof_programyear' operator='in' uitype='ccof_program_year' >",
                    FilteredList_ProgramYear,
                    "     </condition>",
                    "	  <condition attribute='ccof_facility' operator='eq' value='", currentRecordId, "uitype='account' />",
                    "	</filter>",
                    "  </entity>",
                    "</fetch>"
                ].join("");

                subgrid_A.setFilterXml(fetchXml_A);
                subgrid_A.refresh();

                //compose query and refresh the grid - "Internal Parent Fees History"
                var fetchXml_B = [
                    "<fetch version='1.0' mapping='logical' distinct='true' no-lock='false' >",
                    "  <entity name='ccof_parent_fees' >",
                    "	<filter type='and' >",
                    "	  <condition attribute='statecode' operator='eq' value='0' />",
                    "	  <condition attribute='ccof_availability' operator='ne' value='100000001' />",
                    "	  <condition attribute='statuscode' operator='eq' value='1' />",
                    "     <condition attribute='ccof_programyear' operator='in' uitype='ccof_program_year' >",
                    FilteredList_ProgramYear,
                    "     </condition>",
                    "	  <condition attribute='ccof_facility' operator='eq' value='", currentRecordId, "uitype='account' />",
                    "	</filter>",
                    "  </entity>",
                    "</fetch>"
                ].join("");

                subgrid_B.setFilterXml(fetchXml_B);
                subgrid_B.refresh();

            },
            Xrm.Navigation.openErrorDialog);
    },

    passFormContextToHTML: function (formContext, webResourceName) {
        let addressControl = formContext.getControl(webResourceName);
        if (addressControl != null && addressControl != undefined) {
            addressControl.getContentWindow().then(
                function (contentWindow) {
                    contentWindow.setClientApiContext(Xrm, formContext, webResourceName);
                }
            )
        }
    },
    showHidePartnershipTab: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var orgType = formContext.getAttribute("ccof_typeoforganization");
        if (orgType) {
            var orgTypeValue = orgType.getValue();
            var partnershipTab = formContext.ui.tabs.get("tab_partnershipinfo");
            if (orgTypeValue === 100000006) {
                partnershipTab.setVisible(true);
            }
            else {
                partnershipTab.setVisible(false);
            }

        }

    },

    disableHTMLWebResource: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var mailingAddressManual = formContext.getAttribute('ccof_is_org_mailing_address_entered_manually');
        if (mailingAddressManual != null) {
            if (mailingAddressManual.getValue()) {
                var webResArea = formContext.ui.controls.get('WebResource_mailing_address');
                webResArea.setVisible(false);
            }
            else {
                var webResArea = formContext.ui.controls.get('WebResource_mailing_address');
                webResArea.setVisible(true);
                this.passFormContextToHTML(formContext, 'WebResource_mailing_address');
            }
        }
        var streetAddressManual = formContext.getAttribute('ccof_is_org_street_address_entered_manually');
        if (streetAddressManual != null) {
            if (streetAddressManual.getValue()) {
                var webResArea = formContext.ui.controls.get('WebResource_street_address');
                webResArea.setVisible(false);
            }
            else {
                var webResArea = formContext.ui.controls.get('WebResource_street_address');
                webResArea.setVisible(true);
                this.passFormContextToHTML(formContext, 'WebResource_street_address');
            }
        }
        var facilityAddressManual = formContext.getAttribute('ccof_is_facility_address_entered_manually');
        if (facilityAddressManual != null) {
            if (facilityAddressManual.getValue()) {
                var webResArea = formContext.ui.controls.get('WebResource_facility_address');
                webResArea.setVisible(false);
            }
            else {
                var webResArea = formContext.ui.controls.get('WebResource_facility_address');
                webResArea.setVisible(true);
                this.passFormContextToHTML(formContext, 'WebResource_facility_address');
            }
        }
    },
    setFilterXmlFacility: function (executionContext) {

        var formContext = executionContext.getFormContext();
        var subgrid = formContext.getControl("Subgrid_FacStatus");
        //var subgrid_B = formContext.getControl("Subgrid_InternalParentFeesHistory");

        var currentRecordId = formContext.data.entity.getId();


        var FilteredList_ProgramYear = '';
        //query the records of Program Year
        var programYearFetchXml = "<fetch top='3' version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
            " <entity name='ccof_program_year'>" +
            "<attribute name='ccof_program_yearid'/>" +
            "<attribute name='ccof_name'/>" +
            "<attribute name='statuscode'/>" +
            "<attribute name='ccof_previousyear'/>" +
            "<attribute name='ccof_intakeperiodstart'/>" +
            "<attribute name='ccof_programyearnumber'/>" +
            "<order attribute='ccof_name' descending='true'/>" +
            "<filter type='and'>" +
            "<condition attribute='statecode' operator='eq' value='0'/>" +
            "<condition attribute='statuscode' operator='in'>" +
            "<value>1</value>" +
            "<value>3</value>" +
            "<value>4</value>" +
            "</condition>" +
            "</filter>" +
            " </entity>" +
            "</fetch>";
        Xrm.WebApi.retrieveMultipleRecords("ccof_program_year", "?fetchXml=" + encodeURIComponent(programYearFetchXml)).then(
            function (results) {
                FilteredList_ProgramYear = results.entities.map(
                    function (r) {
                        return ("<value>" + r.ccof_program_yearid + "</value>");
                    }).join("")

                // GUID of your custom view (from Advanced Find or Saved Query)
                var ECEWEFetchXml = subgrid.getFetchXml();
                var facilityid = "";
                Xrm.WebApi.retrieveMultipleRecords("account", "?$select=accountid&$filter=(_parentaccountid_value eq " + currentRecordId + ") and ccof_facilitystatus ne 100000009").then(
                    function success(result) {
                        console.log("Retrieved records: " + result.entities.length);
                        for (var i = 0; i < result.entities.length; i++) {
                            // Append each accountid wrapped in <value> tags
                            facilityid += "<value>" + result.entities[i].accountid + "</value>";
                        }
                        var ECEWEFetchXml = ""
                        if (facilityid) {
                            ECEWEFetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ccof_adjudication_ecewe_facility'>" +
                                "<attribute name='ccof_facility'/>" +
                                "<attribute name='ccof_facilityid'/>" +
                                "<attribute name='ccof_adjudication_ecewe_facilityid'/>" +
                                "<attribute name='ccof_name'/>" +
                                "<attribute name='statuscode'/>" +
                                "<order attribute='ccof_name' descending='false'/>" +
                                "<filter type='and'>" +
                                "<condition  attribute='ccof_facility' operator='in'>" +
                                facilityid +
                                "</condition>" +
                                "</filter>" +
                                "<link-entity name='ccof_adjudication_ccfri_facility' from='ccof_adjudication_ccfri_facilityid' to='ccof_adjudicationccfrifacility' visible='false' link-type='outer' alias='CCFRI'>" +
                                "<attribute name='statuscode'/>" +
                                "<attribute name='ccof_programyear'/>" +
                                "<filter>" +
                                "<condition attribute='ccof_programyear' operator='in' uitype='ccof_program_year'>" +
                                FilteredList_ProgramYear +
                                "</condition>" +
                                "</filter>" +
                                "</link-entity>" +
                                "</entity>" +
                                "</fetch>";
                        }
                        else {

                            ECEWEFetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ccof_adjudication_ecewe_facility'>" +
                                "<attribute name='ccof_facility'/>" +
                                "<attribute name='ccof_facilityid'/>" +
                                "<attribute name='ccof_adjudication_ecewe_facilityid'/>" +
                                "<attribute name='ccof_name'/>" +
                                "<attribute name='statuscode'/>" +
                                "<order attribute='ccof_name' descending='false'/>" +
                                "<filter type='and'>" +
                                "<condition  attribute='ccof_facility' operator='in'>" +
                                "<value> 00000000-0000-0000-0000-000000000000 </value>" +
                                "</condition>" +
                                "</filter>" +
                                "<link-entity name='ccof_adjudication_ccfri_facility' from='ccof_adjudication_ccfri_facilityid' to='ccof_adjudicationccfrifacility' visible='false' link-type='outer' alias='CCFRI'>" +
                                "<attribute name='statuscode'/>" +
                                "</link-entity>" +
                                "<link-entity name='ccof_application' from='ccof_applicationid' to='ccof_areyouapublicsectoremployer' visible='false' link-type='outer' alias='CCOF'>" +
                                "<attribute name='ccof_ccofstatus'/>" +
                                "<attribute name='ccof_programyear'/>" +
                                "</link-entity>" +
                                "</entity>" +
                                "</fetch>";
                        }

                        subgrid.setFilterXml(ECEWEFetchXml);

                        subgrid.refresh();

                        result.entities.forEach(function (entity) {
                            console.log("Account Name: " + entity.name + " | Account Number: " + entity.accountnumber);
                        });
                    },
                    function (error) {
                        console.error("Error retrieving records: " + error.message);
                    });
            }, function (error) {
                console.error("Error retrieving records: " + error.message);
            }
        );


        // FetchXML for the custom view



    },

    redirectCustomPage: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var gridContext = executionContext.getEventSource()//formContext.getControl("Subgrid_FacStatus"); // your grid name
        if (!gridContext) {
            // retry until grid exists
            setTimeout(function () {
                this.redirectCustomPage(executionContext);
            }, 500);
            return;
        }
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
        var entityName = executionContext.getFormContext().data.entity.getEntityName();


        // var recordId = gridContext._entityId.guid.replace(/[{}]/g, "");//(entity && entity.getId && entity.getId()) ? entity.getId().replace(/[{}]/g, "") : null;
        if (!recordId) {
            console.warn("Could not resolve selected row ID.");
            return;
        }

        // 🔎 Retrieve the lookup properly using _<schema>_value
        // If the schema name is ccof_facility, the Web API property is _ccof_facility_value
        Xrm.WebApi.retrieveRecord(
            entityName,
            recordId,
            "?$select=_ccof_facility_value"
        ).then(function (res) {
            var facId = res && res["_ccof_facility_value"];
            if (!facId) {
                console.log("No related facility found on record:", recordId);
                return;
            }

            var opts = {
                entityName: "account",
                entityId: facId.replace(/[{}]/g, ""),
                formId: "2011ec70-afcd-457f-b297-331acdb00437" // <-- your target form GUID
                // , openInNewWindow: true   // optional
            };

            return Xrm.Navigation.openForm(opts);
        }).then(function () {
            // opened successfully
        }).catch(function (err) {
            console.error("retrieve/open error:", err && err.message ? err.message : err);
        });

    }
    //gridContext.addOnLoad(function () {
    //    var selectedRows = gridContext.getGrid().getSelectedRows();

    //    selectedRows.forEach(function (row) {
    //        var recordId = row.getData().getEntity().getId().replace("{", "").replace("}", "");
    //        Xrm.WebApi.retrieveRecord("ccof_adjudication_ecewe_facility", recordId, "?$select=_ccof_facility_value").then(
    //            function success(result) {
    //                console.log(result);
    //                // Columns
    //                //var ccof_adjudication_ecewe_facilityid = result["ccof_adjudication_ecewe_facilityid"]; // Guid
    //                //var ccof_facility = result["_ccof_facility_value"]; // Lookup
    //                //var ccof_facility_formatted = result["_ccof_facility_value@OData.Community.Display.V1.FormattedValue"];
    //                //var ccof_facility_lookuplogicalname = result["_ccof_facility_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
    //                if (result["_ccof_facility_value"] && result["_ccof_facility_value"]) {
    //                    var facid = result["_ccof_facility_value"].replace("{", "").replace("}", "");

    //                    var entityFormOptions = {
    //                        entityName: "account", // target entity
    //                        entityId: facid,
    //                        formId: "2011ec70-afcd-457f-b297-331acdb00437" // target form GUID
    //                    };
    //                    Xrm.Navigation.openForm(entityFormOptions).then(
    //                        function (success) {
    //                            console.log("Form opened successfully:", success);
    //                        },
    //                        function (error) {
    //                            console.error("Error opening form:", error);
    //                        }
    //                    );
    //                }
    //            },
    //            function (error) {
    //                console.log(error.message);
    //            }
    //        );
    //    });
    //});
    //   }
};// JavaScript source code
