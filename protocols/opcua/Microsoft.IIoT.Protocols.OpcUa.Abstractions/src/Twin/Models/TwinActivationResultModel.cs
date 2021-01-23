// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {

    /// <summary>
    /// Result of an twin activation
    /// </summary>
    public class TwinActivationResultModel {

        /// <summary>
        /// New id twin was activated as
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
