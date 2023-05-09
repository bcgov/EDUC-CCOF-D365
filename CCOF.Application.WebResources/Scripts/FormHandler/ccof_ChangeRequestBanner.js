
//CCFRI -> Application -> Change Request

function createStyle() {
    var customStyle = 'div.ms-MessageBar-content { background-color: #FFEB2A!important }';
    var css = document.createElement('style');
    css.type = 'text/css';
    css.innerHTML = customStyle;
    parent.document.head.appendChild(css);
}


function showNotification(executionContext, formContext, fetchXML, flag) {
    createStyle();

    Xrm.WebApi.retrieveMultipleRecords("ccof_change_request", fetchXML).then(
        function success(result) {
            for (var i = 0; i < result.entities.length; i++) {
                //if the status is Submitted, In Progress, With Provider, Eligible
                if (result.entities[i].statuscode == 3 || result.entities[i].statuscode == 4 || result.entities[i].statuscode == 5 || result.entities[i].statuscode == 9) {
                    //show the banner
                    console.log(flag);
                    if (flag) { //CCOF
                        formContext.ui.tabs.get("Summary").sections.get("Notification").setVisible(true);


                    } else { // 
                        var ccof_records = formContext.getAttribute("ccof_adjudication").getValue();
                        console.log(ccof_records);
                        var ccof_id = ccof_records[0]["id"].replace("{", "").replace("}", "");
 //                       var powerappId = null;

                        var globalContext = Xrm.Utility.getGlobalContext();
                        var appurl = globalContext.getCurrentAppUrl();
                        globalContext.getCurrentAppProperties().then(
                            function success(app) {
 //                               powerappId = app.appId;
                                console.log(app);
                            },
                            function errorCallback() {
                                console.log("Error");
                            });


                        var url = appurl + '&forceUCI=1&pagetype=entityrecord&etn=ccof_adjudication&id=' + ccof_id;
                        //var url = executionContext.getContext().getClientUrl() + '/main.aspx?appid=' + powerappId + '&forceUCI=1&pagetype=entityrecord&etn=ccof_adjudication&id=' + ccof_id;

                        // var url = "https://mychildcareservicesdev.crm3.dynamics.com/main.aspx?appid=733af835-f8da-4763-b4ab-972ebdc95f65&forceUCI=1&pagetype=entityrecord&etn=ccof_adjudication&id=" + ccof_id;

                        //set the attribute value
                        var notification_link = formContext.getAttribute("ccof_notification_link").getValue();
                        if (notification_link == null) {
                            formContext.getAttribute("ccof_notification_link").setValue(url);
                            formContext.data.entity.save();
                        }

                        //set the notification section visible
                        // formContext.ui.tabs.get("Summary").sections.get("Notification").setVisible(true);
                        formContext.getControl("ccof_notification_link").setVisible(true);
                    }
                    break;
                }
            }
        },
        function (error) {
            console.log(error.message);
        }
    )

}

function showNotificationCCFRI(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var ccfri = formContext.data.entity.getId().replace("{", "").replace("}", "");

    var fetchXML = `?fetchXml=<fetch>
    <entity name="ccof_change_request">
    <link-entity name="ccof_application" from="ccof_applicationid" to="ccof_application" link-type="inner" alias="application">
      <link-entity name="ccof_adjudication_ccfri" from="ccof_application" to="ccof_applicationid" link-type="inner">
        <filter>
          <condition attribute="ccof_adjudication_ccfriid" operator="eq" value="${ccfri}" uitype="ccof_adjudication_ccfri" />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
  </fetch>`

    showNotification(executionContext, formContext, fetchXML, false);

}

function showNotificationCCOF(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var ccof = formContext.data.entity.getId().replace("{", "").replace("}", "");
    // console.log(ccof);
    var fetchXML = `?fetchXml=<fetch>
    <entity name="ccof_change_request">
    <link-entity name="ccof_application" from="ccof_applicationid" to="ccof_application" link-type="inner" alias="application">
      <link-entity name="ccof_adjudication" from="ccof_application" to="ccof_applicationid" link-type="inner">
        <filter>
          <condition attribute="ccof_adjudicationid" operator="eq" value="${ccof}" />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
  </fetch>`

    showNotification(executionContext, formContext, fetchXML, true);

}

function showNotificationECE(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var ecewe = formContext.data.entity.getId().replace("{", "").replace("}", "");

    var fetchXML = `?fetchXml=<fetch>
    <entity name="ccof_change_request">
    <link-entity name="ccof_application" from="ccof_applicationid" to="ccof_application" link-type="inner" alias="application">
      <link-entity name="ccof_adjudication_ecewe" from="ccof_application" to="ccof_applicationid" link-type="inner">
        <filter>
          <condition attribute="ccof_adjudication_eceweid" operator="eq" value="${ecewe}" />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
  </fetch>`

    showNotification(executionContext, formContext, fetchXML, false);

}