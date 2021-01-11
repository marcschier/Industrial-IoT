// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Key parameters
    /// </summary>
    public abstract class KeyParams {

        /// <summary>
        /// Represents a token.
        /// </summary>
        public IReadOnlyCollection<byte> T { get; set; }
    }
}