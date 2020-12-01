// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Http.Ssl {

    /// <summary>
    /// Configuration interface for validation
    /// </summary>
    public class ThumbprintValidatorOptions {

        /// <summary>
        /// The remote endpoint ssl certificate 
        /// thumbprint the client communicates with
        /// </summary>
        public string CertThumbprint { get; }
    }
}
