//using ECC.Core.DataContext;
using System.Text.Json.Serialization;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;

using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using CCOF.Infrastructure.WebAPI.Messages;
using Microsoft.Extensions.Options;

namespace CCOF.Infrastructure.WebAPI.Services.Processes.Payments
{
    public class P505GeneratePaymentLinesProvider(IOptionsSnapshot<ExternalServices> bccasApiSettings, ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ILoggerFactory loggerFactory, TimeProvider timeProvider) : ID365ProcessProvider
    {
        private readonly BCCASApi _BCCASApi = bccasApiSettings.Value.BCCASApi;
        private readonly ID365AppUserService _appUserService = appUserService;
        private readonly D365WebApi.ID365WebApiService _d365WebApiService = d365WebApiService;
        private readonly ILogger _logger = loggerFactory.CreateLogger(LogCategory.Process);

        private readonly TimeProvider _timeProvider = timeProvider;
        private ProcessParameter? _processParams;


        public Int16 ProcessId => Setup.Process.Payments.GeneratePaymentLinesId;
        public string ProcessName => Setup.Process.Payments.GeneratePaymentLinesName;

        public Task<ProcessData> GetDataAsync()
        {
            throw new NotImplementedException();
        }

        #region Data Queries




        #endregion



        public async Task<JsonObject> RunProcessAsync(ID365AppUserService appUserService, ID365WebApiService d365WebApiService, ProcessParameter processParams)

        #region Validation & Setup



        {
            return ProcessResult.Completed(ProcessId).SimpleProcessResult;
        }
    

        

            #endregion

           


       

    }
   

}