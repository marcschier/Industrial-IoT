// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Crypto.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Aes params
    /// </summary>
    public class AesParams : KeyParams {

        /// <summary>
        /// Symmetric key
        /// </summary>
        public IReadOnlyCollection<byte> K { get; set; }
    }
}