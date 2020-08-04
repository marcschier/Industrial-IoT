// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa.Runtime {

    /// <summary>
    /// Certificate information
    /// </summary>
    public class CertificateInfo : CertificateStore {

        /// <summary>
        /// Subject name
        /// </summary>
        public string SubjectName { get; set; }
    }
}