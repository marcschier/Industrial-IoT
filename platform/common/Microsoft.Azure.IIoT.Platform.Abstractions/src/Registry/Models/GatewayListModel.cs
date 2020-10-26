// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Edge Gateway list
    /// </summary>
    public class GatewayListModel {

        /// <summary>
        /// Continuation or null if final
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Edge Gateways
        /// </summary>
        public IReadOnlyList<GatewayModel> Items { get; set; }
    }
}
