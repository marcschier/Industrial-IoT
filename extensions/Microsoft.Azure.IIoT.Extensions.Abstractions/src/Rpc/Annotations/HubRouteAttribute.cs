// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Rpc {
    using System;

    /// <summary>
    /// Metadata for hub
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HubRouteAttribute : Attribute {

        /// <summary>
        /// Create attribute
        /// </summary>
        /// <param name="mapTo"></param>
        public HubRouteAttribute(string mapTo) {
            MapTo = mapTo;
        }

        /// <summary>
        /// Mapping
        /// </summary>
        public string MapTo { get; set; }
    }
}
