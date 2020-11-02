// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Rpc {
    using Microsoft.Azure.IIoT.Rpc.Services;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Text;

    public class TestChunkServer : IJsonMethodClient, IMethodHandler {

        public TestChunkServer(IJsonSerializer serializer,
            int size, Func<string, string, byte[], string, byte[]> handler) {
            MaxMethodPayloadSizeInBytes = size;
            _handler = handler;
            _serializer = serializer;
            _server = new ChunkMethodServer(_serializer, Log.Console());
        }

        public IMethodClient CreateClient() {
            return new ChunkMethodClient(this, _serializer, Log.Console());
        }

        public int MaxMethodPayloadSizeInBytes { get; }

        public async Task<string> CallMethodAsync(string target, string method,
            string json, TimeSpan? timeout, CancellationToken ct) {
            var payload = Encoding.UTF8.GetBytes(json);
            var processed = await _server.InvokeAsync(target, payload,
                ContentMimeType.Json, this).ConfigureAwait(false);
            return Encoding.UTF8.GetString(processed);
        }

        public Task<byte[]> InvokeAsync(string target,
            string method, byte[] payload, string contentType) {
            return Task.FromResult(_handler.Invoke(target, method, payload, contentType));
        }

        private readonly IMethodInvoker _server;
        private readonly IJsonSerializer _serializer;
        private readonly Func<string, string, byte[], string, byte[]> _handler;
    }
}
