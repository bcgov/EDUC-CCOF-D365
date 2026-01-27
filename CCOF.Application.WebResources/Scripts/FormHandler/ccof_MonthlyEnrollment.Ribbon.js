
var CCOF = CCOF || {};
CCOF.MonthlyEnrollment = CCOF.MonthlyEnrollment || {};
CCOF.MonthlyEnrollment.Ribbon = CCOF.MonthlyEnrollment.Ribbon || {};

CCOF.MonthlyEnrollment.Ribbon = {

    BulkApproveMER: function (selectedControl, selectedItemIds) {
        debugger;
        var gridContext = selectedControl;
        var pageInput = {
            pageType: "custom",
            name: "ccof_bulkapproveenrolmentreports_81c58",
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
    showHideBulkApproveMER: function (selectedControl) {
        debugger;
        var visible = false;
        var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

        userRoles.forEach(function hasRole(item, index) {
            if (item.name === "CCOF - Sr. Accounts" || item.name === "System Administrator") {
                visible = true;
            }
        });

        return visible;
    }
}