using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

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
            // option.Protocol = OtlpExportProtocol.HttpProtobuf;
            // option.Endpoint = new Uri("https://otlp-custom-https-otel.apps.cluster-hvnhl.hvnhl.sandbox2235.opentlc.com/");
            // option.Endpoint = new Uri("http://otel-collector.otel.svc.cluster.local:4318");

            option.Protocol = OtlpExportProtocol.Grpc;
            // option.Endpoint = new Uri("https://otel-custom-grpc-otel.apps.cluster-hvnhl.hvnhl.sandbox2235.opentlc.com"); // Expose gRPC endpoint as route doesn't work!!!
            option.Endpoint = new Uri("http://otel-collector.observability.svc.cluster.local:4317"); // Only gRPC service endpoint works, 
            option.ExportProcessorType = ExportProcessorType.Batch;
            
        });
    });
});

var otel_collector_addr = System.Environment.GetEnvironmentVariable("MY_OTELCOL_COLLECTOR_SERVICE_HOST");
var otel_collector_port = System.Environment.GetEnvironmentVariable("MY_OTELCOL_COLLECTOR_SERVICE_PORT");
var otel_collector_endpoint = "http://" + otel_collector_addr + ":" + otel_collector_port;
Console.WriteLine(otel_collector_endpoint + "        .....writing console output...");

var logger = loggerFactory.CreateLogger<Program>();

logger.LogDebug("This is a debug message from dotnet-web-simple", LogLevel.Debug);
logger.LogInformation("Information messages from dotnet-web-simple are used to provide contextual information", LogLevel.Information);
logger.LogError(new Exception("Application exception"), "dotnet-web-simple ==> These are usually accompanied by an exception");

app.Run();
