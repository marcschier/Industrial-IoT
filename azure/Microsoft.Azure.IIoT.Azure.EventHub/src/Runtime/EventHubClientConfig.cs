// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Event hub configuration - wraps a configuration root
    /// </summary>
    internal sealed class EventHubClientConfig : ConfigBase<EventHubClientOptions> {

        /// <inheritdoc/>
        public EventHubClientConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, EventHubClientOptions options) {
            options.EventHubConnString = GetStringOrDefault(PcsVariable.PCS_EVENTHUB_CONNSTRING, () => null);
            options.EventHubPath = GetStringOrDefault(PcsVariable.PCS_EVENTHUB_NAME, () => null);
        }
    }
}
