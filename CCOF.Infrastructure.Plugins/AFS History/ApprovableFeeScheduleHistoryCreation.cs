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

namespace CCOF.Infrastructure.Plugins.AFS_History
{
    public class ApprovableFeeScheduleHistoryCreation : IPlugin
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
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    tracingService.Trace("Starting AFS History plugin");
                    //Retrieve MTFI record
                    var mtfi = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ccof_mtfi_qcdecision", "ownerid"));
                    if (mtfi != null && ((OptionSetValue)mtfi.Attributes["ccof_mtfi_qcdecision"]).Value == 100000007)
                    {
                        Entity afsHistory = new Entity("ccof_approvable_fee_schedule_history");
                        OptionSetValue preValue;
                        int preStatus = 0;
                        if (entity.Attributes.Contains("ownerid"))
                        {
                            if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity preImage)
                            {
                                var ownerId = preImage.Contains("ownerid") ? ((EntityReference)preImage["ownerid"]).Id : new Guid();
                                afsHistory["ccof_previous_owner"] = new EntityReference("systemuser", ownerId);
                                afsHistory["ccof_current_owner"] = mtfi.GetAttributeValue<EntityReference>("ownerid").Id;
                            }
                        }
                        //--------------------------------------------------------
                        if (entity.Attributes.Contains("statuscode"))
                        {
                            if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity preImage)
                            {
                                preValue = preImage.Contains("statuscode") ? (OptionSetValue)preImage["statuscode"] : null;
                                preStatus = preValue.Value;
                                tracingService.Trace("preStatus" + preStatus.ToString());
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
                            afsHistory["ccof_previous_status"] = preChoiceLabel;
                            afsHistory["ccof_current_status"] = postChoiceLabel;
                        }
                            //--------------------------------------------------------
                            
                        // add a new row                        
                        afsHistory["ccof_log_date"] = DateTime.UtcNow;
                        afsHistory["ccof_regarding_id"] = new EntityReference(entity.LogicalName, entity.Id);
                        Guid recordId = service.Create(afsHistory);
                        tracingService.Trace($"Record created successfully with ID: {recordId}");
                        tracingService.Trace("End App Status History plugin");
                    }

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
