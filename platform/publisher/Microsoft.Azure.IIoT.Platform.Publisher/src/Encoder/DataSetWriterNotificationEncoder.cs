// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using System.Collections.Generic;
    using System.IO;
    using System;
    using Opc.Ua;

    /// <summary>
    /// Dataset writer notification encoder
    /// </summary>
    public class DataSetWriterNotificationEncoder : IDataSetWriterNotificationEncoder {

        /// <inheritdoc/>
        public void Encode(string dataSetWriterId, uint sequenceNumber,
            NotificationData notification, IList<string> stringTable,
            out byte[] header, out byte[] payload, ref ServiceMessageContext context) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (notification is null) {
                throw new ArgumentNullException(nameof(notification));
            }
            context ??= new ServiceMessageContext();
            using (var stream = new MemoryStream()) {
                using (var encoder = new BinaryEncoder(stream, context)) {
                    encoder.WriteString("dataSetWriterId", dataSetWriterId);
                    encoder.WriteUInt32("sequenceNumber", sequenceNumber);
                    encoder.WriteVariant("notification", new Variant(notification));
                    encoder.WriteStringArray("stringTable", stringTable ?? new List<string>());
                }
                payload = stream.ToArray();
            }
            using (var stream = new MemoryStream()) {
                using (var encoder = new BinaryEncoder(stream, ServiceMessageContext.GlobalContext)) {
                    encoder.WriteStringArray("serverUris", context.ServerUris.ToArray());
                    encoder.WriteStringArray("namespaceUris", context.NamespaceUris.ToArray());
                }
                header = stream.ToArray();
            }
        }


        /// <inheritdoc/>
        public void Decode(byte[] header, byte[] payload, out string dataSetWriterId,
            out uint sequenceNumber, out NotificationData notification,
            out IList<string> stringTable, ref ServiceMessageContext context) {
            if (payload is null) {
                throw new ArgumentNullException(nameof(payload));
            }
            context ??= new ServiceMessageContext();
            if (header != null && header.Length != 0) {
                using (var stream = new MemoryStream(header)) {
                    context = new ServiceMessageContext();
                    using (var decoder = new BinaryDecoder(stream, ServiceMessageContext.GlobalContext)) {
                        context.ServerUris = new StringTable(decoder.ReadStringArray("serverUris"));
                        context.NamespaceUris = new NamespaceTable(decoder.ReadStringArray("namespaceUris"));
                    }
                }
            }
            using (var stream = new MemoryStream(payload)) {
                using (var decoder = new BinaryDecoder(stream, context)) {
                    dataSetWriterId = decoder.ReadString("dataSetWriterId");
                    sequenceNumber = decoder.ReadUInt32("sequenceNumber");
                    notification = decoder.ReadVariant("notification").Value
                        as NotificationData;
                    stringTable = decoder.ReadStringArray("stringTable");
                }
            }
        }
    }
}