// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.AspNetCore.Tests {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hosting.Services;
    using Microsoft.Azure.IIoT.Rpc;
    using System;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Called by chunk server (typically the actual rpc client)
    /// </summary>
    public class JsonMethodClientRouterAdapter : IJsonMethodClient {

        public int MaxMethodPayloadCharacterCount => 120 * 1024;

        public JsonMethodClientRouterAdapter(IMethodRouter router) {
            _router = router;
        }

        public async Task<string> CallMethodAsync(string target, string method,
            string payload, TimeSpan? timeout, CancellationToken ct) {
            var result = await _router.InvokeAsync(target, method,
                Encoding.UTF8.GetBytes(payload), ContentMimeType.Json);
            const int kMaxMessageSize = 127 * 1024;
            if (result.Length > kMaxMessageSize) {
                throw new MethodCallStatusException(
                    (int)HttpStatusCode.RequestEntityTooLarge);
            }
            return Encoding.UTF8.GetString(result);
        }

        private readonly IMethodRouter _router;
    }

}
