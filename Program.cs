using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.Logger.LogInformation("Adding Routes");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Logger.LogInformation("Starting the app");

var appResourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("dotnet-web-simple", "1.0");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(appResourceBuilder);
        options.AddOtlpExporter(option =>
        {
            option.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            option.Endpoint = new Uri("https://otlp-custom-https-otel.apps.cluster-hvnhl.hvnhl.sandbox2235.opentlc.com/");
            // option.Endpoint = new Uri("https://otel-collector.otel.svc.cluster.local:4317");
            
        });
    });
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogDebug("This is a debug message from dotnet-web-simple", LogLevel.Debug);
logger.LogInformation("Information messages from dotnet-web-simple are used to provide contextual information", LogLevel.Information);
logger.LogError(new Exception("Application exception"), "dotnet-web-simple ==> These are usually accompanied by an exception");

app.Run();
