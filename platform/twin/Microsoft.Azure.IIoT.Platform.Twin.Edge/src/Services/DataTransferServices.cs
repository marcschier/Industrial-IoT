// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Services {
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Azure.IIoT.Auth;
    using Microsoft.Azure.IIoT.Http;
    using Serilog;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net.Http.Headers;

    /// <summary>
    /// Transfer large data to and from cloud
    /// </summary>
    public sealed class DataTransferServices : ITransferServices<EndpointModel>, IDisposable {

        /// <summary>
        /// Create upload services
        /// </summary>
        /// <param name="client"></param>
        /// <param name="http"></param>
        /// <param name="tokens"></param>
        /// <param name="scheduler"></param>
        /// <param name="logger"></param>
        public DataTransferServices(IEndpointServices client, IHttpClient http,
            ISasTokenGenerator tokens, ITaskScheduler scheduler, ILogger logger) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _tasks = new ConcurrentDictionary<ConnectionIdentifier, ModelUploadTask>();
        }

        /// <inheritdoc/>
        public Task<ModelUploadStartResultModel> ModelUploadStartAsync(EndpointModel endpoint,
            ModelUploadStartRequestModel request) {
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            // Get or add new task
            var task = _tasks.GetOrAdd(new ConnectionIdentifier(new ConnectionModel {
                Endpoint = endpoint
            }), id => new ModelUploadTask(this, id, request, _logger));

            // Return info about task
            return Task.FromResult(new ModelUploadStartResultModel {
                FileName = task.FileName,
                ContentMimeType = task.MimeType,
                TimeStamp = task.StartTime
            });
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() {
            if (_tasks.Count != 0) {
                Task.WaitAll(_tasks.Values.Select(t => t.CancelAsync()).ToArray());
                _tasks.Clear();
            }
        }

        /// <summary>
        /// A scheduled model upload task
        /// </summary>
        private class ModelUploadTask {

            /// <summary>
            /// File name to upload to
            /// </summary>
            public string FileName { get; }

            /// <summary>
            /// Url to upload to
            /// </summary>
            public string Url { get; }

            /// <summary>
            /// Basic Authorization token to use
            /// </summary>
            public string Authorization { get; }

            /// <summary>
            /// Content encoding
            /// </summary>
            public string MimeType { get; }

            /// <summary>
            /// Start time
            /// </summary>
            public DateTime StartTime { get; }

            /// <summary>
            /// Create model upload task
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="id"></param>
            /// <param name="request"></param>
            /// <param name="logger"></param>
            public ModelUploadTask(DataTransferServices outer, ConnectionIdentifier id,
                ModelUploadStartRequestModel request, ILogger logger) {
                _outer = outer;
                _cts = new CancellationTokenSource();
                MimeType = ValidateEncoding(request.ContentMimeType, out var extension);
                Authorization = request.AuthorizationHeader;
                Url = request.UploadEndpointUrl;
                StartTime = DateTime.UtcNow;
                FileName = $"{id.GetHashCode()}_{StartTime.ToBinary()}{extension}";
                _logger = logger.ForContext("SourceContext", new {
                    endpoint = id.Connection.Endpoint.Url,
                    url = Url,
                    fileName = FileName,
                    mimeType = MimeType
                });
                _job = _outer._scheduler.Run(() =>
                    UploadModelAsync(id, request.Diagnostics, _cts.Token));
            }

            /// <summary>
            /// Run export cycles
            /// </summary>
            /// <param name="id"></param>
            /// <param name="diagnostics"></param>
            /// <param name="ct"></param>
            /// <returns></returns>
            private async Task UploadModelAsync(ConnectionIdentifier id, DiagnosticsModel diagnostics,
                CancellationToken ct) {
                var fullPath = Path.Combine(Path.GetTempPath(), FileName);
                try {
                    _logger.Information("Start model upload.");
                    using (var file = new FileStream(fullPath, FileMode.Create)) {
                        using (var stream = new GZipStream(file, CompressionMode.Compress)) {
                            // TODO: Try read nodeset from namespace metadata!
                            // ...


                            // Otherwise browse model
                            await BrowseEncodeModelAsync(id.Connection.Endpoint, diagnostics, stream, ct);
                        }

                        // Rewind
                        file.Seek(0, SeekOrigin.Begin);

                        // now upload file
                        var request = _outer._http.NewRequest($"{Url}/{FileName}");
                        if (!string.IsNullOrEmpty(Authorization)) {
                            request.Headers.Authorization = AuthenticationHeaderValue.Parse(Authorization);
                        }
                        else {
                            // Otherwise set shared access token
                            var token = await _outer._tokens.GenerateTokenAsync(request.Uri.ToString());
                            request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
                        }
                        request.SetStreamContent(file, MimeType);
                        await _outer._http.PutAsync(request, ct);
                        _logger.Information("Model uploaded");
                    }
                }
                catch (OperationCanceledException) {
                    _logger.Information("Cancelled model upload of {fileName} for {url}",
                        FileName, id.Connection.Endpoint.Url);
                }
                catch (Exception ex) {
                    _logger.Error(ex, "Error during export to {fileName} for {url}.",
                        FileName, id.Connection.Endpoint.Url);
                }
                finally {
                    File.Delete(fullPath);
                    _outer._tasks.TryRemove(id, out _);
                }
            }

            /// <summary>
            /// Export using browse encoder
            /// </summary>
            /// <param name="endpoint"></param>
            /// <param name="diagnostics"></param>
            /// <param name="stream"></param>
            /// <param name="ct"></param>
            /// <returns></returns>
            private async Task BrowseEncodeModelAsync(EndpointModel endpoint,
                DiagnosticsModel diagnostics, Stream stream, CancellationToken ct) {
                using (var encoder = new BrowsedNodeStreamEncoder(_outer._client, endpoint,
                    stream, MimeType, diagnostics, _logger, null)) {
                    await encoder.EncodeAsync(ct);
                }
            }

            /// <summary>
            /// Get file extension for content type
            /// </summary>
            /// <param name="contentType"></param>
            /// <param name="extension"></param>
            /// <returns></returns>
            private static string ValidateEncoding(string contentType, out string extension) {
                if (contentType == null) {
                    contentType = ContentMimeType.UaJson;
                }
                switch (contentType.ToLowerInvariant()) {
#if NO_SUPPORT
                    case ContentMimeType.UaBinary:
                        extension = ".ua.bin.gzip";
                        break;
                    case ContentMimeType.UaXml:
                        extension = ".ua.xml.gzip";
                        break;
#endif
                    case ContentMimeType.UaBson:
                        extension = ".ua.bson.gzip";
                        break;
                    default:
                        extension = ".ua.json.gzip";
                        contentType = ContentMimeType.UaJson;
                        break;
                }
                return contentType + "+gzip";
            }

            /// <summary>
            /// Cancel task
            /// </summary>
            /// <returns></returns>
            public Task CancelAsync() {
                _cts.Cancel();
                return _job;
            }

            private readonly CancellationTokenSource _cts;
            private readonly Task _job;
            private readonly DataTransferServices _outer;
            private readonly ILogger _logger;
        }

        private readonly IEndpointServices _client;
        private readonly IHttpClient _http;
        private readonly ISasTokenGenerator _tokens;
        private readonly ITaskScheduler _scheduler;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<ConnectionIdentifier, ModelUploadTask> _tasks;
    }
}
