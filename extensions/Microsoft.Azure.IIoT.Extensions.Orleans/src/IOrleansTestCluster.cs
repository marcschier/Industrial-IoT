// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Extensions.Orleans.Testing {
    using global::Orleans.TestingHost;

    /// <summary>
    /// Test cluster accessor
    /// </summary>
    public interface IOrleansTestCluster {

        /// <summary>
        /// Access test cluster
        /// </summary>
        TestCluster Cluster { get; }
    }
}