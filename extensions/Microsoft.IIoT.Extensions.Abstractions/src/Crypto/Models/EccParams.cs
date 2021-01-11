// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Ecc parameters
    /// </summary>
    public class EccParams : KeyParams {

        /// <summary>
        /// Name of the curve
        /// </summary>
        public CurveType Curve { get; set; }

        /// <summary>
        /// Represents the private key D
        /// stored in big-endian format.
        /// </summary>
        public IReadOnlyCollection<byte> D { get; set; }

        /// <summary>
        /// Represents the public key X-Coord.
        /// </summary>
        public IReadOnlyCollection<byte> X { get; set; }

        /// <summary>
        /// Represents the public key Y-Coord.
        /// </summary>
        public IReadOnlyCollection<byte> Y { get; set; }
    }
}