// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {

    /// <summary>
    /// PubSub writer group job
    /// </summary>
    public class WriterGroupJobModel {

        /// <summary>
        /// Writer group configuration
        /// </summary>
        public WriterGroupModel WriterGroup { get; set; }

        /// <summary>
        /// Injected connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }
}