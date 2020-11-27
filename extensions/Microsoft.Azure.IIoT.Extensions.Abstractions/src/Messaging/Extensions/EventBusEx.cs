// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging {
    using System;

    /// <summary>
    /// Event bus extensions
    /// </summary>
    public static class EventBusEx {

        /// <summary>
        /// Convert type name to event name - the namespace of the type should
        /// include versioning information.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetMoniker(this Type type) {
            if (type is null) {
                throw new ArgumentNullException(nameof(type));
            }
            var name = type.FullName
                .Replace("Microsoft.Azure.IIoT.", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace(".", "-", StringComparison.InvariantCultureIgnoreCase)
                .Replace("Model", "", StringComparison.InvariantCultureIgnoreCase)
                .ToUpperInvariant();
            if (name.Length >= 50) {
                name = name.Substring(0, 50);
            }
            return name;
        }
    }
}
