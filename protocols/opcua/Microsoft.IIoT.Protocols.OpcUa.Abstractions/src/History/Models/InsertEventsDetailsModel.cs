// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Insert historic events
    /// </summary>
    public class InsertEventsDetailsModel {

        /// <summary>
        /// The filter to use to select the events
        /// </summary>
        public EventFilterModel Filter { get; set; }

        /// <summary>
        /// The new events to insert
        /// </summary>
        public IReadOnlyList<HistoricEventModel> Events { get; set; }
    }
}
