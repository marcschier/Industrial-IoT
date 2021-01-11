﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Handlers {
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    /// <summary>
    /// Twin registry change events
    /// </summary>
    public sealed class TwinChangeEventHandler : DeviceTwinChangeHandlerBase {

        /// <inheritdoc/>
        public override string MessageSchema => NotificationType.TwinChangeNotification;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public TwinChangeEventHandler(IJsonSerializer serializer,
            IEnumerable<IDeviceTwinEventHandler> handlers, ILogger logger)
            : base(serializer, handlers, logger) {
        }

        /// <summary>
        /// Get operation
        /// </summary>
        /// <param name="opType"></param>
        /// <returns></returns>
        protected override DeviceTwinEventType? GetOperation(string opType) {
            switch (opType) {
                case "replaceTwin":
                    return DeviceTwinEventType.Create;
                case "updateTwin":
                    return DeviceTwinEventType.Update;
            }
            return null;
        }
    }
}
