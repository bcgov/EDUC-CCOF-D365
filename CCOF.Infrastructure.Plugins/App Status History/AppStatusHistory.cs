using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CCOF.Infrastructure.Plugins
{
    public class AppStatusHistory : IPlugin
    {
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                IOrganizationService serviceForSA = serviceFactory.CreateOrganizationService(null);

                try
                {
                    tracingService.Trace("Starging App Status History plugin");
                    // Retrieve the current user ID (the user who triggered the plugin).
                    Guid currentUserId = context.InitiatingUserId;
                    tracingService.Trace("context.InitiatingUserId: " + currentUserId);
                    // Or use context.UserId if impersonation is involved
                    Guid executingUserId = context.UserId;
                    EntityReference postApp = null, account = null, preOwner = null, postOwner = null;
                    OptionSetValue preStatus = new OptionSetValue(9999), postStatus = new OptionSetValue(9999);
                    Entity preImage = null, postImage = null, statusHistoryRecord = null;
                    tracingService.Trace("Entity is: " + entity.LogicalName + "; Message Type is:" + context.MessageName);
                    if (context.PostEntityImages.Contains("PostImage") && context.PostEntityImages["PostImage"] is Entity)
                    {
                        postImage = (Entity)context.PostEntityImages["PostImage"];
                        postStatus = postImage.Contains("statuscode") ? (OptionSetValue)postImage["statuscode"] : null;
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
                                EntityReference adjECEWE = (EntityReference)postImage["ccof_adjudication_ecewe"];
                                Entity adjECEWERecord = service.Retrieve(adjECEWE.LogicalName, adjECEWE.Id, new ColumnSet(true));
                                postApp = adjECEWERecord.Contains("ccof_application") ? (EntityReference)adjECEWERecord["ccof_application"] : null;
                                break;
                            case "ccof_adjudication":
                                account = postImage.Contains("ccof_organization") ? (EntityReference)postImage["ccof_organization"] : null;
                                break;
                            default:
                                throw new InvalidPluginExecutionException($"Entity '{context.PrimaryEntityName}' is not supported for account lookup.");
                        }
                        tracingService.Trace("account:" + account.Name + "; App:" + postApp.Name);

                    }

                    switch (context.MessageName)
                    {
                        case "Create":
                            postStatus = entity.GetAttributeValue<OptionSetValue>("statuscode");
                            postOwner = entity.GetAttributeValue<EntityReference>("ownerid");
                            tracingService.Trace($"StatusCodee: {postStatus.Value};Owner:{postOwner.Name}");

                            var request = new RetrieveAttributeRequest
                            {
                                EntityLogicalName = entity.LogicalName,
                                LogicalName = "statuscode",
                                RetrieveAsIfPublished = true
                            };

                            var response = (RetrieveAttributeResponse)service.Execute(request);
                            var attributeMetadata = (EnumAttributeMetadata)response.AttributeMetadata;
                            var postOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == postStatus.Value);
                            string preChoiceLabel = string.Empty;
                            string postChoiceLabel = string.Empty;
                            if (postOption != null)
                            {
                                postChoiceLabel = postOption.Label.UserLocalizedLabel.Label;
                                tracingService.Trace($"Statuscode Choice Label: {postChoiceLabel}");
                            }
                            else
                            {
                                tracingService.Trace("Choice value not found in metadata.");
                            }

                            statusHistoryRecord = new Entity("ccof_applicationstatushistory");
                            statusHistoryRecord["ccof_logdate"] = DateTime.UtcNow;
                            statusHistoryRecord["ccof_lookuptable"] = entity.LogicalName;
                            statusHistoryRecord["ccof_statusvalue"] = (int)postStatus.Value;
                            statusHistoryRecord["ccof_statuslabel"] = postChoiceLabel;
                            statusHistoryRecord["ccof_messagetype"] = new OptionSetValue(100000000);
                            statusHistoryRecord["ccof_postownerid"] = new EntityReference(
                                       postOwner.LogicalName,
                                       postOwner.Id
                                  );
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
                            Guid recordId = serviceForSA.Create(statusHistoryRecord);
                            tracingService.Trace($"Record created successfully with ID: {recordId}");

                            break;

                        case "Update":
                            // not track statuscode no changes
                            if (!entity.Contains("statuscode"))
                            {
                                tracingService.Trace("There is no statuscode field in context. The vlaue is ");
                                if (!entity.Contains("ownerid"))
                                {
                                    tracingService.Trace("There is no Ownerid field in context.");
                                    break;
                                }
                            }
                            if (entity.Contains("statuscode"))
                            {
                                if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity)
                                {
                                    preImage = (Entity)context.PreEntityImages["PreImage"];
                                    preStatus = preImage.Contains("statuscode") ? (OptionSetValue)preImage["statuscode"] : null;
                                    tracingService.Trace("preStatus" + preStatus.Value);
                                }
                                postStatus = entity.GetAttributeValue<OptionSetValue>("statuscode");
                                tracingService.Trace($"postStatus: {postStatus.Value}");
                                if (preStatus.Value == postStatus.Value)
                                {
                                    tracingService.Trace("Statuscode value doesn't change. preValue is: " + preStatus.Value);
                                }
                                else
                                {
                                    request = new RetrieveAttributeRequest
                                    {
                                        EntityLogicalName = entity.LogicalName,
                                        LogicalName = "statuscode",
                                        RetrieveAsIfPublished = true
                                    };
                                    response = (RetrieveAttributeResponse)service.Execute(request);
                                    attributeMetadata = (EnumAttributeMetadata)response.AttributeMetadata;
                                    var preOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == preStatus.Value);
                                    postOption = attributeMetadata.OptionSet.Options.FirstOrDefault(opt => opt.Value == postStatus.Value);
                                    preChoiceLabel = string.Empty;
                                    postChoiceLabel = string.Empty;
                                    if (preOption != null)
                                    {
                                        preChoiceLabel = preOption.Label.UserLocalizedLabel.Label;
                                        tracingService.Trace($"preStatus Choice Label: {preChoiceLabel}");
                                    }
                                    else
                                    {
                                        tracingService.Trace("Choice value not found in metadata.");
                                    }
                                    if (postOption != null)
                                    {
                                        postChoiceLabel = postOption.Label.UserLocalizedLabel.Label;
                                        tracingService.Trace($"postStatus Choice Label: {postChoiceLabel}");
                                    }
                                    else
                                    {
                                        tracingService.Trace("Choice value not found in metadata.");
                                    }
                                    // new a row to track statuscode change
                                    statusHistoryRecord = new Entity("ccof_applicationstatushistory");
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
                                    recordId = serviceForSA.Create(statusHistoryRecord);
                                    tracingService.Trace($"Record created successfully with ID: {recordId}");
                                }
                            }
                            if (entity.Contains("ownerid"))
                            {
                                if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity)
                                {
                                    preImage = (Entity)context.PreEntityImages["PreImage"];
                                    preOwner = preImage.Contains("ownerid") ? (EntityReference)preImage["ownerid"] : null;
                                    tracingService.Trace("preOwner" + preOwner.Name);
                                }
                                postOwner = entity.GetAttributeValue<EntityReference>("ownerid");
                                tracingService.Trace($"post Ownerid: {postOwner.Name}");
                                if ((preOwner.LogicalName == postOwner.LogicalName) && (preOwner.Id == postOwner.Id))
                                {
                                    tracingService.Trace("Ownerid doesn't change. preValue is: " + preOwner.Id);
                                }
                                else
                                {
                                    statusHistoryRecord = new Entity("ccof_applicationstatushistory");
                                    statusHistoryRecord["ccof_logdate"] = DateTime.UtcNow;
                                    statusHistoryRecord["ccof_lookuptable"] = entity.LogicalName;
                                    statusHistoryRecord["ccof_messagetype"] = new OptionSetValue(100000002);
                                    statusHistoryRecord["ccof_preownerid"] = new EntityReference(
                                            preOwner.LogicalName,
                                            preOwner.Id
                                        );
                                    statusHistoryRecord["ccof_postownerid"] = new EntityReference(
                                               postOwner.LogicalName,
                                               postOwner.Id
                                          );
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
                                    recordId = serviceForSA.Create(statusHistoryRecord);
                                    tracingService.Trace($"Record created successfully with ID: {recordId}");
                                }
                            }
                            break;
                        default:
                            throw new InvalidPluginExecutionException($"Unhandled Message: {context.MessageName}");
                    }

                    tracingService.Trace("End App Status History plugin");

                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    tracingService.Trace("An error occurred in AppStatusHistory Plugin." + ex.ToString());
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
