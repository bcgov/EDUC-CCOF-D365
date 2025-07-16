using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CCOF.Infrastructure.Plugins.MonthlyEnrolmentReport
{
    public class UpdateSubmissionDeadline : IPlugin
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
                tracingService.Trace("Starting Update Submission Deadline plugin");
                try
                {
                    if (!entity.Attributes.Contains("ccof_month")) return;

                    int monthValue = entity.GetAttributeValue<OptionSetValue>("ccof_month").Value;
                    int year = DateTime.UtcNow.Year;
                    int year_d365;
                    if (!entity.Attributes.Contains("ccof_year"))
                    {
                        var entityYear = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ccof_year"));
                        year_d365 = Convert.ToInt16(entityYear.GetAttributeValue<string>("ccof_year"));
                    }
                    else
                        year_d365 = Convert.ToInt16(entity.GetAttributeValue<string>("ccof_year"));
                    if (monthValue + 6 > 12)
                    {
                        monthValue = monthValue - 12;
                        year_d365 += 1;
                    }
                    int lastDay = DateTime.DaysInMonth(year_d365, monthValue + 6);
                    var submissionDate = new DateTime(year_d365, monthValue + 6, lastDay);
                    entity["ccof_submissiondeadline"] = submissionDate;

                    tracingService.Trace("End Update Submission Deadline plugin");
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("UpdateSubmissionDeadline Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}