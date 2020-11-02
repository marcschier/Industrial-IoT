// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb {

    /// <summary>
    /// Configuration for cosmos db
    /// </summary>
    public class CosmosDbOptions {

        /// <summary>
        /// Connection string to use (mandatory)
        /// </summary>
        public string DbConnectionString { get; set; }

        /// <summary>
        /// Throughput units (optional)
        /// </summary>
        public int? ThroughputUnits { get; set; }
    }
}
