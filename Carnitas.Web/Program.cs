using Carnitas.Extensions;
using Carnitas.Model;
using Carnitas.Model.Identity;
using Carnitas.Model.Operations;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Carnitas.Web.Components;
using Carnitas.Web.Components.Account;
using Carnitas.Web.Grpc;
using MudBlazor.Services;
using NLog.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddNLog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks();

// builder.Services.AddCarnitas(builder.Configuration);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

///////////////////////
//   TASK QUEUE
///////////////////////

var taskQueueOptions = builder.Configuration.GetSection(TaskQueueOptions.Key).Get<TaskQueueOptions>()
                       ?? new TaskQueueOptions();
builder.Services.AddSingleton(taskQueueOptions);
builder.Services.AddScoped<ITaskQueue, TaskQueueService>();
builder.Services.AddHostedService<TaskQueueMaintenanceService>();

///////////////////////
//   OBSERVABILITY
///////////////////////

builder.Services.AddOpenTelemetry()
    .WithLogging(b =>
    {
        b.AddOtlpExporter();
    })
    .WithMetrics(b =>
    {
        b.AddMeter("Sarsoo.*");
        b.AddMeter("Carnitas.*");
        b.AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    })
    .WithTracing(b =>
    {
        b.AddSource("Sarsoo.*");
        b.AddSource("Carnitas.*");
        b.AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGrpcService<AgentService>();
app.MapGrpcReflectionService();
app.MapGrpcHealthChecksService();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();