using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace CCOF.Infrastructure.Plugins.FundingAgreement
{
    public class RestrictFundingStatus : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(null);
                tracingService.Trace("Starting Restrict funding status plugin");
                try
                {

                    if (!entity.Attributes.Contains("statuscode")) return;

                    var newStatus = ((OptionSetValue)entity["statuscode"]).Value;

                    // Drafted statuses' option set values
                    //var draftedStatuses = new List<int> { 101510002, 101510003, 101510004 }; // Replace with real values
                    var draftedStatuses = new List<int>();
                    var preStatus = 0;
                    if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity preImage)
                    {
                        preStatus = preImage.Contains("statuscode") ? ((OptionSetValue)preImage["statuscode"]).Value : -1;
                        if (preStatus != -1)
                        {
                            if (preStatus == 101510003 || preStatus == 101510005)
                            {
                                draftedStatuses.Add(101510002);
                            }
                            else if (preStatus == 101510004)
                            {
                                draftedStatuses.Add(101510002);
                                draftedStatuses.Add(101510003);
                            }
                            else if (preStatus == 101510001)
                            {
                                draftedStatuses.Add(101510002);
                                draftedStatuses.Add(101510003);
                                draftedStatuses.Add(101510004);
                            }
                            else if (preStatus == 1)
                            {
                                draftedStatuses.Add(101510002);
                                draftedStatuses.Add(101510003);
                                draftedStatuses.Add(101510004);
                                draftedStatuses.Add(101510001);
                            }
                        }
                    }

                    if (draftedStatuses.Contains(newStatus))
                    {
                        // Fetch user roles
                        var userId = context.InitiatingUserId;
                        var fetchXml = $@"
            <fetch>
              <entity name='systemuserroles'>
                <attribute name='roleid' />
                <filter>
                  <condition attribute='systemuserid' operator='eq' value='{userId}' />
                </filter>
                <link-entity name='role' from='roleid' to='roleid' alias='r'>
                  <attribute name='name' />
                </link-entity>
              </entity>
            </fetch>";

                        var roles = service.RetrieveMultiple(new FetchExpression(fetchXml));

                        var allowedRoles = new List<string> { "System Administrator", "CCOF - Admin" };
                        bool isAuthorized = roles.Entities.Any(role =>
                        {
                            var roleName = (string)((AliasedValue)role["r.name"]).Value;
                            return allowedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
                        });

                        if (!isAuthorized)
                        {
                            throw new InvalidPluginExecutionException("You do not have permission to change the funding status. Only Administrators can perform this action.");
                        }
                    }

                    if (newStatus == 1)
                    {
                        if (preStatus != 101510001)
                        {
                            throw new InvalidPluginExecutionException("Every FA needs to goes through ‘Approved’ status and then become ‘Active’");
                        }
                        var fundingAgreement = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ccof_organization"));
                        var allFundingAgreements = new QueryExpression(entity.LogicalName)
                        {
                            Criteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("ccof_organization",ConditionOperator.Equal,((EntityReference)fundingAgreement["ccof_organization"]).Id),
                                new ConditionExpression("statuscode",ConditionOperator.Equal,1),
                                new ConditionExpression("ccof_funding_agreementid",ConditionOperator.NotEqual,entity.Id)
                            }
                        }
                        };

                        var existingActiveFDRs = service.RetrieveMultiple(allFundingAgreements);
                        if (existingActiveFDRs.Entities.Any())
                        {
                            throw new InvalidPluginExecutionException("Only one funding agreement record can be active for a given organization. Please deactivate the existing record before activating another.");
                        }
                    }

                    tracingService.Trace("End Restrict funding status plugin");
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("RestrictFundingStatus Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
