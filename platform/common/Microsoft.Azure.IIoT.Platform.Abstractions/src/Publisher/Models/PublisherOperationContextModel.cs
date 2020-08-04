// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using System;

    /// <summary>
    /// Publisher operation log model
    /// </summary>
    public class PublisherOperationContextModel {

        /// <summary>
        /// User
        /// </summary>
        public string AuthorityId { get; set; }

        /// <summary>
        /// Operation time
        /// </summary>
        public DateTime Time { get; set; }
    }
}

