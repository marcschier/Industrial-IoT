// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.SignalR.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// SignalR configuration
    /// </summary>
    internal sealed class SignalRServiceConfig : PostConfigureOptionBase<SignalRServiceOptions> {

        /// <inheritdoc/>
        public override void PostConfigure(string name, SignalRServiceOptions options) {
            if (string.IsNullOrEmpty(options.ConnectionString)) {
                options.ConnectionString = GetStringOrDefault(PcsVariable.PCS_SIGNALR_CONNSTRING);
            }
            var serverless = GetStringOrDefault(PcsVariable.PCS_SIGNALR_MODE)
                .EqualsIgnoreCase(kSignalRServerLessMode);
            if (serverless) {
                options.IsServerLess = serverless;
            }
        }

        /// <inheritdoc/>
        public SignalRServiceConfig(IConfiguration configuration) :
            base(configuration) {
        }

        private const string kSignalRServerLessMode = "Serverless";
    }
}
