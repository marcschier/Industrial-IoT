// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Storage {

    /// <summary>
    /// Configure a specific container to open
    /// </summary>
    public class CollectionFactoryOptions : ContainerOptions {

        /// <summary>
        /// Name of database
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Name of container
        /// </summary>
        public string ContainerName { get; set; }
    }
}
