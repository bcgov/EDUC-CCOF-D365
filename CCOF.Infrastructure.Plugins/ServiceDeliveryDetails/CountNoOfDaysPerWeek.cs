using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CCOF.Infrastructure.Plugins.ServiceDeliveryDetails
{
    public class CountNoOfDaysPerWeek : IPlugin
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

                // Obtain the IOrganizationService instance which you will need for web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                tracingService.Trace("Starting Count No. Of Days Per Week plugin");
                try
                {
                    if (!(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity target))
                        return;

                    const string MultiSelectAttr = "ccof_max_days_per_week";
                    const string CountAttr = "ccof_max_days_per_week_count";

                    int count = 0;

                    if (entity.Attributes.Contains(MultiSelectAttr))
                    {
                        count = CountSelected(target[MultiSelectAttr]);
                    }

                    // Write the count back to the record in the same transaction
                    target[CountAttr] = count;

                    tracingService.Trace("End Count No. Of Days Per Week plugin");
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("CountNoOfDaysPerWeek Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

        private static int CountSelected(object value)
        {
            var col = value as OptionSetValueCollection;
            return col?.Count ?? 0;
        }
    }
}