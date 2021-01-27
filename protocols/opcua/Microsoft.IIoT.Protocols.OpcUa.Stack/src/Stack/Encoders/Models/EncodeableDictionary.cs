﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Opc.Ua.Encoders {
    using Opc.Ua;

    /// <summary>
    /// Encodeable dictionary carrying field names and values
    /// </summary>
    public class EncodeableDictionary : IEncodeable {

        /// <summary>
        /// Event fields
        /// </summary>
        public KeyValuePairCollection Fields { get; private set; }

        /// <inheritdoc/>
        public ExpandedNodeId TypeId =>
            nameof(EncodeableDictionary);

        /// <inheritdoc/>
        public ExpandedNodeId BinaryEncodingId =>
            nameof(EncodeableDictionary) + "_Encoding_DefaultBinary";

        /// <inheritdoc/>
        public ExpandedNodeId XmlEncodingId =>
            nameof(EncodeableDictionary) + "_Encoding_DefaultXml";

        /// <inheritdoc/>
        public EncodeableDictionary(KeyValuePairCollection fields) {
            Fields = fields ?? new KeyValuePairCollection();
        }

        /// <inheritdoc/>
        public EncodeableDictionary()
            : this(null) {
        }

        /// <inheritdoc/>
        public virtual void Encode(IEncoder encoder) {
            if (encoder is null) {
                throw new System.ArgumentNullException(nameof(encoder));
            }
            //  todo: check if "EventFields" is appropriate
            encoder.WriteEncodeableArray("EventFields", Fields.ToArray(), typeof(KeyValuePair));
        }

        /// <inheritdoc/>
        public virtual void Decode(IDecoder decoder) {
            if (decoder is null) {
                throw new System.ArgumentNullException(nameof(decoder));
            }
            Fields = (KeyValuePairCollection)decoder.ReadEncodeableArray(
                "EventFields", typeof(KeyValuePair));
        }

        /// <inheritdoc/>
        public virtual bool IsEqual(IEncodeable encodeable) {
            if (this == encodeable) {
                return true;
            }
            if (encodeable is not EncodeableDictionary eventFieldList) {
                return false;
            }
            if (!Utils.IsEqual(Fields, eventFieldList.Fields)) {
                return false;
            }
            return true;
        }
    }
}