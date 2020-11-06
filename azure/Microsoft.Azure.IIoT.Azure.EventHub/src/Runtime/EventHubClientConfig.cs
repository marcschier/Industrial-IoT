// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Event hub configuration - wraps a configuration root
    /// </summary>
    internal sealed class EventHubClientConfig : PostConfigureOptionBase<EventHubClientOptions> {

        /// <inheritdoc/>
        public EventHubClientConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, EventHubClientOptions options) {
            if (string.IsNullOrEmpty(options.EventHubConnString)) {
                options.EventHubConnString = GetStringOrDefault(PcsVariable.PCS_EVENTHUB_CONNSTRING);
            }
            if (string.IsNullOrEmpty(options.EventHubPath)) {
                options.EventHubPath = GetStringOrDefault(PcsVariable.PCS_EVENTHUB_NAME);
            }
        }
    }
}
