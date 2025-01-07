using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CCOF.Infrastructure.Plugins
{
    public class AppStatusHistory : IPlugin
    {
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    tracingService.Trace("Starging App Status History plugin");
                    // Retrieve the current user ID (the user who triggered the plugin).
                    Guid currentUserId = context.InitiatingUserId;
                    // Or use context.UserId if impersonation is involved
                    Guid executingUserId = context.UserId;
                    EntityReference postApp = null;
                    EntityReference account = null;
                    OptionSetValue preStatus =new OptionSetValue(9999);
                    OptionSetValue postStatus= new OptionSetValue(9999);
                    tracingService.Trace("Entity is: " + entity.LogicalName+"; Message Type is:"+context.MessageName);
                    if (context.PostEntityImages.Contains("PostImage") && context.PostEntityImages["PostImage"] is Entity postImage)
                    {
                        var postValue = postImage.Contains("statuscode") ? (OptionSetValue)postImage["statuscode"] : null;
                        postApp = postImage.Contains("ccof_application") ? (EntityReference)postImage["ccof_application"] : null;
                        switch (context.PrimaryEntityName)
                        {
                            case "ccof_adjudication_ccfri":
                                account = postImage.Contains("ccof_organization") ? (EntityReference)postImage["ccof_organization"] : null;
                                break;
                            case "ccof_adjudication_ccfri_facility":
                                account = postImage.Contains("ccof_facility") ? (EntityReference)postImage["ccof_facility"] : null;
                                break;

                            case "ccof_adjudication_ecewe":
                                account = postImage.Contains("ccof_organization") ? (EntityReference)postImage["ccof_organization"] : null;
                                break;
                            case "ccof_adjudication_ecewe_facility":
                                account = postImage.Contains("ccof_facility") ? (EntityReference)postImage["ccof_facility"] : null;
                                break;
                            case "ccof_adjudication":
                                account = postImage.Contains("ccof_organization") ? (EntityReference)postImage["ccof_organization"] : null;
                                break;
                            default:
                                throw new InvalidPluginExecutionException($"Entity '{context.PrimaryEntityName}' is not supported for account lookup.");
                        }
                        tracingService.Trace("postValue" + postValue.Value.ToString() + ";" + postApp.Name);

                    }

                    switch (context.MessageName)
                    {
                        case "Create":

                            break;
                        case "Update":
                            if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity preImage)
                            {
                                preStatus = preImage.Contains("statuscode") ? (OptionSetValue)preImage["statuscode"] : null;
                                tracingService.Trace("preStatus" + preStatus.Value);
                            }
                            // not track statuscode no changes
                            if (!entity.Contains("statuscode"))
                            {
                                tracingService.Trace("The Statuscode value doesn't change. The vlaue is " + preStatus.Value);
                                break;
                            }
                            postStatus = entity.GetAttributeValue<OptionSetValue>("statuscode");
                            tracingService.Trace($"Choice Value: {postStatus.Value}");

                            var request = new RetrieveAttributeRequest
                            {
                                EntityLogicalName = entity.LogicalName,
                                LogicalName = "statuscode",
                                RetrieveAsIfPublished = true
                            };

                            var response = (RetrieveAttributeResponse)service.Execute(request);
                            var attributeMetadata = (EnumAttributeMetadata)response.AttributeMetadata;

                            var preOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == preStatus.Value);
                            var postOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == postStatus.Value);
                            string preChoiceLabel = string.Empty;
                            string postChoiceLabel = string.Empty;
                            if (preOption != null)
                            {
                                preChoiceLabel = preOption.Label.UserLocalizedLabel.Label;
                                tracingService.Trace($"Choice Label: {preChoiceLabel}");
                            }
                            else
                            {
                                tracingService.Trace("Choice value not found in metadata.");
                            }
                            if (postOption != null)
                            {
                                postChoiceLabel = postOption.Label.UserLocalizedLabel.Label;
                                tracingService.Trace($"Choice Label: {postChoiceLabel}");
                            }
                            else
                            {
                                tracingService.Trace("Choice value not found in metadata.");
                            }
                            // new a row to track statuscode change
                            Entity statusHistoryRecord = new Entity("ccof_applicationstatushistory");
                            statusHistoryRecord["ccof_prestatusvalue"] = (int)preStatus.Value;
                            statusHistoryRecord["ccof_prestatuslabel"] = preChoiceLabel;
                            statusHistoryRecord["ccof_statusvalue"] = (int)postStatus.Value;
                            statusHistoryRecord["ccof_statuslabel"] = postChoiceLabel;
                            statusHistoryRecord["ccof_logdate"] = DateTime.UtcNow;
                            statusHistoryRecord["ccof_lookuptable"] = entity.LogicalName;
                            statusHistoryRecord["ccof_messagetype"] = new OptionSetValue(100000001);
                            statusHistoryRecord["ccof_application"] = new EntityReference(
                                    "ccof_application",
                                    postApp.Id
                                );
                            statusHistoryRecord["ccof_statushistoryregardingid"] = new EntityReference(
                                entity.LogicalName,
                                entity.Id
                            );
                            statusHistoryRecord["ccof_operationuser"] = new EntityReference(
                                 "systemuser",
                                  currentUserId
                            );
                            statusHistoryRecord["ccof_account"] = new EntityReference(
                                  "account",
                                  account.Id
                            );
                            Guid recordId = service.Create(statusHistoryRecord);
                            tracingService.Trace($"Record created successfully with ID: {recordId}");
                            break;
                        case "Assign":
                            break;
                        default:
                            throw new InvalidPluginExecutionException($"Unhandled Message: {context.MessageName}");
                    }

                    tracingService.Trace("End App Status History plugin");

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in AppStatusHistory Plugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("AppStatusHistory Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
