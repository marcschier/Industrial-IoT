// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {

    /// <summary>
    /// Common event hub message properties
    /// </summary>
    public static class EventProperties {

        /// <summary>
        /// Content encoding of event
        /// </summary>
        public const string ContentEncoding = "content-encoding";

        /// <summary>
        /// Content type of event.
        /// </summary>
        public const string ContentType = "content-type";

        /// <summary>
        /// Event schema.
        /// </summary>
        public const string EventSchema = "EventSchema";

        /// <summary>
        /// Target resource
        /// </summary>
        public const string Target = "Target";
    }
}
