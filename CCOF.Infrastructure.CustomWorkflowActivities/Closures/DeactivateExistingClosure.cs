using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace CCOF.Infrastructure.CustomWorkflowActivities.Closures
{
    public class DeactivateExistingClosure : CodeActivity
    {
        [ReferenceTarget("account")]
        [RequiredArgument]
        [Input("Facility")]
        public InArgument<EntityReference> facility { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
            //var recordId = context.PrimaryEntityId;
            var recordId = facility.Get(executionContext).Id;
            tracingService.Trace("{0}{1}", "Start Custom Workflow Activity: Closures - DeactivateExistingClosure", DateTime.Now.ToLongTimeString());
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""ccof_application_ccfri_closure"">
                                    <attribute name=""ccof_application_ccfri_closureid"" />
                                    <filter type=""and"">
                                      <condition attribute=""ccof_facilityinfo"" operator=""eq"" uitype=""account"" value=""{recordId}"" />
                                      <condition attribute=""ccof_closure_status"" operator=""eq"" value=""100000003"" />
                                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                                    </filter>
                                  </entity>
                                </fetch>";
                EntityCollection closures = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (closures.Entities.Count == 1)
                {
                    Entity closureTable = new Entity("ccof_application_ccfri_closure");
                    closureTable.Id = closures.Entities[0].Id;
                    closureTable["statuscode"] = new OptionSetValue(2);
                    closureTable["statecode"] = new OptionSetValue(1);
                    service.Update(closureTable);
                }
                tracingService.Trace("Workflow activity end.");
            }
            catch (Exception ex)
            {
                throw new InvalidWorkflowException("Exeception in Custom Workflow -" + ex.Message + ex.InnerException);
            }
        }
    }
}
