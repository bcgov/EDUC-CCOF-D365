using CCOF.Infrastructure.WebAPI.Caching;
using CCOF.Infrastructure.WebAPI.Extensions;
using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.AppUsers;
using CCOF.Infrastructure.WebAPI.Services.D365WebApi;
using CCOF.Infrastructure.WebAPI.Services.D365WebAPI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ID365WebAPIService, D365WebAPIService>();
builder.Services.AddScoped<ID365TokenService, D365TokenService>();
builder.Services.AddScoped<ID365AppUserService, D365AppUserService>();


builder.Services.Configure<D365AuthSettings>(builder.Configuration.GetSection(nameof(D365AuthSettings)));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
