using CCOF.Core.DataContext;
using Newtonsoft.Json;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Handlers;
using CCOF.Infrastructure.WebAPI.Messages;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Net;

namespace CCOF.Infrastructure.WebAPI.Services.Processes.ECER
{
    public class P700ECEREmployeeCertificates(IOptionsSnapshot<ExternalServices> ECERSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory, TimeProvider timeProvider) : ID365ProcessProvider
    {
        private readonly ID365AppUserService _appUserService = appUserService;
        private readonly ID365WebApiService _d365webapiservice = d365WebApiService;
        private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);
        private readonly TimeProvider _timeProvider = timeProvider;
        private ProcessParameter? _processParams;
        private ProcessData? _data;
        private Guid dataImportID = Guid.Empty;
        private readonly ECERSettings _ECERSettings = ECERSettings.Value.ECERApi;
        public Int16 ProcessId => Setup.Process.ECER.ProcessECEREmployeeCertificatesId;
        public string ProcessName => Setup.Process.ECER.ProcessECEREmployeeCertificatesName;


        public async Task<ProcessData> GetDataAsync()
        {
            _logger.LogDebug(CustomLogEvent.Process, "Calling GetData of {nameof}", nameof(P700ECEREmployeeCertificates));

            return await Task.FromResult(_data);
        }

        private string DataImportActiveRequestUri
        {
            get
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""ofm_data_import"">
                        <attribute name=""ofm_name"" />
                        <attribute name=""statecode"" />
                        <filter>
                          <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                        </filter>
                      </entity>
                    </fetch>";
                var requestUri = $"""
                            ofm_data_imports?fetchXml={WebUtility.UrlEncode(fetchXml)}
                            """;

                return requestUri.CleanCRLF();
            }
        }

        public async Task<List<JsonNode>> FetchAllRecordsFromCRMAsync(string requestUri)
        {
            _logger.LogDebug(CustomLogEvent.Process, "Getting records with query {requestUri}", requestUri.CleanLog());
            var allRecords = new List<JsonNode>();  // List to accumulate all records
            string nextPageLink = requestUri;  // Initial request URI
            do
            {
                // 5000 is limit number can retrieve from crm
                var response = await _d365webapiservice.SendRetrieveRequestAsync(_appUserService.AZSystemAppUser, nextPageLink, false, 5000, isProcess: false);
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(CustomLogEvent.Process, "Failed to query records with server error {responseBody}", responseBody.CleanLog());
                    var returnJsonNodeList = new List<JsonNode>();
                    returnJsonNodeList.Add(responseBody);
                    return returnJsonNodeList;
                    // null;
                }
                var jsonObject = await response.Content.ReadFromJsonAsync<JsonObject>();
                JsonNode currentBatch = string.Empty;
                if (jsonObject?.TryGetPropertyValue("value", out var currentValue) == true)
                {
                    if (currentValue?.AsArray().Count == 0)
                    {
                        _logger.LogInformation(CustomLogEvent.Process, "No more records found with query {nextPageLink}", nextPageLink.CleanLog());
                        break;  // Exit the loop if no more records
                    }
                    currentBatch = currentValue!;
                    allRecords.AddRange(currentBatch.AsArray());  // Add current batch to the list
                }
                _logger.LogDebug(CustomLogEvent.Process, "Fetched {batchSize} records. Total records so far: {totalRecords}", currentBatch.AsArray().Count, allRecords.Count);

                // Check if there's a next link in the response for pagination
                nextPageLink = null;
                if (jsonObject?.TryGetPropertyValue("@odata.nextLink", out var nextLinkValue) == true)
                {
                    nextPageLink = nextLinkValue.ToString();
                }
            }
            while (!string.IsNullOrEmpty(nextPageLink));

            _logger.LogDebug(CustomLogEvent.Process, "Total records fetched: {totalRecords}", allRecords.Count);
            return allRecords;
        }

        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)
        {
            var startTime = _timeProvider.GetTimestamp();
            _logger.LogInformation(CustomLogEvent.Process, "Beginning to process P700 Data process at {startTime}", startTime);
            _processParams = processParams;
            string dataImportMessages = string.Empty;
            string upsertMessages = string.Empty;
            string deactiveMessages = string.Empty;
            bool upsertSucessfully = false;
            bool deactiveSucessfully = false;
            
            try
            {
                #region Connect with ECER API 

                using (var client = new HttpClient())
                {
                    // ===========================
                    // Step 1. Retrieve an access token
                    // ===========================
                    var tokenUrl = _ECERSettings.InterfaceURL;
                    var tokenRequestBody = "grant_type=client_credentials&client_id=" + _ECERSettings.ClientId + "&client_secret=" + _ECERSettings.ClientSecret;

                    var tokenResult = await ECERAPIHandler.GetTokenAsync(tokenUrl, tokenRequestBody);

                    // Set the authorization header for subsequent API calls
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult);

                    // ===========================
                    // Step 2. Retrieve certification files list
                    // ===========================
                    var filesUrl = string.Concat(_ECERSettings.ECERURL, "s");
                    HttpResponseMessage filesResponse = await client.GetAsync(filesUrl);
                    filesResponse.EnsureSuccessStatusCode();

                    string filesContent = await filesResponse.Content.ReadAsStringAsync();
                    List<CertificationFile> certificationFiles = JsonConvert.DeserializeObject<List<CertificationFile>>(filesContent);

                    if (certificationFiles == null || certificationFiles.Count == 0)
                    {
                        return ProcessResult.Failure(ProcessId, new String[] { "ECER Certificate is not found" }, 0, 0).SimpleProcessResult;

                    }

                    // ===========================
                    // Step 3. Download certification details from the first file
                    // ===========================
                    string firstFileId = certificationFiles[0].id;
                    Console.WriteLine("Retrieving details for file id: " + firstFileId);

                    var downloadUrl = string.Concat(_ECERSettings.ECERURL, "/download/", firstFileId);// $"https://dev-ecer-api.apps.silver.devops.gov.bc.ca/api/certifications/file/download/{firstFileId}";
                    HttpResponseMessage downloadResponse = await client.GetAsync(downloadUrl);
                    downloadResponse.EnsureSuccessStatusCode();

                    string downloadContent = await downloadResponse.Content.ReadAsStringAsync();
                    bool savePFEResult = await SaveImportFile(appUserService, d365WebApiService, downloadContent);
                    List<CertificationDetail> certificationDetails = JsonConvert.DeserializeObject<List<CertificationDetail>>(downloadContent);
                    var upsertECERequests = new List<HttpRequestMessage>() { };
                    foreach (var record in certificationDetails)
                    {
                        var ECECert = new JsonObject
                        {
                            { "ofm_first_name", record?.firstname},
                            { "ofm_last_name", record?.lastname},
                            { "ofm_effective_date", record?.effectivedate?.ToString("yyyy-MM-dd")},
                            { "ofm_expiry_date", record?.expirydate?.ToString("yyyy-MM-dd")},
                            { "statecode", 0 }
                        };

                        if (!String.IsNullOrEmpty(record?.certificatelevel) && !String.IsNullOrEmpty(record.registrationnumber))
                        {
                            upsertECERequests.Add(new UpsertRequest(new D365EntityReference("ofm_employee_certificates(ofm_certificate_number='" + record?.registrationnumber + "',ofm_certificate_level='" + record?.certificatelevel?.Replace(",", " ").ToString() + "')"), ECECert));
                        }
                    }
                    var upsertECECertResults = await d365WebApiService.SendBatchMessageAsync(_appUserService.AZSystemAppUser, upsertECERequests, null);
                    if (upsertECECertResults.Errors.Any())
                    {
                        var errorInfos = ProcessResult.Failure(ProcessId, upsertECECertResults.Errors, upsertECECertResults.TotalProcessed, upsertECECertResults.TotalRecords);

                        _logger.LogError(CustomLogEvent.Process, "Failed to Upsert ECE Certification: {error}", JsonValue.Create(errorInfos)!.ToString());
                        upsertMessages += "Batch Upsert errors: " + JsonValue.Create(errorInfos) + "\n\r";
                    }

                    if (string.IsNullOrEmpty(upsertMessages))
                    {
                        upsertSucessfully = true;
                    }
                    else
                    {
                        dataImportMessages += dataImportMessages + "Upsert records Failed \r\n";
                    }

                    if (upsertSucessfully)
                    {
                        var localtime = _timeProvider.GetLocalNow();
                        TimeZoneInfo PSTZone = GetPSTTimeZoneInfo("Pacific Standard Time", "America/Los_Angeles");
                        var pstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PSTZone);
                        var endtime = _timeProvider.GetTimestamp();
                        var timediff = _timeProvider.GetElapsedTime(startTime, endtime).TotalSeconds;
                        dataImportMessages = pstTime.ToString("yyyy-MM-dd HH:mm:ss") + " Total time:" + Math.Round(timediff, 2) + " seconds.\r\n";

                        var ECECertStatement = $"ofm_data_imports({dataImportID})";
                        var payload = new JsonObject {
                            { "ofm_message", dataImportMessages},
                            { "statuscode", 4},
                            { "statecode", 0 }
                        };
                        var requestBody = System.Text.Json.JsonSerializer.Serialize(payload);
                        var patchResponse = await d365WebApiService.SendPatchRequestAsync(appUserService.AZSystemAppUser, ECECertStatement, requestBody);
                        if (!patchResponse.IsSuccessStatusCode)
                        {
                            var responseBody = await patchResponse.Content.ReadAsStringAsync();
                            _logger.LogError(CustomLogEvent.Process, "Failed to patch the record with the server error {responseBody}", responseBody.CleanLog());
                            return ProcessResult.Failure(ProcessId, new String[] { responseBody }, 0, 0).SimpleProcessResult;
                        }
                        // Deactive Previous Data Imports 
                        List<JsonNode> allActiveDataImports = await FetchAllRecordsFromCRMAsync(DataImportActiveRequestUri);
                        allActiveDataImports = allActiveDataImports.Where(item => !item["ofm_data_importid"].ToString().Equals(dataImportID.ToString())).ToList();
                        foreach (var dataImport in allActiveDataImports)
                        {
                            var deactiveDataImport = $"ofm_data_imports({dataImport["ofm_data_importid"].ToString()})";
                            payload = new JsonObject {
                                { "statecode", 1 }
                            };
                            requestBody = System.Text.Json.JsonSerializer.Serialize(payload);
                            patchResponse = await d365WebApiService.SendPatchRequestAsync(appUserService.AZSystemAppUser, deactiveDataImport, requestBody);
                            if (!patchResponse.IsSuccessStatusCode)
                            {
                                var responseBody = await patchResponse.Content.ReadAsStringAsync();
                                _logger.LogError(CustomLogEvent.Process, "Failed to patch the record with the server error {responseBody}", responseBody.CleanLog());
                                return ProcessResult.Failure(ProcessId, new String[] { responseBody }, 0, 0).SimpleProcessResult;
                            }
                        }
                    }
                    else
                    {
                        var ECECertStatement = $"ofm_data_imports({_processParams.DataImportId})";
                        var payload = new JsonObject {
                        { "ofm_message", dataImportMessages},
                        { "statuscode", 5},
                        { "statecode", 0 }
                    };
                        var requestBody = System.Text.Json.JsonSerializer.Serialize(payload);
                        var patchResponse = await d365WebApiService.SendPatchRequestAsync(appUserService.AZSystemAppUser, ECECertStatement, requestBody);
                        if (!patchResponse.IsSuccessStatusCode)
                        {
                            var responseBody = await patchResponse.Content.ReadAsStringAsync();
                            _logger.LogError(CustomLogEvent.Process, "Failed to patch the record with the server error {responseBody}", responseBody.CleanLog());
                            return ProcessResult.Failure(ProcessId, new String[] { responseBody }, 0, 0).SimpleProcessResult;
                        }
                        return ProcessResult.Failure(ProcessId, new String[] { "Upsert action failed" }, 0, 0).SimpleProcessResult;
                    }
                    #endregion

                    return ProcessResult.Completed(ProcessId).SimpleProcessResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var returnObject = ProcessResult.Failure(ProcessId, new String[] { "Critical error", ex.StackTrace }, 0, 0).ODProcessResult;
                return returnObject;
            }
        }

        private async Task<bool> SaveImportFile(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, string result)
        {
            var importfileName = ("Provider Certificate" + "-" + DateTime.UtcNow.ToLocalPST().ToString("yyyyMMddHHmmss"));
            var requestBody = new JsonObject()
            {
                ["ofm_name"] = importfileName,
                ["ofm_import_type"] = 1
            };

            var pfeCreateResponse = await d365WebApiService.SendCreateRequestAsync(appUserService.AZSystemAppUser, OfM_Data_Import.EntitySetName, requestBody.ToString());

            if (!pfeCreateResponse.IsSuccessStatusCode)
            {
                var pfeCreateError = await pfeCreateResponse.Content.ReadAsStringAsync();
                _logger.LogError(CustomLogEvent.Process, "Failed to create payment file exchange record with the server error {responseBody}", JsonValue.Create(pfeCreateError)!.ToString());

                return await Task.FromResult(false);
            }

            var pfeRecord = await pfeCreateResponse.Content.ReadFromJsonAsync<JsonObject>();

            if (pfeRecord is not null && pfeRecord.ContainsKey(OfM_Data_Import.Fields.OfM_Data_ImportId))
            {
                dataImportID = new Guid(pfeRecord[OfM_Data_Import.Fields.OfM_Data_ImportId].ToString());
                if (importfileName.Length > 0)
                {
                    // Update the new Payment File Exchange record with the new document
                    HttpResponseMessage pfeUpdateResponse = await _d365webapiservice.SendDocumentRequestAsync(_appUserService.AZPortalAppUser, OfM_Data_Import.EntitySetName,
                                                                                                        new Guid(pfeRecord[OfM_Data_Import.Fields.OfM_Data_ImportId].ToString()),
                                                                                                        Encoding.ASCII.GetBytes(result.TrimEnd()),
                                                                                                        importfileName);

                    if (!pfeUpdateResponse.IsSuccessStatusCode)
                    {
                        var pfeUpdateError = await pfeUpdateResponse.Content.ReadAsStringAsync();
                        _logger.LogError(CustomLogEvent.Process, "Failed to update data import document with ECER file with the server error {responseBody}", pfeUpdateError.CleanLog());

                        return await Task.FromResult(false);
                    }
                }
            }
            return await Task.FromResult(true);
        }

        public TimeZoneInfo GetPSTTimeZoneInfo(string timezoneId1, string timezoneId2)
        {
            try
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(timezoneId1);

                return info;
            }
            catch (System.TimeZoneNotFoundException)
            {
                try
                {
                    TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(timezoneId2);

                    return info;
                }
                catch (System.TimeZoneNotFoundException)
                {
                    _logger.LogError(CustomLogEvent.Process, "Could not find timezone by Id");
                    return null;
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}