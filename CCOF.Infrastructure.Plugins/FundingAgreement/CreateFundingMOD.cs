using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCOF.Infrastructure.Plugins.FundingAgreement

{
    public class CreateFundingMOD : IPlugin
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
                IOrganizationServiceFactory serviceFactory =
                  (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                var recordId = context.PrimaryEntityId;
                try
                {
                    Entity entity = service.Retrieve("account", recordId, new ColumnSet("ccof_base_funding_number", "statuscode"));

                    if (entity != null && entity.Attributes.Count > 0)
                    {


                        

                        var fetchXml = $@"<fetch version='1.0' mapping='logical' no-lock='false' distinct='true'>
                                     <entity name='ccof_funding_agreement'>
                                        <attribute name='statecode'/>
                                      <attribute name='ccof_funding_agreementid'/>
                                          <attribute name='ccof_name'/>
                                          <attribute name='createdon'/>
                                          <attribute name='ccof_programyear'/>
                                          <attribute name='ccof_start_date'/>
                                          <attribute name='ccof_end_date'/>                                        
                                          <attribute name='ccof_organization'/>
                                           <attribute name='ccof_facility'/>
                                            <attribute name='ccof_application'/>
                                           <attribute name='ccof_maximum_contract_amount'/>
                                           <attribute name='ccof_version'/>
                                          <order attribute='ccof_version' descending='true'/>
                                        <filter type='and'><condition attribute='statecode' operator='eq' value='0'/>
                                         <condition attribute='ccof_organization' operator='eq' value='{recordId}'  uitype='account'/>
                                         </filter></entity></fetch>
                            ";

                        EntityCollection colfunding = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        Entity funding = colfunding[0]; //the first return result

                        tracingService.Trace("This funding agreement: " + funding["ccof_name"]);

                        #region Clone Funding Record and tag license
                        Entity newFundingRecord = new Entity("ccof_funding_agreement");
                        var excludedFields = new List<string>{funding.LogicalName + "id", "createdon", "createdby", "modifiedon", "modifiedby",
                                                     "ownerid", "owninguser", "owningbusinessunit", "statecode", "statuscode","ccof_declaration","ccof_ready_for_provider_action","ccof_sp_primary_contact_name","ccof_date_signed_ministry","ccof_ministry_ea_adjudicator_name","ccof_date_signed_sp"
                                                      };

                        foreach (var attr in funding.Attributes.Where(a => !excludedFields.Contains(a.Key)))
                        {
                            if (attr.Value is EntityReference entityRef)
                            {
                                newFundingRecord[attr.Key] = new EntityReference(entityRef.LogicalName, entityRef.Id);
                            }
                            else if (attr.Key.Equals("ccof_version"))
                            {
                                newFundingRecord[attr.Key] = Convert.ToInt32(attr.Value) + 1;

                            }
                            else { 
                            tracingService.Trace("All Funding  fields{0}:{1} ", attr.Key, attr.Value);
                            newFundingRecord[attr.Key] = attr.Value;
                        }
                        }

                        
                        Guid fundingID = service.Create(newFundingRecord);

                      
                        if (funding.Contains("ccof_facility"))
                        {
                            fetchXml = $@"<fetch version=""1.0"" mapping=""logical"" distinct=""true"">
                                         <entity name=""ccof_license"">
                                         <attribute name=""statecode""/>
                                         <attribute name=""ccof_licenseid""/>
                                         <attribute name=""ccof_name""/>
                                        <order attribute=""ccof_name"" descending=""false""/>
                                        <attribute name=""ccof_facility""/>
                                       <filter type=""and"">
                                       <condition attribute=""statecode"" operator=""eq"" value=""0""/>
                                      <condition attribute=""ccof_facility"" operator=""eq"" value=""{((EntityReference)funding["ccof_facility"]).Id}"" uitype=""account""/>
                                        <condition attribute=""statuscode"" operator=""eq"" value=""100000001""/></filter>
                                      </entity>
                                       </fetch>
                            ";

                            EntityCollection colLicense = service.RetrieveMultiple(new FetchExpression(fetchXml));
                            // Entity license = colLicense[0]; //the first return result

                            // tracingService.Trace("This funding agreement: " + license["ccof_name"]);
                            foreach (var license in colLicense.Entities)
                            {
                                Entity updateLicense = new Entity("ccof_license");
                                updateLicense["ccof_licenseid"] = license.Id;
                                updateLicense["ccof_associated_funding_agreement_number"] = new EntityReference("ccof_funding_agreement", fundingID);
                                service.Update(updateLicense);
                            }
                        }
                        #endregion
                        #region Deactivate old Funding
                        var deactivateRequest = new OrganizationRequest("SetState")
                        {
                            ["EntityMoniker"] = new EntityReference("ccof_funding_agreement", funding.Id),
                            ["State"] = new OptionSetValue(1),     // 1 = Inactive
                            ["Status"] = new OptionSetValue(101510007)    //  2 = Replaced
                        };

                        // Execute the request
                        service.Execute(deactivateRequest);
                        tracingService.Trace("Deactivate Funding  agreement: " + funding["ccof_name"]);
                        #endregion


                        tracingService.Trace("\nUpdate Agreement Number Base and create first Funding record successfully.");



                    }
                    tracingService.Trace("Plugin activity end.");
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("Exeception in Plugin -" + ex.Message + ex.InnerException);
                }
            }
        }
    }
}
