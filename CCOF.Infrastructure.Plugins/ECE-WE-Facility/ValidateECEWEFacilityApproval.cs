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

namespace CCOF.Infrastructure.Plugins.ECE_WE_Facility
{
    public class ValidateECEWEFacilityApproval : IPlugin
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
                tracingService.Trace("Starting ECE WE Facility plugin");

                try
                {
                    // Ensure "PreImage" exists and is an Entity
                    if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity preImage)
                    {
                        tracingService.Trace("entered");
                        // Check if the attribute "ccof_adjudication_ecewe" exists in the entity
                        if (preImage.Attributes.Contains("ccof_adjudication_ecewe"))
                        {
                            tracingService.Trace("Yes, contains ecewe");

                            // Retrieve the ECEWE entity reference from the pre-image
                            var eceweReference = preImage.GetAttributeValue<EntityReference>("ccof_adjudication_ecewe");
                            if (eceweReference != null)
                            {
                                Guid ECEWEId = eceweReference.Id;
                                tracingService.Trace("ECEWEId: " + ECEWEId);

                                // Retrieve the ECEWE entity
                                Entity ECEWE = service.Retrieve("ccof_adjudication_ecewe", ECEWEId, new ColumnSet("ccof_adjudication"));
                                tracingService.Trace("Retrieved ECE WE record");

                                // Get the reference to the CCOF entity from the ECEWE record
                                var ccofReference = ECEWE.GetAttributeValue<EntityReference>("ccof_adjudication");
                                if (ccofReference != null)
                                {
                                    Guid CCOFId = ccofReference.Id;
                                    tracingService.Trace("Starting ECE WE Facility plugin");

                                    // Retrieve the CCOF entity
                                    Entity CCOF = service.Retrieve("ccof_adjudication", CCOFId, new ColumnSet("ccof_basepayactivated"));
                                    tracingService.Trace("Retrieved CCOF");

                                    // Check if base pay is activated
                                    bool basePayActivated = CCOF.GetAttributeValue<bool>("ccof_basepayactivated");
                                    tracingService.Trace("CCOF Base Pay Activated? " + basePayActivated.ToString());

                                    // If base pay is not activated, throw an exception
                                    if (!basePayActivated)
                                    {
                                        throw new InvalidPluginExecutionException("Base Pay must be Active in order to approve.");
                                    }
                                }
                                else
                                {
                                    throw new InvalidPluginExecutionException("CCOF entity reference not found in ECEWE.");
                                }
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("ECEWE entity reference not found in PreImage.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log more details for debugging
                    tracingService.Trace("Error: " + ex.Message);
                    tracingService.Trace("Stack Trace: " + ex.StackTrace);

                    throw new InvalidPluginExecutionException("An error occurred in the plugin: " + ex.Message);
                }

            }
        }
    }
}
