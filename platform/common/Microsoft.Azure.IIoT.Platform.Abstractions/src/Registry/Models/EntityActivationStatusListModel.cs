// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Entity activation status list
    /// </summary>
    public class EntityActivationStatusListModel {

        /// <summary>
        /// Continuation
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Items in the list
        /// </summary>
        public List<EntityActivationStatusModel> Items { get; set; }
    }
}
