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
using BlazorExample.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// OpenTelemetry
var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

// Switch between Zipkin/Jaeger/OTLP by setting UseExporter in appsettings.json.
var tracingExporter = builder.Configuration.GetValue<string>("UseTracingExporter").ToLowerInvariant();

var resourceBuilder = tracingExporter switch
{
    "jaeger" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Jaeger:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    "zipkin" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Zipkin:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    "otlp" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Otlp:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    _ => ResourceBuilder.CreateDefault().AddService(ClientSemantics.ServiceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
};

// Traces
builder.Services.AddOpenTelemetryTracing(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    options.SetSampler(new AlwaysOnSampler());
    // options.AddSource(ClientSemantics.UiActivity);
    options.AddHttpClientInstrumentation(c => c.RecordException = true);
    options.AddConsoleExporter();
    //switch (tracingExporter)
    //{
    //    // case "jaeger":
    //    //     options.AddJaegerExporter();
    //    //
    //    //     builder.Services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));
    //    //
    //    //     // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
    //    //     builder.Services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
    //    //     break;
    //    //
    //    // case "zipkin":
    //    //     options.AddZipkinExporter();
    //    //
    //    //     builder.Services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));
    //    //     break;

    //    case "otlp":
    //        options.AddOtlpExporter(otlpOptions =>
    //        {
    //            otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
    //            otlpOptions.ExportProcessorType = ExportProcessorType.Simple;
    //            otlpOptions.Endpoint = new Uri($"{builder.Configuration.GetValue<string>("Otlp:Endpoint")}/v1/traces");
    //        });
    //        break;

    //    default:
    //        options.AddConsoleExporter();

    //        break;
    //}
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
