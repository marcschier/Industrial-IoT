// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.LogAnalytics {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publish metrics to log analytics workspace from within the process.
    /// </summary>
    public class LogAnalyticsMetricsHandler : MetricsHandlerBase {

        /// <summary>
        /// Http client property injected
        /// </summary>
        public IHttpClient HttpClient { get; set; }

        /// <inheritdoc/>
        public override bool IsEnabled =>
            !string.IsNullOrEmpty(_config?.LogWorkspaceId) &&
            !string.IsNullOrEmpty(_config?.LogWorkspaceKey);

        /// <summary>
        /// Create publisher
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="serializer"></param>
        public LogAnalyticsMetricsHandler(IJsonSerializer serializer, ILogger logger,
            ILogAnalyticsConfig config = null) : base(serializer) {
            _config = config;
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public override void OnStarting() {
            if (_config == null) {
                _logger.Information("Inject Log analytics configuration to enable publishing.");
                return;
            }
            // Create client if not configured before...
            if (HttpClient == null) {
                HttpClient = new HttpClient(new HttpClientFactory(_logger), _logger);
            }
        }

        /// <summary>
        /// Post metrics to log analytics
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected override async Task ProcessBatchAsync(List<MetricsRecord> batch,
            CancellationToken ct) {
            var workspaceId = _config.LogWorkspaceId;
            var workspaceKey = _config.LogWorkspaceKey;
            if (string.IsNullOrEmpty(workspaceId) || string.IsNullOrEmpty(workspaceKey)) {
                return;  // id or key was updated after collection
            }
            var request = HttpClient.NewRequest($"https://{workspaceId}.ods.opinsights.azure.com" +
                $"/api/logs?api-version={kApiVersion}");
            request.AddHeader("Log-Type", _config.LogType ?? "promMetrics");
            // Set authorization
            var dateString = DateTime.UtcNow.ToString("r");
            request.AddHeader("x-ms-date", dateString);
            var content = _serializer.SerializeToBytes(batch).ToArray();
            var signature = GetSignature(workspaceId, workspaceKey, "POST", content.Length,
                ContentMimeType.Json, dateString, "/api/logs");
            request.AddHeader("Authorization", signature);
            request.SetByteArrayContent(content, ContentMimeType.Json);
            var response = await HttpClient.PostAsync(request, ct);
            response.Validate();
        }

        /// <summary>
        /// Create shared access signature
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="workspaceKey"></param>
        /// <param name="method"></param>
        /// <param name="contentLength"></param>
        /// <param name="contentType"></param>
        /// <param name="date"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private static string GetSignature(string workspaceId, string workspaceKey,
            string method, int contentLength, string contentType, string date, string resource) {
            var message =
                $"{method}\n{contentLength}\n{contentType}\nx-ms-date:{date}\n{resource}";
            var bytes = Encoding.UTF8.GetBytes(message);
            using (var encryptor = new HMACSHA256(Convert.FromBase64String(workspaceKey))) {
                var hash = encryptor.ComputeHash(bytes);
                return $"SharedKey {workspaceId}:{Convert.ToBase64String(hash)}";
            }
        }

        private const string kApiVersion = "2016-04-01";
        private readonly IJsonSerializer _serializer;
        private readonly ILogAnalyticsConfig _config;
        private readonly ILogger _logger;
    }
}