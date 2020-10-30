// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Grains {
    using global::Orleans;

    /// <summary>
    /// Grain as grain factory adapter
    /// </summary>
    public class OrleansGrainClient : IOrleansGrainClient {

        /// <inheritdoc/>
        public IGrainFactory Grains { get; }

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="factory"></param>
        public OrleansGrainClient(IGrainFactory factory) {
            Grains = factory;
        }
    }
}