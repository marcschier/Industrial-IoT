// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Serializers.MessagePack {
    using System.Collections.Generic;
    using global::MessagePack;

    /// <summary>
    /// Default formatters
    /// </summary>
    public class MessagePackResolvers : IMessagePackFormatterResolverProvider {

        /// <inheritdoc/>
        public IEnumerable<IFormatterResolver> GetResolvers() {
            var resolvers = new List<IFormatterResolver> {
                // TODO
            };
            return resolvers;
        }
    }
}