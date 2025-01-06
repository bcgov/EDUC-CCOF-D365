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
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
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
                    OptionSetValue preValue;
                    int preStatus = 99999;
                    switch (entity.LogicalName)
                    {
                        case "ccof_adjudication_ccfri":
                        case "ccof_adjudication_ccfri_facility":
                            tracingService.Trace("Starging log:"+entity.LogicalName);
                            if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity preImage)
                            {
                                // Access a value from Pre-Image
                                preValue = preImage.Contains("statuscode") ? (OptionSetValue)preImage["statuscode"] : null;
                                preStatus = preValue.Value;
                                tracingService.Trace("preStatus" + preStatus.ToString());

                            }
                            if (context.PostEntityImages.Contains("PostImage") && context.PostEntityImages["PostImage"] is Entity postImage)
                            {
                                // Access a value from Post-Image
                                var postValue = postImage.Contains("statuscode") ? (OptionSetValue)postImage["statuscode"] : null;
                                postApp = postImage.Contains("ccof_application") ? (EntityReference)postImage["ccof_application"] : null;
                                tracingService.Trace("postValue" + postValue.Value.ToString() + ";" + postApp.Name);

                            }
                            int postStatus = entity.GetAttributeValue<OptionSetValue>("statuscode").Value;
                            tracingService.Trace($"Choice Value: {postStatus}");

                            // Retrieve Choice Metadata for Label
                            var request = new RetrieveAttributeRequest
                            {
                                EntityLogicalName = entity.LogicalName,
                                LogicalName = "statuscode",
                                RetrieveAsIfPublished = true
                            };

                            var response = (RetrieveAttributeResponse)service.Execute(request);
                            var attributeMetadata = (EnumAttributeMetadata)response.AttributeMetadata;

                            // Find the matching label
                            var preOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == preStatus);
                            var postOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == postStatus);
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
                            // new a row
                            Entity statusHistoryRecord = new Entity("ccof_applicationstatushistory");
                            statusHistoryRecord["ccof_prestatusvalue"] = preStatus;
                            statusHistoryRecord["ccof_prestatuslabel"] = preChoiceLabel;
                            statusHistoryRecord["ccof_statusvalue"] = postStatus;
                            statusHistoryRecord["ccof_statuslabel"] = postChoiceLabel;
                            statusHistoryRecord["ccof_logdate"] = DateTime.UtcNow;
                            statusHistoryRecord["ccof_lookuptable"] = entity.LogicalName;
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
                            Guid recordId = service.Create(statusHistoryRecord);
                            tracingService.Trace($"Record created successfully with ID: {recordId}");
                            break;
                        case "ccof_adjudication_ccfri_facility00000":

                            break;
                        default:
                            tracingService.Trace($"No specific handler for entity: {entity.LogicalName}");
                            break;
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
