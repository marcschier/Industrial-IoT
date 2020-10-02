// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Certificate chain
    /// </summary>
    public sealed class X509CertificateChainModel {

        /// <summary>
        /// Chain
        /// </summary>
        public IReadOnlyList<X509CertificateModel> Chain { get; set; }

        /// <summary>
        /// Chain validation status if validated
        /// </summary>
        public IReadOnlyList<X509ChainStatus> Status { get; set; }
    }

}
