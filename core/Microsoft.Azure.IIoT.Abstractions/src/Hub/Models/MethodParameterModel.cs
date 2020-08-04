// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub.Models {
    using System;

    /// <summary>
    /// Twin services method params
    /// </summary>
    public class MethodParameterModel {

        /// <summary>
        /// Name of method
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Response timeout
        /// </summary>
        public TimeSpan? ResponseTimeout { get; set; }

        /// <summary>
        /// Connection timeout
        /// </summary>
        public TimeSpan? ConnectionTimeout { get; set; }

        /// <summary>
        /// Json payload of the method request
        /// </summary>
        public string JsonPayload { get; set; }
    }
}
