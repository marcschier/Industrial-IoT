// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Opc.Ua {
    using Newtonsoft.Json;
    using System;
    using System.Globalization;

    /// <summary>
    /// Writes and reads status codes
    /// </summary>
    public sealed class StatusCodeConverter : JsonConverter {

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) {
            return
                objectType == typeof(StatusCode) ||
                objectType == typeof(StatusCode?);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer) {
            if (reader is null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.TokenType != JsonToken.Integer) {
                if (objectType == typeof(StatusCode?)) {
                    return null;
                }
                return StatusCodes.Good;
            }
            return new StatusCode(Convert.ToUInt32(reader.Value, CultureInfo.InvariantCulture));
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer) {
            if (writer is null) {
                throw new ArgumentNullException(nameof(writer));
            }

            var statusCode = value as StatusCode?;
            if (statusCode == null) {
                writer.WriteNull();
            }
            else {
                writer.WriteToken(JsonToken.Integer, statusCode.Value.CodeBits);
            }
        }
    }
}
