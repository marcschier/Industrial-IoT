// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Models {

    /// <summary>
    /// Result of an application registration
    /// </summary>
    public class ApplicationRegistrationResultModel {

        /// <summary>
        /// New id application was registered under
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
