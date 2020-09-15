// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Handlers {
    using System;

    /// <summary>
    /// Hub resource utilities
    /// </summary>
    public static class HubResource {

        /// <summary>
        /// Parse hub resource
        /// </summary>
        /// <param name="target"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public static string Parse(string target, out string moduleId) {
            var elements = target.Split('/', StringSplitOptions.None);
            if (elements.Length > 1 && elements[0].Equals("devices",
                StringComparison.InvariantCultureIgnoreCase)) {
                if (elements.Length > 3 && elements[2].Equals("modules",
                    StringComparison.InvariantCultureIgnoreCase)) {
                    moduleId = elements[3];
                }
                else {
                    moduleId = null;
                }
                return elements[1];
            }
            moduleId = null;
            return null;
        }

        /// <summary>
        /// Format hub resource
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public static string Format(string deviceId, string moduleId = null) {
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentNullException(nameof(deviceId));
            }
            if (string.IsNullOrEmpty(moduleId)) {
                return $"devices/{deviceId}";
            }
            return $"devices/{deviceId}/modules/{moduleId}";
        }
    }
}
