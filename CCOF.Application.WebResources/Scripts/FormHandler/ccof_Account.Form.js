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
        switch (formContext.ui.getFormType()) {
            case 0: //undefined
                break;
            case 1: //Create/QuickCreate

            case 2: // update                           
                this.getTypeOfForm();
				this.setFilterXml_ParentFeeGrid(executionContext);				
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
            lblForm = "Organization Information";
        }
        else {
            lblForm = "Facility Information";
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
			"	<order attribute='ccof_programyearnumber' descending='true' />",
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
			function(results) {
				var FilteredList_ProgramYear = results.entities.map(
					function(r) { 
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
					"	  <condition attribute='ccof_facility' operator='eq' value='",currentRecordId, "uitype='account' />",
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
					"	  <condition attribute='ccof_facility' operator='eq' value='",currentRecordId, "uitype='account' />",
					"	</filter>",
					"  </entity>",
					"</fetch>"
				].join("");				

				subgrid_B.setFilterXml(fetchXml_B);
				subgrid_B.refresh();
				
			}, 
			Xrm.Navigation.openErrorDialog);
    }

};
