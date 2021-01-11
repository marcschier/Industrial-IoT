// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Storage.Models {
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Crl document
    /// </summary>
    [DataContract]
    public class CrlDocument {

        /// <summary>
        /// Document type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = ClassTypeName;
        /// <summary/>
        public static readonly string ClassTypeName = "Crl";

        /// <summary>
        /// Serial number of the certificate
        /// </summary>
        [DataMember(Name = "id")]
        public string CertificateSerialNumber { get; set; }

        /// <summary>
        /// Issuer serial number
        /// </summary>
        [DataMember]
        public string IssuerSerialNumber { get; set; }

        /// <summary>
        /// Crl serial number
        /// </summary>
        [DataMember]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Next update
        /// </summary>
        [DataMember]
        public DateTime? NextUpdate { get; set; }

        /// <summary>
        /// This update
        /// </summary>
        [DataMember]
        public DateTime ThisUpdate { get; set; }

        /// <summary>
        /// Raw crl for the certificate
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<byte> RawData { get; set; }

        /// <summary>
        /// Expiration in seconds
        /// </summary>
        [DataMember(Name = "ttl")]
        public int Ttl { get; set; }
    }
}

