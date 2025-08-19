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

                    // Fetch user roles
                    var userId = context.InitiatingUserId;
                    var fetchXmlUserRoles = $@"
                                            <fetch>
                                              <entity name='systemuser'>
                                                <attribute name='fullname' />
                                                <filter>
                                                  <condition attribute='systemuserid' operator='eq' value='{{83fab296-c1d8-ed11-a7c6-000d3a09d4d4}}' />
                                                </filter>
                                                <link-entity name='systemuserroles' from='systemuserid' to='systemuserid' link-type='outer'>
                                                  <link-entity name='role' from='roleid' to='roleid' alias='r1'>
                                                    <attribute name='name' />
                                                    <attribute name='roleid' />
                                                  </link-entity>
                                                </link-entity>
                                              </entity>
                                            </fetch>";

                    var fetchXmlTeamRoles = $@"
                                            <fetch>
                                              <entity name='systemuser'>
                                                <attribute name='fullname' />
                                                <filter>
                                                  <condition attribute='systemuserid' operator='eq' value='{{83fab296-c1d8-ed11-a7c6-000d3a09d4d4}}' />
                                                </filter>
                                                <link-entity name='teammembership' from='systemuserid' to='systemuserid' link-type='outer'>
                                                  <link-entity name='team' from='teamid' to='teamid'>
                                                    <link-entity name='teamroles' from='teamid' to='teamid'>
                                                      <link-entity name='role' from='roleid' to='roleid' alias='r2'>
                                                        <attribute name='name' />
                                                        <attribute name='roleid' />
                                                      </link-entity>
                                                    </link-entity>
                                                  </link-entity>
                                                </link-entity>
                                              </entity>
                                            </fetch>";

                    var roles1 = service.RetrieveMultiple(new FetchExpression(fetchXmlUserRoles));       // systemUserRoles
                    var roles2 = service.RetrieveMultiple(new FetchExpression(fetchXmlTeamRoles));       // teamRoles

                    // Status transition - backward operation

                    if (draftedStatuses.Contains(newStatus))
                    {
                        var allowedRoles = new List<string> { "System Administrator", "CCOF - Admin" };
                        bool isAuthorized1 = roles1.Entities.Any(role =>
                        {
                            var roleName = (string)((AliasedValue)role["r1.name"]).Value;
                            return allowedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
                        });
                        bool isAuthorized2 = roles2.Entities.Any(role =>
                        {
                            var roleName = (string)((AliasedValue)role["r2.name"]).Value;
                            return allowedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
                        });

                        if (isAuthorized1 == false && isAuthorized2 == false)
                        {
                            throw new InvalidPluginExecutionException("You do not have permission to change the funding status. Only Administrators can perform this action.");
                        }
                    }

                    // Status transition - forward operation

                    if (newStatus == 101510001)          // 101510001 - "Approved"
                    {
                        if (preStatus != 101510004)      // 101510004 - "Drafted - with Ministry"
                        {
                            throw new InvalidPluginExecutionException("Every FA needs to go through ‘Drafted - with Ministry’ status and then becomes ‘Approved’");
                        }
                        var allowedRoles = new List<string> { "System Administrator",
                                                              "CCOF - Admin",
                                                              "CCOF - Leadership",
                                                              "CCOF - Super Awesome Mods Squad",															  
                                                              "CCOF - Mod QC",
                                                              "CCOF - QC",
                                                              "CCOF - Sr. Adjudicator",
                                                              "CCOF - Adjudicator"                      
                                                            };
                        bool isAuthorized1 = roles1.Entities.Any(role =>
                        {
                            var roleName = (string)((AliasedValue)role["r1.name"]).Value;
                            return allowedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
                        });
                        bool isAuthorized2 = roles2.Entities.Any(role =>
                        {
                            var roleName = (string)((AliasedValue)role["r2.name"]).Value;
                            return allowedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
                        });

                        if (isAuthorized1 == false && isAuthorized2 == false)
                        {
                            throw new InvalidPluginExecutionException("You do not have permission to change the funding status. Only Adjudicator and higher can perform this action.");
                        }
                    }

                    if (newStatus == 1)                  // 1 - "Active"
                    {
                        if (preStatus != 101510001)      // 101510001 - "Approved"
                        {
                            throw new InvalidPluginExecutionException("Every FA needs to go through ‘Approved’ status and then becomes ‘Active’");
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