// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Crypto.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Rsa parameters
    /// </summary>
    public class RsaParams : KeyParams {

        /// <summary>
        /// Represents the D parameter.
        /// </summary>
        public IReadOnlyCollection<byte> D { get; set; }

        /// <summary>
        /// Represents the DP parameter.
        /// </summary>
        public IReadOnlyCollection<byte> DP { get; set; }

        /// <summary>
        /// Represents the DQ parameter.
        /// </summary>
        public IReadOnlyCollection<byte> DQ { get; set; }

        /// <summary>
        /// Represents the Exponent parameter.
        /// </summary>
        public IReadOnlyCollection<byte> E { get; set; }

        /// <summary>
        /// Represents the InverseQ parameter (QI).
        /// </summary>
        public IReadOnlyCollection<byte> QI { get; set; }

        /// <summary>
        /// Represents the Modulus parameter (N).
        /// </summary>
        public IReadOnlyCollection<byte> N { get; set; }

        /// <summary>
        /// Represents the RSA secret prime (P).
        /// </summary>
        public IReadOnlyCollection<byte> P { get; set; }

        /// <summary>
        /// Represents the RSA secret prime, with p &lt; q.
        /// </summary>
        public IReadOnlyCollection<byte> Q { get; set; }
    }
}