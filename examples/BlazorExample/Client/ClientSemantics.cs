// <copyright file="ClientSemantics.cs" company="OpenTelemetry Authors">
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

namespace BlazorExample.Client
{
    /// <summary>
    /// General naming and organizing of telemetry semantics for the Blazor Client
    /// </summary>
    internal static class ClientSemantics
    {
        public const string ServiceName = "BlazorExample_Client";

        /// <summary>
        /// The Activity display names
        /// </summary>
        public static class DisplayName
        {
            public const string CounterButtonClick = "Counter_Click";
            public const string WeatherButtonClick = "Weather_Click";
        }

        /// <summary>
        /// Activity Source names.
        /// Remember an <see cref="ActivitySource"/> name will need a listener to be recorded
        /// </summary>
        public static class SubscribedSourceName
        {
            public const string UIWeatherRequest = "WeatherRequest";
            public const string UICounter = "Counter";
        }
    }
}
