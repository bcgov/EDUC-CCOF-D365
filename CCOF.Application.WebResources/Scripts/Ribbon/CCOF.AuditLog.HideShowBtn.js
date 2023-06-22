// Audit Log  Hide-Show  buttons based on security role
function hideButton(primaryContext) {
    debugger; 
    var isButtonEnabled = true;
    var userRoles=Xrm.Utility.getGlobalContext().userSettings;
    if(Object.keys(userRoles.roles._collection).length>0)
    {
        for ( var rolidcollection in userRoles.roles._collection)
        {
           var currentUserRoles= Xrm.Utility.getGlobalContext().userSettings.roles._collection[rolidcollection].name;    
           if(currentUserRoles == "System Administrator" || currentUserRoles == "CCOF - Apps & Mods" ){            
               isButtonEnabled = true;                            
               break;                                                                                                 
           } 
           else if (currentUserRoles != "System Administrator" || currentUserRoles != "CCOF - Apps & Mods" ){
            isButtonEnabled = false;                    
           }      
        }           
    }    
    return isButtonEnabled;    
}