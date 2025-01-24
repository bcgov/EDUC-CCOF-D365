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

namespace CCOF.Infrastructure.Plugins.Assigned_Date
{
    public class UpdateAssignedDate : IPlugin
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
                tracingService.Trace("Starting Update Assigned Date plugin");
                try
                {

                    if (entity.Attributes.Contains("ownerid"))
                    {
                        //Update Assigned Date
                        Entity updateEntity = new Entity(entity.LogicalName)
                        {
                            Id = entity.Id,
                        };
                        updateEntity["ccof_assigned_date"] = DateTime.UtcNow;
                        service.Update(updateEntity);
                    }

                    tracingService.Trace("End UpdateAssignedDate plugin");
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in UpdateAssignedDate Plugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("UpdateAssignedDate Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
