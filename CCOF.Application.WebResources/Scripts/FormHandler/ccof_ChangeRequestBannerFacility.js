function createStyle() {
    var customStyle = 'div.ms-MessageBar-content { background-color: #FFEB2A!important }';
    var css = document.createElement('style');
    css.type = 'text/css';
    css.innerHTML = customStyle;
    parent.document.head.appendChild(css);
}

function showNotification(executionContext, formContext, fetchXML, CCOFXML) {
    createStyle();
    Xrm.WebApi.retrieveMultipleRecords("ccof_change_request", fetchXML).then(
        function success(result) {
            for (var i = 0; i < result.entities.length; i++) {
                //if the status is Submitted, In Progress, With Provider, Eligible
                if (result.entities[i].statuscode == 3 || result.entities[i].statuscode == 4 || result.entities[i].statuscode == 5 || result.entities[i].statuscode == 9) {

                    //get the CCOF record
                    Xrm.WebApi.retrieveMultipleRecords("ccof_adjudication", CCOFXML).then(
                        function success(result) {
                            var ccof_id = result.entities[0].ccof_adjudicationid;
                            var appId = null;

                            var globalContext = Xrm.Utility.getGlobalContext();
                            var appurl = globalContext.getCurrentAppUrl();
                            globalContext.getCurrentAppProperties().then(
                                function success(app) {
                                    appId = app["appId"];
                                    console.log(appId);
                                },
                                function errorCallback() {
                                    console.log("Error");
                                });
                            var url = appurl + '&forceUCI=1&pagetype=entityrecord&etn=ccof_adjudication&id=' + ccof_id;
                            // var url = executionContext.getContext().getClientUrl() + '/main.aspx?' + appId + '&forceUCI=1&pagetype=entityrecord&etn=ccof_adjudication&id=' + ccof_id;
                            var notification_link = formContext.getAttribute("ccof_notification_link").getValue();
                            if (notification_link == null) {
                                formContext.getAttribute("ccof_notification_link").setValue(url);
                                formContext.data.entity.save();
                            }
                            formContext.ui.tabs.get("Summary").sections.get("Notification").setVisible(true);

                        },
                        function (error) {
                            console.log(error.message);
                            // handle error conditions
                        }
                    );
                    break;
                }
            }
        },
        function (error) {
            console.log(error.message);
        }
    )

}

function showNotificationCCFRIFacility(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var ccfriFacility = formContext.data.entity.getId().replace("{", "").replace("}", "");

    var fetchXML = `?fetchXml=<fetch>
    <entity name="ccof_change_request">
    <link-entity name="ccof_application" from="ccof_applicationid" to="ccof_application">
      <link-entity name="ccof_adjudication_ccfri_facility" from="ccof_application" to="ccof_applicationid" link-type="inner">
        <filter>
          <condition attribute="ccof_adjudication_ccfri_facilityid" operator="eq" value="${ccfriFacility}" />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
  </fetch>`

    var CCOFXML = `?fetchXml=<fetch>
  <entity name="ccof_adjudication">
    <link-entity name="ccof_adjudication_ccfri" from="ccof_adjudication" to="ccof_adjudicationid" link-type="inner">
      <link-entity name="ccof_adjudication_ccfri_facility" from="ccof_adjudicationccfri" to="ccof_adjudication_ccfriid">
        <filter>
          <condition attribute="ccof_adjudication_ccfri_facilityid" operator="eq" value="${ccfriFacility}" />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>`

    showNotification(executionContext, formContext, fetchXML, CCOFXML);
}


function showNotificationECEWEFacility(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var eceweFacility = formContext.data.entity.getId().replace("{", "").replace("}", "");

    var fetchXML = `?fetchXml=<fetch>
    <entity name="ccof_change_request">
    <link-entity name="ccof_application" from="ccof_applicationid" to="ccof_application" link-type="inner">
      <link-entity name="ccof_adjudication_ecewe" from="ccof_application" to="ccof_applicationid">
        <link-entity name="ccof_adjudication_ecewe_facility" from="ccof_adjudication_ecewe" to="ccof_adjudication_eceweid">
          <filter>
            <condition attribute="ccof_adjudication_ecewe_facilityid" operator="eq" value="${eceweFacility}" />
          </filter>
        </link-entity>
      </link-entity>
    </link-entity>
  </entity>
  </fetch>`

    var CCOFXML = `?fetchXml=<fetch>
  <entity name="ccof_adjudication">
    <link-entity name="ccof_adjudication_ecewe" from="ccof_adjudication" to="ccof_adjudicationid">
      <link-entity name="ccof_adjudication_ecewe_facility" from="ccof_adjudication_ecewe" to="ccof_adjudication_eceweid">
        <filter>
          <condition attribute="ccof_adjudication_ecewe_facilityid" operator="eq" value="${eceweFacility}" />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>`

    showNotification(executionContext, formContext, fetchXML, CCOFXML);
}
