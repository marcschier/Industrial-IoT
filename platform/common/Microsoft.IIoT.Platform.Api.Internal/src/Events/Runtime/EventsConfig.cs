// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Events.Api.Runtime {
    using Microsoft.IIoT.Platform.Events.Api;
    using Microsoft.IIoT.Api.Runtime;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration - wraps a configuration root
    /// </summary>
    public class EventsConfig : ApiConfigBase, IEventsConfig {

        /// <summary>
        /// Events configuration
        /// </summary>
        private const string kEventsServiceUrlKey = "EventsServiceUrl";

        /// <summary>Events configuration endpoint</summary>
        public string OpcUaEventsServiceUrl => GetStringOrDefault(
            kEventsServiceUrlKey,
            GetStringOrDefault(PcsVariable.PCS_EVENTS_SERVICE_URL,
                GetDefaultUrl("9050", "events")));

        /// <inheritdoc/>
        public EventsConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
