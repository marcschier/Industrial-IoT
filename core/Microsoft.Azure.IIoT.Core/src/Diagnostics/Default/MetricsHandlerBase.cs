// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Parse prometheus metrics into records and process records
    /// </summary>
    public abstract class MetricsHandlerBase : IMetricsHandler {

        /// <summary>
        /// Handler is enabled
        /// </summary>
        public abstract bool IsEnabled { get; }

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="serializer"></param>
        public MetricsHandlerBase(IJsonSerializer serializer) {
            _serializer = serializer ??
                throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public virtual void OnStarting() {
            // no op
        }

        /// <inheritdoc/>
        public async Task PushAsync(Stream stream, CancellationToken ct) {
            if (!IsEnabled) {
                return;
            }
            foreach (var batch in GetMetricsRecords(stream)) {
                await ProcessBatchAsync(batch, ct);
            }
        }

        /// <inheritdoc/>
        public virtual void OnStopped() {
            // no op
        }

        /// <summary>
        /// Post metrics to log analytics
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected abstract Task ProcessBatchAsync(List<MetricsRecord> batch,
            CancellationToken ct);

        /// <summary>
        /// Parse records from prometheus log.
        /// </summary>
        /// <param name="logstream"></param>
        /// <returns></returns>
        private IEnumerable<List<MetricsRecord>> GetMetricsRecords(Stream logstream) {
            var metricsDataList = new List<MetricsRecord>();
            using (var sr = new StreamReader(logstream, Encoding.UTF8, false, 4096, true)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    if (line.Trim().StartsWith('#')) {
                        continue;
                    }
                    var match = kPrometheusSchemaRegex.Match(line.Trim());
                    if (!match.Success) {
                        continue;
                    }
                    var metricName = string.Empty;
                    var name = match.Groups["metricname"];
                    if (name?.Length > 0) {
                        metricName = name.Value;
                    }
                    var metricValue = string.Empty;
                    var value = match.Groups["metricvalue"];
                    if (value?.Length > 0) {
                        metricValue = value.Value;
                    }
                    var tagNames = new List<string>();
                    var tagnames = match.Groups["tagname"];
                    if (tagnames.Length > 0) {
                        for (var i = 0; i < tagnames.Captures.Count; i++) {
                            tagNames.Add(tagnames.Captures[i].Value);
                        }
                    }
                    var tagValues = new List<string>();
                    var tagvalues = match.Groups["tagvalue"];
                    if (tagvalues.Length > 0) {
                        for (var i = 0; i < tagvalues.Captures.Count; i++) {
                            tagValues.Add(tagvalues.Captures[i].Value);
                        }
                    }
                    var tags = tagNames.Zip(tagValues, (k, v) => new { k, v })
                        .ToDictionary(x => x.k, x => x.v);
                    var metricsData = new MetricsRecord(
                        "prometheus",
                        metricName,
                        metricValue,
                        _serializer.SerializeToString(tags));

                    metricsDataList.Add(metricsData);
                    if (metricsDataList.Count == kMaxMetricsInBatch) {
                        yield return metricsDataList;
                        metricsDataList = new List<MetricsRecord>();
                    }
                }
            }
            yield return metricsDataList;
        }

        /// <summary>
        /// Metrics records
        /// </summary>
        [DataContract]
        protected sealed class MetricsRecord {

            /// <summary>
            /// Time generated
            /// </summary>
            [DataMember]
            public DateTime TimeGeneratedUtc { get; }

            /// <summary>
            /// Namespace
            /// </summary>
            [DataMember]
            public string Namespace { get; }

            /// <summary>
            /// Name
            /// </summary>
            [DataMember]
            public string Name { get; }

            /// <summary>
            /// Value
            /// </summary>
            [DataMember]
            public string Value { get; }

            /// <summary>
            /// Tags
            /// </summary>
            [DataMember]
            public string Tags { get; }

            /// <summary>
            /// Create record
            /// </summary>
            /// <param name="ns"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// <param name="tags"></param>
            public MetricsRecord(string ns, string name, string value, string tags) {
                TimeGeneratedUtc = DateTime.UtcNow;
                Namespace = ns;
                Name = name;
                Value = value;
                Tags = tags;
            }
        }

        private const int kMaxMetricsInBatch = 500;
        private const string kPrometheusMetricSchema =
            @"^(?<metricname>[^#\{\}]+)(\{((?<tagname>[^="",]+)=(\""(?<tagvalue>[^="",]+)\"")" +
            @"(,(?<tagname>[^="",]+)=(\""(?<tagvalue>[^="",]+)\""))*)\})?\s(?<metricvalue>.+)$";
        private static readonly Regex kPrometheusSchemaRegex =
            new Regex(kPrometheusMetricSchema, RegexOptions.Compiled);

        private readonly IJsonSerializer _serializer;
    }
}