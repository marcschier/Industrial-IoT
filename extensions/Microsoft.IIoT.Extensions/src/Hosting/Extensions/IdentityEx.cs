// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Hosting {

    /// <summary>
    /// Identity extensions
    /// </summary>
    public static class IdentityEx {

        /// <summary>
        /// Convert to resource string
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string AsHubResource(this IIdentity identity) {
            return HubResource.Format(identity.Hub, identity.DeviceId, identity.ModuleId);
        }
    }
}
