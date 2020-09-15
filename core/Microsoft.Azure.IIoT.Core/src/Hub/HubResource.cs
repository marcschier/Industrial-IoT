// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub {
    using System;
    using System.Text;

    /// <summary>
    /// Hub resource utilities
    /// </summary>
    public static class HubResource {

        /// <summary>
        /// Parse hub resource
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="target"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public static string Parse(string target, out string hub, out string moduleId) {
            if (string.IsNullOrEmpty(target)) {
                throw new ArgumentNullException(nameof(target));
            }
            // Split path
            var elements = target.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var found = 0;
            hub = null;
            for (; found < elements.Length; found++) {
                if (elements[found].Equals("devices",
                    StringComparison.InvariantCultureIgnoreCase)) {
                    break;
                }
                hub = elements[found]; // hub comes pre-"devices"
            }
            if (++found >= elements.Length) {
                throw new FormatException("No deviceid found.");
            }
            var deviceId = elements[found];
            if (++found == elements.Length) {
                // Good but no module id
                moduleId = null;
                return deviceId;
            }
            if (!elements[found].Equals("modules",
                StringComparison.InvariantCultureIgnoreCase) ||
                ++found >= elements.Length) {
                throw new FormatException("No moduleId found or more items than expected.");
            }

            moduleId = elements[found];
            if (++found != elements.Length) {
                throw new FormatException("More items after moduleid than expected.");
            }
            return deviceId;
        }

        /// <summary>
        /// Format hub resource
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public static string Format(string hub, string deviceId, string moduleId) {
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentNullException(nameof(deviceId));
            }
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(hub)) {
                sb.Append(hub);
                sb.Append('/');
            }
            sb.Append("devices/");
            sb.Append(deviceId);
            if (!string.IsNullOrEmpty(moduleId)) {
                sb.Append("/modules/");
                sb.Append(moduleId);
            }
            return sb.ToString();
        }
    }
}
