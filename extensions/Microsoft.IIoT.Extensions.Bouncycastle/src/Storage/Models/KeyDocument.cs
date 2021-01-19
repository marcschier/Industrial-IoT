﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Storage.Models {
    using System.Runtime.Serialization;
    using Microsoft.IIoT.Extensions.Serializers;

    /// <summary>
    /// Key document
    /// </summary>
    [DataContract]
    public class KeyDocument {

        /// <summary>
        /// Document type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = ClassTypeName;
        /// <summary/>
        public static readonly string ClassTypeName = "Key";

        /// <summary>
        /// Key id
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Whether the key is disabled
        /// </summary>
        [DataMember]
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Whether the key is exportable
        /// </summary>
        [DataMember]
        public bool IsExportable { get; set; }

        /// <summary>
        /// Key itself in json format
        /// </summary>
        [DataMember]
        public VariantValue KeyJson { get; set; }
    }
}
