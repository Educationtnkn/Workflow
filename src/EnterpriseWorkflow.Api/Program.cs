using DB_Provider.Extensions;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.Extensions;
using EnterpriseWorkflow.Api.Stubs;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Core.Extensions;
using EnterpriseWorkflow.Elsa.Adapters;
using EnterpriseWorkflow.Elsa.Extensions;
using EnterpriseWorkflow.Utilities.DbConnectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ND.ExceptionFramework.Extentions;
using ND.Observability.Framework.Core.Application.Services;
using Quartz;


var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;

var allowedOriginCsv =
    Environment.GetEnvironmentVariable("AllowedCors")
    ?? configuration["AllowedCors"];

var allowedOrigins = string.IsNullOrWhiteSpace(allowedOriginCsv)
    ? Array.Empty<string>()
    : allowedOriginCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AppFW API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", policy =>
    {
        policy
                .WithOrigins(allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("*");
    });
});

// Register all services
builder.Services
    .AddWorkflowCore(configuration)                           // Your core
    .AddElsaWorkflowAdapter(configuration)
    .AddRuleEngine();                   // Elsa adapter
                                        //.AddMsRulesEngineAdapter(configuration)                    // Rules engine
                                        //.AddHttpTriggerHandler()                                   // HTTP triggers
                                        //   .AddQueueTriggerHandler(configuration.GetSection("ServiceBus"))
                                        // .AddEfCoreWorkflowStorage(configuration.GetConnectionString("WorkflowDb"))
                                        // .AddMultiTenancy(configuration.GetSection("Tenancy"))
                                        //.AddWorkflowSecurity(configuration.GetSection("Security"));

builder.Services.AddDbProviderServices();

builder.Services.AddNDLogging(builder.Configuration);
builder.Services.AddNDTracing(builder.Configuration);
builder.Services.AddNDMetrics(builder.Configuration);

builder.Host.ConfigureHostOptions(o =>
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<MSSQLConnector>();
builder.Services.AddScoped<SqlServerConnector2>();
builder.Services.AddExceptionHandling();
//builder.Services.AddEnterpriseWorkflowLibrary(builder.Configuration);

//// csharp


//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//// Requires Microsoft.EntityFrameworkCore.SqlServer package
//builder.Services.AddDbContext<EnterpriseWorkflowDbContext>(options =>
//    options.UseSqlServer(connectionString));






//builder.Services
//    .AddElsa(elsa => elsa
//        .UseIdentity(identity =>
//        {
//            identity.TokenOptions = options =>
//                options.SigningKey = configuration["Elsa:SigningKey"]!;
//            identity.UseAdminUserProvider();
//        })
//        .UseDefaultAuthentication()
//        .UseWorkflowManagement(m => m.UseEntityFrameworkCore(ef =>
//            ef.UseSqlServer(configuration.GetConnectionString("ElsaDb")!)))
//        .UseWorkflowRuntime(r => r.UseEntityFrameworkCore(ef =>
//            ef.UseSqlServer(configuration.GetConnectionString("ElsaDb")!)))
//        .UseScheduling(scheduling => scheduling.UseQuartzScheduler())
//        .UseJavaScript()
//        .UseCSharp()
//        .UseWorkflowsApi()
//        .UseHttp(http => http.ConfigureHttpOptions = options =>
//            configuration.GetSection("Http").Bind(options))
//        .AddActivitiesFrom<EvaluateRuleActivity>()
//        .AddActivitiesFrom<Program>());
var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<WorkflowLogDbContext>();
//    await db.Database.MigrateAsync(); // applies pending migrations 


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AppFW API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast =  Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast")
//.WithOpenApi();


app.MapControllers();

app.Run();

