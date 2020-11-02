// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.SignalR.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// SignalR configuration
    /// </summary>
    internal sealed class SignalRServiceConfig : ConfigBase<SignalRServiceOptions> {

        /// <inheritdoc/>
        public override void Configure(string name, SignalRServiceOptions options) {
            options.SignalRConnString = GetStringOrDefault(PcsVariable.PCS_SIGNALR_CONNSTRING, 
                () => null);
            var signalRServiceMode = GetStringOrDefault(PcsVariable.PCS_SIGNALR_MODE, 
                () => kSignalRServerLessMode);
            options.SignalRServerLess = signalRServiceMode.EqualsIgnoreCase(kSignalRServerLessMode);
        }

        /// <inheritdoc/>
        public SignalRServiceConfig(IConfiguration configuration) :
            base(configuration) {
        }

        private const string kSignalRServerLessMode = "Serverless";
    }
}
