// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using System;

    /// <summary>
    /// Schema type extensions
    /// </summary>
    public static class MessageSchemaEx {

        /// <summary>
        /// Construct message schema from messaging mode and encoding
        /// </summary>
        /// <returns></returns>
        public static string ToMessageSchemaMimeType(this NetworkMessageEncoding? encoding) {
            switch (encoding) {
                case NetworkMessageEncoding.Uadp:
                    return MessageSchemaTypes.NetworkMessageUadp;
                case NetworkMessageEncoding.Json:
                default: // Default encoding is json
                    return MessageSchemaTypes.NetworkMessageJson;
            }
        }

        /// <summary>
        /// Match encoding
        /// </summary>
        /// <param name="mimeType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool Matches(this NetworkMessageEncoding encoding, string mimeType) {
            switch (mimeType) {
                case MessageSchemaTypes.NetworkMessageUadp:
                    return encoding == NetworkMessageEncoding.Uadp;
                case MessageSchemaTypes.NetworkMessageJson:
                    return encoding == NetworkMessageEncoding.Json;
                default:
                    throw new ArgumentException($"Unknown type {mimeType}",
                        nameof(mimeType));
            }
        }

        /// <summary>
        /// Match content mask
        /// </summary>
        /// <param name="mimeType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool Matches(this NetworkMessageContentMask content, string mimeType) {
            _ = !content.HasFlag(NetworkMessageContentMask.NetworkMessageHeader);
            _ = !content.HasFlag(NetworkMessageContentMask.DataSetMessageHeader);
            switch (mimeType) {
                case MessageSchemaTypes.NetworkMessageUadp:
                case MessageSchemaTypes.DataSetWriterMessage:
                case MessageSchemaTypes.NetworkMessageJson:
                    return true; // TODO -
                default:
                    throw new ArgumentException($"Unknown type {mimeType}",
                        nameof(mimeType));
            }
        }
    }
}