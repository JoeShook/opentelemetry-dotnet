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

// using System.Reflection;

using System.Reflection;
using BlazorExample.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenTelemetry.Resources;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

// See OpenTelemetry exporter comment below.

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
// builder.Services.AddOpenTelemetryTracing(options =>
// {
//     options.SetResourceBuilder(resourceBuilder);
//     options.SetSampler(new AlwaysOnSampler());
//     options.AddSource(ClientSemantics.UiActivity);
//     options.AddHttpClientInstrumentation(c => c.RecordException = true);
//
// switch (tracingExporter)
// {
//     // case "jaeger":
//     //     options.AddJaegerExporter();
//     //
//     //     builder.Services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));
//     //
//     //     // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
//     //     builder.Services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
//     //     break;
//     //
//     // case "zipkin":
//     //     options.AddZipkinExporter();
//     //
//     //     builder.Services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));
//     //     break;
//

//
//
//    OpenTelemetry exporter ( OpenTelemetry.Exporter.OpenTelemetryProtocol ) is integrating with the .NET Core EventListener thus
//    scheduling a background thread that eventually enters code that is most likely annotated with [UnsupportedOSPlatform("browser")]
//    or some other code checks.
//
//    I tried to fix up BaseOtlpHttpExportClient.cs and change the call on line 79 from HttpClient.Send to HttpClient.SendAsync as
//    it is easy to trace the code to an [UnsupportedOSPlatform("browser")].  I used
//    RuntimeInformation.RuntimeIdentifier == "browser-wasm" condition.  But the reality is the EventListener is triggering
//    this call.
//
//    Note that I had to compile with DebugType = portable.  Without it I would not be able
//    to step into any WebAssembly code nor the Otel libraries.  This one got me for a few days.  I chased my tail thinking I had a
//    Visual Studio 2022 issue.
//
//    If I want to use the Otel Exporter I will need to run run it from each command in Simple ExportProcessorType mode.
//    At lest that is my next plan.
//
//    Issues opened against dotnet/runtime related to this issue.
//    https://github.com/dotnet/runtime/issues/61308
//    https://github.com/dotnet/runtime/issues/61308
//
//    Issue that has a Milestone of .NET 7 Planning to add "Real multithreading (on supported browsers).
//    So maybe this technique can be revisited later this year when we see 7.0 bits with this feature.
//    https://github.com/dotnet/aspnetcore/issues/17730
//
//

// case "otlp":
//         options.AddOtlpExporter(otlpOptions =>
//         {
//             otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
//             otlpOptions.ExportProcessorType = ExportProcessorType.Simple;
//             otlpOptions.Endpoint = new Uri($"{builder.Configuration.GetValue<string>("Otlp:Endpoint")}/v1/traces");
//         });
//         break;
//
//     default:
//         options.AddConsoleExporter();
//
//         break;
// }
// });

// Logging
// builder.Logging.ClearProviders();

//
// Same problem as with Tracing above. Might need to look at Javascript telemetry and see how they do this.
// Blazor 7 should handle this.
//

// builder.Logging.AddOpenTelemetry(options =>
// {
//     options.SetResourceBuilder(resourceBuilder);
//     var logExporter = builder.Configuration.GetValue<string>("UseLogExporter").ToLowerInvariant();
//     switch (logExporter)
//     {
//         case "otlp":
//             options.AddOtlpExporter(otlpOptions =>
//             {
//                 otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
//                 otlpOptions.ExportProcessorType = ExportProcessorType.Simple;
//                 otlpOptions.Endpoint = new Uri($"{builder.Configuration.GetValue<string>("Otlp:Endpoint")}/v1/logs");
//             });
//             break;
//         default:
//             options.AddConsoleExporter();
//             break;
//     }
// });
//
// builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
// {
//     opt.IncludeScopes = true;
//     opt.ParseStateValues = true;
//     opt.IncludeFormattedMessage = true;
// });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
