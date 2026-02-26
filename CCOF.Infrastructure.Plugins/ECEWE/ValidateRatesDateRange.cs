using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCOF.Infrastructure.Plugins.ECEWE
{
    public class ValidateRatesDateRange : IPlugin
    {


        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            if (!context.InputParameters.Contains("Target"))
                return;

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != "ccof_ece_rate")
                return;

            // Get full image for updates
            Entity full = target;
            if (context.MessageName == "Update" && context.PreEntityImages.Contains("PreImage"))
            {
                full = context.PreEntityImages["PreImage"];
            }

            // Extract start date
            DateTime start = target.Contains("ccof_effective_start_date")
                ? target.GetAttributeValue<DateTime>("ccof_effective_start_date")
                : full.GetAttributeValue<DateTime>("ccof_effective_start_date");

            // Extract end date (nullable)
            DateTime? end = null;
            if (target.Contains("ccof_effective_end_date"))
                end = target.GetAttributeValue<DateTime?>("ccof_effective_end_date");
            else
                end = full.GetAttributeValue<DateTime?>("ccof_effective_end_date");

            // Treat null end date as infinite future for overlap logic
            DateTime endForComparison = end ?? DateTime.MaxValue;

            Guid currentId = full.Id;

            // ------------------------------------------------------------
            // 1. VALIDATE OVERLAP (null-safe)
            // ------------------------------------------------------------
            var overlapQuery = new QueryExpression("ccof_ece_rate")
            {
                ColumnSet = new ColumnSet("ccof_effective_start_date", "ccof_effective_end_date")
            };

            overlapQuery.Criteria.AddCondition("ccof_effective_start_date", ConditionOperator.LessEqual, endForComparison);
            overlapQuery.Criteria.AddCondition("ccof_effective_end_date", ConditionOperator.GreaterEqual, start);
            overlapQuery.Criteria.AddCondition("ccof_ece_rateid", ConditionOperator.NotEqual, currentId);

            var overlaps = service.RetrieveMultiple(overlapQuery);

            if (overlaps.Entities.Any())
            {
                throw new InvalidPluginExecutionException(
                    "The date range overlaps with an existing ECE rate. ECE rate periods cannot overlap."
                );
            }

            // ------------------------------------------------------------
            // 2. FIND PREVIOUS RECORD (must include null end dates)
            // ------------------------------------------------------------
            var prevQuery = new QueryExpression("ccof_ece_rate")
            {
                ColumnSet = new ColumnSet("ccof_effective_end_date")
            };

            prevQuery.AddOrder("ccof_effective_start_date", OrderType.Descending);
            
            var prevFilter = new FilterExpression(LogicalOperator.Or);

            // Case 1: previous record has an end date earlier than new start
            prevFilter.AddCondition("ccof_effective_end_date", ConditionOperator.LessThan, start);

            // Case 2: previous record is open-ended (enddate = null)
            prevFilter.AddCondition("ccof_effective_end_date", ConditionOperator.Null);

            

            prevQuery.Criteria.AddFilter(prevFilter);
            prevQuery.Criteria.AddCondition("ccof_ece_rateid", ConditionOperator.NotEqual, target.Id);
            // Order so the closest previous record is selected
            prevQuery.AddOrder("ccof_effective_end_date", OrderType.Descending);
            prevQuery.TopCount = 1;

            var previous = service.RetrieveMultiple(prevQuery).Entities.FirstOrDefault();

            // ------------------------------------------------------------
            // 3. AUTO-ADJUST PREVIOUS RECORD
            // ------------------------------------------------------------
            if (previous != null)
            {
                var updated = new Entity("ccof_ece_rate", previous.Id);
                updated["ccof_effective_end_date"] = start.AddDays(-1);
                service.Update(updated);
            }
        }
    }
    
    
}


 