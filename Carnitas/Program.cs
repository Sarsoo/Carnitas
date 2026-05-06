using Carnitas.Components;
using MudBlazor.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddOpenTelemetry()
    .WithLogging(b =>
    {
        b.AddOtlpExporter();
    })
    .WithMetrics(b =>
    {
        b.AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    })
    .WithTracing(b =>
    {
        b.AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();