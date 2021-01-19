// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Result of a variable removal
    /// </summary>
    [DataContract]
    public class DataSetRemoveVariableResponseApiModel {

        /// <summary>
        /// Diagnostics information in case of partial success
        /// </summary>
        [DataMember(Name = "errorInfo", Order = 0,
            EmitDefaultValue = false)]
        public ServiceResultApiModel ErrorInfo { get; set; }
    }
}