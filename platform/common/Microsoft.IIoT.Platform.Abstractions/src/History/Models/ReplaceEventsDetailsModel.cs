// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Replace historic events
    /// </summary>
    public class ReplaceEventsDetailsModel {

        /// <summary>
        /// The filter to use to select the events
        /// </summary>
        public EventFilterModel Filter { get; set; }

        /// <summary>
        /// The new events to replace
        /// </summary>
        public IReadOnlyList<HistoricEventModel> Events { get; set; }
    }
}
