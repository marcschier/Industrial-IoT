// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System.Threading.Tasks;
    using Opc.Ua.Extensions;
    using Opc.Ua;

    public static class ConnectionServicesEx {

        /// <summary>
        /// Read value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="connection"></param>
        /// <param name="readNode"></param>
        /// <returns></returns>
        public static Task<VariantValue> ReadValueAsync(this IConnectionServices client,
            ConnectionModel connection, string readNode) {
            return ReadValueAsync(client, connection, null, readNode);
        }

        /// <summary>
        /// Read value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="connection"></param>
        /// <param name="elevation"></param>
        /// <param name="readNode"></param>
        /// <returns></returns>
        public static Task<VariantValue> ReadValueAsync(this IConnectionServices client,
            ConnectionModel connection, CredentialModel elevation, string readNode) {
            var codec = new VariantEncoderFactory();
            return client.ExecuteServiceAsync(connection, elevation, session => {
                var nodesToRead = new ReadValueIdCollection {
                    new ReadValueId {
                        NodeId = readNode.ToNodeId(session.MessageContext),
                        AttributeId = Attributes.Value
                    }
                };
                var responseHeader = session.Read(null, 0, TimestampsToReturn.Both,
                    nodesToRead, out var values, out var diagnosticInfos);
                var result = codec.Create(session.MessageContext)
                    .Encode(values[0].WrappedValue, out var tmp);
                return Task.FromResult(result);
            });
        }
    }
}
