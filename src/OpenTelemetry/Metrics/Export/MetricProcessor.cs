﻿// <copyright file="MetricProcessor.cs" company="OpenTelemetry Authors">
// Copyright 2018, OpenTelemetry Authors
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTelemetry.Metrics.Export
{
    public abstract class MetricProcessor<T> where T : struct
    {
        /// <summary>
        /// Process the counter metric.
        /// </summary>
        /// <param name="labelSet">the labelSet associated with counter value.</param>
        /// <param name="value">the counter value.</param>
        public abstract void AddCounter(LabelSet labelSet, T value);
    }
}