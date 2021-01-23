// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Historic event
    /// </summary>
    public class HistoricEventModel {

        /// <summary>
        /// The selected fields of the event
        /// </summary>
        public IReadOnlyList<VariantValue> EventFields { get; set; } // TODO: Update to concrete type
    }
}
