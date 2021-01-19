// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Rpc.Services {
    using Microsoft.IIoT.Extensions.Rpc.Models;
    using Microsoft.IIoT.Extensions.Rpc;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.IO;
    using System.Threading;
    using System.Text;

    /// <summary>
    /// Chunked method provide reliable any size send/receive
    /// </summary>
    public sealed class ChunkMethodClient : IMethodClient {

        /// <summary>
        /// Create client wrapping a json method client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public ChunkMethodClient(IJsonMethodClient client, IJsonSerializer serializer,
            ILogger logger) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //
            // assume base64 encoding is 33% reduction compared to raw bytes
            // plus the additional overhead of the model payload.
            //
            _maxSize = (int)(_client.MaxMethodPayloadSizeInBytes * 0.66);
            if (_maxSize == 0) {
                _maxSize = 1;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> CallMethodAsync(string target,
            string method, byte[] payload, string contentType, TimeSpan? timeout,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(method)) {
                throw new ArgumentNullException(nameof(method));
            }
            if (payload == null) {
                payload = new byte[] { (byte)' ' };
            }
            if (contentType == null) {
                contentType = ContentMimeType.Json;
            }
            // Send chunks
            var buffer = payload.Zip(); // Gzip payload
            var status = 200;
            using (var received = new MemoryStream()) {
                string handle = null;
                for (var offset = 0; offset < buffer.Length; offset += _maxSize) {
                    var length = Math.Min(buffer.Length - offset, _maxSize);
                    var chunk = buffer.AsSpan(offset, length).ToArray();
                    var result = await _client.CallMethodAsync(target,
                        MethodNames.Call, _serializer.SerializeToString(offset == 0 ?
                            new MethodChunkModel {
                                Timeout = timeout,
                                MethodName = method,
                                Target = target,
                                ContentType = contentType,
                                ContentLength = buffer.Length,
                                MaxChunkLength = _maxSize,
                                Payload = chunk
                            } : new MethodChunkModel {
                                Handle = handle,
                                Payload = chunk
                            }),
                        timeout, ct).ConfigureAwait(false);
                    var response = _serializer.Deserialize<MethodChunkModel>(result);
                    if (response.Payload != null) {
                        received.Write(response.Payload);
                    }
                    if (response.Status != null) {
                        status = response.Status.Value;
                    }
                    handle = response.Handle;
                }
                // Receive all responses
                while (!string.IsNullOrEmpty(handle)) {
                    var result = await _client.CallMethodAsync(target,
                        MethodNames.Call, _serializer.SerializeToString(new MethodChunkModel {
                            Handle = handle,
                        }), timeout, ct).ConfigureAwait(false);
                    var response = _serializer.Deserialize<MethodChunkModel>(result);
                    if (response.Payload != null) {
                        received.Write(response.Payload);
                    }
                    if (response.Status != null) {
                        status = response.Status.Value;
                    }
                    handle = response.Handle;
                }
                payload = received.ToArray().Unzip();
                if (status != 200) {
                    var result = AsString(payload);
                    _logger.LogDebug("Chunked call on {method} on {target} with {payload} " +
                         "returned with error {status}: {result}",
                         method, target, payload, status, result);
                    throw new MethodCallStatusException(result, status);
                }
                return payload;
            }
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static string AsString(byte[] buffer) {
            try {
                if (buffer == null) {
                    return string.Empty;
                }
                return Encoding.UTF8.GetString(buffer);
            }
            catch {
                return Convert.ToBase64String(buffer);
            }
        }

        private readonly IJsonMethodClient _client;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly int _maxSize;
    }
}