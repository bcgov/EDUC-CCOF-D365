using System;
using System.Activities;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace CCOF.Infrastructure.CustomWorkflowActivities.ECE_WEAdjudication
{
    public class CCFRIFields : CodeActivity
    {
        [Input("PaymentEligibilityStartDate")]
        public InArgument<DateTime> PaymentEligibilityStartDate { get; set; }

        [Input("OptOutMonth")]
        public InArgument<DateTime> OptOutMonth { get; set; }

        [Input("CCFRIFacility")]
        [ReferenceTarget("ccof_adjudication_ccfri_facility")]
        [RequiredArgument]
        public InArgument<EntityReference> CCFRIFacility { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

            var recordId = CCFRIFacility.Get(executionContext).Id;
            var startDate = PaymentEligibilityStartDate.Get(executionContext);
            var optOutDate = OptOutMonth.Get(executionContext);
            tracingService.Trace("{0}{1}", "Start Custom Workflow Activity: ECE-WE Facility - CCFRIFields", DateTime.Now.ToLongTimeString());
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""ccof_adjudication_ecewe_facility"">
                                    <attribute name=""ccof_adjudication_ecewe_facilityid"" />
		                            <attribute name=""ccof_mid_year_funding_date"" />
		                            <attribute name=""ccof_pay_eligibility_start_date"" />
                                    <filter type=""and"">
                                      <condition attribute=""ccof_adjudicationccfrifacility"" operator=""eq"" value=""{recordId}"" />
                                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                                    </filter>
                                    <link-entity name=""account"" from=""accountid"" to=""ccof_facility"" alias=""Facility"">
			                          <attribute name=""name"" />
                                      <link-entity name=""account"" from=""accountid"" to=""parentaccountid"" alias=""Org"">
                                        <attribute name=""name"" />
                                      </link-entity>
		                            </link-entity>
                                  </entity>
                                </fetch>";
                var teamFetch = $@"<fetch>
                                    <entity name=""team"">
                                        <attribute name=""teamid"" />
                                        <filter>
                                            <condition attribute=""name"" operator=""eq"" value=""CCOF - Adjudicator Team"" />
                                        </filter>  
                                    </entity>
                                   </fetch>";
                EntityCollection eceweFacilities = service.RetrieveMultiple(new FetchExpression(fetchXml));
                EntityCollection team = service.RetrieveMultiple(new FetchExpression(teamFetch));

                foreach (var item in eceweFacilities.Entities)
                {
                    Entity eceweRecord = new Entity("ccof_adjudication_ecewe_facility");
                    eceweRecord.Id = item.Id;
                    if (startDate > DateTime.MinValue)
                    {
                        eceweRecord["ccof_pay_eligibility_start_date"] = startDate.ToString("yyyy-MM");
                    }
                    if (optOutDate > DateTime.MinValue)
                    {
                        eceweRecord["ccof_mid_year_funding_date"] = optOutDate.ToString("yyyy-MM");
                        Entity task = new Entity("task");
                        task["ccof_regarding"] = new EntityReference("ccof_adjudication_ecewe_facility", item.Id);
                        task["subject"] = "SP has opted out of CCFRI for Org: " + ((AliasedValue)item["Org.name"]).Value + " and facility: " + ((AliasedValue)item["Facility.name"]).Value;
                        task["ownerid"] = new EntityReference("team", team.Entities[0].Id);
                        service.Create(task);

                    }
                    service.Update(eceweRecord);
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
