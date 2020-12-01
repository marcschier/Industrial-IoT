// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Crypto {
    using Microsoft.IIoT.Crypto.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Can serialize and deserialize a key handle for
    /// storage.
    /// </summary>
    public interface IKeyHandleSerializer {

        /// <summary>
        /// Serialize a handle to buffer
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        IReadOnlyCollection<byte> SerializeHandle(KeyHandle handle);

        /// <summary>
        /// Deserialize a handle from buffer
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        KeyHandle DeserializeHandle(IReadOnlyCollection<byte> token);
    }
}