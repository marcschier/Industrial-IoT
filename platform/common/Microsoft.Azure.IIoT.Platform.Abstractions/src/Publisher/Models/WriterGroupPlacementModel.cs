// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {

    /// <summary>
    /// Writer group placement
    /// </summary>
    public class WriterGroupPlacementModel {

        /// <summary>
        /// Publisher id
        /// </summary>
        public string PublisherId { get; set; }

        /// <summary>
        /// Dataset writer group identifier
        /// </summary>
        public string WriterGroupId { get; set; }
    }
}
