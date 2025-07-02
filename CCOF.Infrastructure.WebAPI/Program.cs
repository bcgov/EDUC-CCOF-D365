using CCOF.Infrastructure.WebAPI.Caching;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using CCOF.Infrastructure.WebAPI.Services.D365WebAPI;
using CCOF.Infrastructure.WebAPI.Services.Processes.Payments;
using CCOF.Infrastructure.WebAPI.Services.Processes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using CCOF.Infrastructure.WebAPI.Services.Documents;
using CCOF.Infrastructure.WebAPI.Services.Batches;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Hellang.Middleware.ProblemDetails;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions => serverOptions.AddServerHeader = false);
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.AddSwaggerApiKeySecurity(builder.Configuration);
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
    config.IncludeXmlComments(xmlCommentsFullPath);
});
builder.Services.AddCustomProblemDetails();
builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();
builder.Services.TryAddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));
builder.Services.TryAddSingleton(TimeProvider.System);
builder.Services.AddScoped<ID365AuthenticationService, AuthenticationServiceMSAL>();
builder.Services.AddScoped<ID365WebApiService, D365WebApiService>();
builder.Services.AddScoped<ID365TokenService, D365TokenService>();
builder.Services.AddScoped<ID365AppUserService, D365AppUserService>();

builder.Services.AddScoped<ID365ScheduledProcessService, ProcessService>();

builder.Services.AddScoped<ID365DataService, D365DataService>();

builder.Services.AddScoped<ID365BackgroundProcessHandler, D365BackgroundProcessHandler>();

builder.Services.AddScoped<ID365BatchService, D365BatchService>();
builder.Services.AddScoped<ID365BatchProvider, BatchProvider>();

builder.Services.AddScoped<ID365ProcessProvider, P505GeneratePaymentLinesProvider>();

builder.Services.AddD365HttpClient(builder.Configuration);
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.AddAuthentication();
builder.Services.AddHealthChecks();


//======== Configuration >>>
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection(nameof(AuthenticationSettings)));
builder.Services.Configure<D365AuthSettings>(builder.Configuration.GetSection(nameof(D365AuthSettings)));
builder.Services.Configure<DocumentSettings>(builder.Configuration.GetSection(nameof(DocumentSettings)));
builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection(nameof(NotificationSettings)));
builder.Services.Configure<ProcessSettings>(builder.Configuration.GetSection(nameof(ProcessSettings)));
builder.Services.Configure<ExternalServices>(builder.Configuration.GetSection(nameof(ExternalServices)));
//======== <<<


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseApiKey();
app.UseProblemDetails();

app.MapFallback(() => Results.Redirect("/swagger"));
app.UseHttpsRedirection();
app.UseAuthentication();


app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();
app.RegisterBatchProcessesEndpoints();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
