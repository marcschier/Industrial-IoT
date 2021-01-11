// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Storage {
    using Microsoft.IIoT.Extensions.Crypto.Storage.Models;
    using Microsoft.IIoT.Extensions.Crypto.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Key document key handle serializer
    /// </summary>
    public class KeyHandleSerializer : IKeyHandleSerializer {

        /// <summary>
        /// Create serializer
        /// </summary>
        /// <param name="serializer"></param>
        public KeyHandleSerializer(IJsonSerializer serializer) {
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<byte> SerializeHandle(KeyHandle handle) {
            if (handle is KeyId id) {
                return _serializer.SerializeToBytes(id).ToArray();
            }
            throw new ArgumentException("Bad handle type");
        }

        /// <inheritdoc/>
        public KeyHandle DeserializeHandle(IReadOnlyCollection<byte> token) {
            if (token == null) {
                return null;
            }
            return _serializer.Deserialize<KeyId>(token.ToArray());
        }

        private readonly IJsonSerializer _serializer;
    }
}

