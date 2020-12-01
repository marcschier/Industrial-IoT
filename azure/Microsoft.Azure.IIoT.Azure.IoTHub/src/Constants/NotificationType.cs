// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub {

    /// <summary>
    /// Notification schema type constants
    /// </summary>
    public static class NotificationType {

        /// <summary>
        /// Content is a twin change event
        /// </summary>
        public const string TwinChangeNotification =
            "twinChangeNotification";

        /// <summary>
        /// Content is a lifecycle event
        /// </summary>
        public const string DeviceLifecycleNotification =
            "deviceLifecycleNotification";
    }
}
