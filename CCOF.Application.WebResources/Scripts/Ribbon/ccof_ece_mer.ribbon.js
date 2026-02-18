// A webresource for AccountRibbon only

//Create Namespace Object if its defined
var CCOF = CCOF || {};
CCOF.ECE_MER = CCOF.ECE_MER || {};
CCOF.ECE_MER.Ribbon = CCOF.ECE_MER.Ribbon || {};

CCOF.ECE_MER.Ribbon = {
    // description
    RecalculateMER: function (primarycontrol, primaryEntiityId) {
         
        var gridContext = primarycontrol;
        var pageInput = {
            pageType: "custom",
            name: "ccof_monthlyecereportrecalculatehours_7e8cc",
            recordId: primaryEntiityId
        };
        var navigationOptions = {
            target: 2,
            position: 1,
            height: 306,
            width: 703,
        };
        Xrm.Navigation.navigateTo(pageInput, navigationOptions)
            .then(
                function () {
                    primarycontrol?.refresh();
                    console.log("Success");
                }
            ).catch(
                function (error) {
                    console.log(Error);
                }
            );
    }
}