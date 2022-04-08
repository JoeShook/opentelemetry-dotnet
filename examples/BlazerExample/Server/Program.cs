// <copyright file="Program.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "BlazorExample_ServerService";

// OpenTelemetry
var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

// Switch between Zipkin/Jaeger/OTLP by setting UseExporter in appsettings.json.
var tracingExporter = builder.Configuration.GetValue<string>("UseTracingExporter").ToLowerInvariant();

var resourceBuilder = tracingExporter switch
{
    "jaeger" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Jaeger:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    "zipkin" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Zipkin:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    "otlp" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Otlp:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    _ => ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
};

// Traces
builder.Services.AddOpenTelemetryTracing(options =>
{
    options
        .SetResourceBuilder(resourceBuilder)
        .SetSampler(new AlwaysOnSampler())
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation();

    switch (tracingExporter)
    {
        case "jaeger":
            options.AddJaegerExporter();

            builder.Services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));

            // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
            builder.Services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
            break;

        case "zipkin":
            options.AddZipkinExporter();

            builder.Services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));
            break;

        case "otlp":
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"));
            });
            break;

        default:
            options.AddConsoleExporter();

            break;
    }
});

// For options which can be bound from IConfiguration.
builder.Services.Configure<AspNetCoreInstrumentationOptions>(
    builder.Configuration.GetSection("AspNetCoreInstrumentation"));

// Logging
// builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;

    var logExporter = builder.Configuration.GetValue<string>("UseLogExporter").ToLowerInvariant();
    switch (logExporter)
    {
        case "otlp":
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"));
            });
            break;
        default:
            options.AddConsoleExporter();
            break;
    }
});

builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
{
    opt.IncludeScopes = true;
    opt.ParseStateValues = true;
    opt.IncludeFormattedMessage = true;
});

// Metrics
builder.Services.AddOpenTelemetryMetrics(options =>
{
    options.SetResourceBuilder(resourceBuilder)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation();

    var metricsExporter = builder.Configuration.GetValue<string>("UseMetricsExporter").ToLowerInvariant();
    switch (metricsExporter)
    {
        case "prometheus":
            options.AddPrometheusExporter();
            break;
        case "otlp":
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"));
            });
            break;
        default:
            options.AddConsoleExporter((exporterOptions, metricReaderOptions) =>
            {
                // The ConsoleMetricExporter defaults to a manual collect cycle.
                // This configuration causes metrics to be exported to stdout on a 10s interval.
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
            });
            break;
    }
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
