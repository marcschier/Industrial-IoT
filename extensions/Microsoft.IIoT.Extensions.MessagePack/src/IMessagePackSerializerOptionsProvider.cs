// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Serializers {
    using System.Collections.Generic;
    using global::MessagePack;

    /// <summary>
    /// Message pack serializer options provider
    /// </summary>
    public interface IMessagePackSerializerOptionsProvider {

        /// <summary>
        /// Serializer options
        /// </summary>
        MessagePackSerializerOptions Options { get; }

        /// <summary>
        /// Resolvers
        /// </summary>
        IEnumerable<IFormatterResolver> Resolvers { get; }
    }
}
