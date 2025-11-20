using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CCOF.Infrastructure.Plugins.ECEWE
{
    public class GenerateMonthlyReportNumber : IPlugin
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
                context.InputParameters["Target"] is Entity entity)
            {
                // Obtain the target entity from the input parameters.  
               // Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                Entity facility = service.Retrieve("account", entity.GetAttributeValue<EntityReference>("ccof_facility").Id, new ColumnSet("accountnumber"));
             

                tracingService.Trace("Starting ECE WE Facility plugin");

                try
                {
                    if (entity.Contains("ccof_base_report_id"))
                    {
                       Entity base_Report = service.Retrieve("ccof_ece_monthly_report", entity.GetAttributeValue<EntityReference>("ccof_base_report_id").Id, new ColumnSet("ccof_name"));
                           var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>" +
                             "<entity name='ccof_ece_monthly_report'>" +
                              "<attribute name='ccof_version' />" +
                            "<attribute name='ccof_name'/>" +
                            "<attribute name='createdon' />" +
                            "<order attribute='ccof_version' descending='true' />" +
                            "<filter type='and'>" +
                            "<condition attribute='ccof_base_report_id' operator='eq' uitype='ccof_ece_monthly_report'  value='{"+ base_Report.Id+ "}' />" +
                            "</filter></entity></fetch>";


                       
                        EntityCollection ece_report = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    
                        if (ece_report.Entities.Count > 0)
                        {
                            var version = Convert.ToInt32(ece_report.Entities[0].Attributes["ccof_version"]) + 1;
                            entity["ccof_version"] = version;
                            if (version < 10)
                                entity["ccof_name"] = base_Report["ccof_name"].ToString().Split('-').First() + "-0" + (version);
                            else entity["ccof_name"] = base_Report["ccof_name"].ToString().Split('-').First() + "-" + (version);



                        }
                        else
                        {
                            tracingService.Trace("base report first part" + ((EntityReference)entity["ccof_base_report_id"]).Name);

                            entity["ccof_name"] = base_Report["ccof_name"].ToString().Split('-').First() + "-02";
                            entity["ccof_version"] = 2;

                        }


                    }
                    else
                    {
                        tracingService.Trace("base report main"+entity.GetAttributeValue<EntityReference>("ccof_facility").Id);
                          var name = DateTime.UtcNow.Date.ToString("MMyyyy");
                        if(facility.Attributes.Contains("accountnumber"))
                        name = name + Convert.ToString(facility["accountnumber"]).Split('-').Last() + "-01";
                        
                         entity["ccof_name"] = name.ToString();
                        entity["ccof_version"] = 1;
                     


                    }
                    tracingService.Trace("Entering in fetch");

                    var ecefac_fetch = $"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                    "<entity name='ccof_adjudication_ecewe_facility'>" +
                    "<attribute name='ccof_adjudication_ecewe_facilityid'/>" +
                    "<attribute name='ccof_optinstartmonth'/>" +
                    "<attribute name='createdon'/>" +
                    "<order attribute = 'createdon' descending = 'true'/>" +
                    "<filter type='and'>" +
                    "<condition attribute='ccof_facility' operator='eq'  uitype='account' value='"+facility.Id+"'/>" +
                    "<condition attribute='statuscode' operator='in'>" +
                    "<value>5</value>" +
                    "<value>4</value>" +
                    "</condition>" +
                    "</filter>" +
                    "<link-entity name='ccof_application' from='ccof_applicationid' to='ccof_areyouapublicsectoremployer' link-type='inner' alias='am'>" +
                    "<link-entity name='ccof_program_year' from='ccof_program_yearid' to='ccof_programyear' link-type='inner' alias='program'>" +
                    "<filter type='and'>" +
                    "<condition attribute='ccof_intakeperiodstart' operator='on-or-before' value='"+ DateTime.UtcNow.Date.ToString("yyyy-MM-dd") + "'/>" +
                    "<filter type='or'>" +
                    "<condition attribute='ccof_intakeperiodend' operator='on-or-after' value='"+ DateTime.UtcNow.Date.ToString("yyyy-MM-dd") + "'/>" +
                    "<condition attribute='ccof_intakeperiodend' operator='null'/>" +
                    "</filter>" +
                    "</filter>" +
                    "</link-entity>" +
                    "</link-entity>" +
                    "</entity></fetch>";
                    tracingService.Trace("base ECE Fac fetch" + ecefac_fetch);
                    EntityCollection ecefac = service.RetrieveMultiple(new FetchExpression(ecefac_fetch));
                    //tracingService.Trace("base ECE Fac" + ecefac.Entities[0].Id);

                    if (ecefac.Entities.Count > 0)
                    {
                        entity["ccof_ece_we_facility_id"] = new EntityReference("ccof_adjudication_ecewe_facility", ecefac.Entities[0].Id);


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
