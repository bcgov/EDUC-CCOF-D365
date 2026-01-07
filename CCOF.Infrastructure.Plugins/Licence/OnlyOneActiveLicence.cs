using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCOF.Infrastructure.Plugins.Licence

{
    public class OnlyOneActiveLicence : IPlugin
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
                context.InputParameters["Target"] is Entity target)
            {
                IOrganizationServiceFactory serviceFactory =
                  (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                var isCreate = context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase);
                var isUpdate = context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase);

                Guid recordId = context.PrimaryEntityId;
                Entity current = null;

                var qe = new QueryExpression("ccof_license")
                {
                    ColumnSet = new ColumnSet("ccof_licenseid")
                };
                if (isUpdate)
                {
                    var colset = new ColumnSet("ccof_facility", "ccof_record_start_date", "ccof_record_end_date");
                    current = service.Retrieve("ccof_license", recordId, colset);
                    qe.Criteria.AddCondition("ccof_license" + "id", ConditionOperator.NotEqual, recordId);
                }

                var facilityRef =
                    target.GetAttributeValue<EntityReference>("ccof_facility") ??
                    current?.GetAttributeValue<EntityReference>("ccof_facility");
                qe.Criteria.AddCondition("ccof_facility", ConditionOperator.Equal, facilityRef.Id);
                // Commented by Harpreet, This line is not allowing to create new version with DRAFT status
                //qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                if (target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 101510001)//ACTIVE
                {
                    qe.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 101510001);//ACTIVE
                    var existingActive = service.RetrieveMultiple(qe).Entities.FirstOrDefault();
                    if (existingActive != null)
                    {
                        throw new InvalidPluginExecutionException("This facility already has an active licence version. End date the active version before proceeding.");
                    }
                }

                if (target.GetAttributeValue<DateTime?>("ccof_record_start_date") == null && target.GetAttributeValue<DateTime?>("ccof_record_end_date") == null)
                    return;
                if (target.Contains("ccof_record_start_date") || target.Contains("ccof_record_end_date"))
                {
                    DateTime? start = GetDate(target, "ccof_record_start_date") ?? GetDate(current, "ccof_record_start_date");
                    DateTime? end = GetDate(target, "ccof_record_end_date") ?? GetDate(current, "ccof_record_end_date");
                    var endNotBeforeThisStartOrNull = new FilterExpression(LogicalOperator.Or);

                    endNotBeforeThisStartOrNull.AddCondition("ccof_record_end_date", ConditionOperator.OnOrAfter, start.Value);
                    endNotBeforeThisStartOrNull.AddCondition("ccof_record_end_date", ConditionOperator.Null);

                    qe.Criteria.AddFilter(endNotBeforeThisStartOrNull);

                    if (end.HasValue)
                    {
                        qe.Criteria.AddCondition("ccof_record_start_date", ConditionOperator.OnOrBefore, end.Value);
                    }
                    else
                    {
                        qe.Criteria.AddCondition("ccof_record_start_date", ConditionOperator.OnOrBefore, start.Value);
                    }
                    var licenceRecord = service.RetrieveMultiple(qe).Entities;
                    var anyOverlap = licenceRecord.Any();
                    if (anyOverlap)
                    {
                        tracingService.Trace("Licence ID : " + licenceRecord[0].Attributes["ccof_licenseid"]);
                        throw new InvalidPluginExecutionException("The dates of this licence overlap with another version of this licence");
                    }
                }

                tracingService.Trace("Plugin activity end.");
            }
        }

        private static DateTime? GetDate(Entity e, string attr)
        {
            if (e == null || !e.Attributes.Contains(attr)) return null;
            var val = e[attr];
            if (val is DateTime dt) return dt;
            return null;
        }
    }
}
