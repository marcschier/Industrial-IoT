// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Replace historic data
    /// </summary>
    [DataContract]
    public class ReplaceValuesDetailsApiModel {

        /// <summary>
        /// Values to replace
        /// </summary>
        [DataMember(Name = "values", Order = 0)]
        [Required]
        public List<HistoricValueApiModel> Values { get; set; }
    }
}
