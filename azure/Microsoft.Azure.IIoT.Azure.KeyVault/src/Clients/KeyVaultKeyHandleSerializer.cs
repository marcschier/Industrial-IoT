﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.KeyVault.Clients {
    using Microsoft.Azure.IIoT.Azure.KeyVault.Models;
    using Microsoft.Azure.IIoT.Crypto;
    using Microsoft.Azure.IIoT.Crypto.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Keyvault key handle serializer
    /// </summary>
    public class KeyVaultKeyHandleSerializer : IKeyHandleSerializer {

        /// <summary>
        /// Create serializer
        /// </summary>
        /// <param name="serializer"></param>
        public KeyVaultKeyHandleSerializer(IJsonSerializer serializer) {
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<byte> SerializeHandle(KeyHandle handle) {
            if (handle is KeyVaultKeyHandle id) {
                return _serializer.SerializeToBytes(id).ToArray();
            }
            throw new ArgumentException("Bad handle type");
        }

        /// <inheritdoc/>
        public KeyHandle DeserializeHandle(IReadOnlyCollection<byte> token) {
            if (token == null) {
                return null;
            }
            return _serializer.Deserialize<KeyVaultKeyHandle>(token.ToArray());
        }

        private readonly IJsonSerializer _serializer;
    }
}