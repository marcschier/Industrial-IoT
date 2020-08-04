// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher edge client
    /// </summary>
    public interface IDataSetWriterRegistryEdgeClient {

        /// <summary>
        /// Get dataset writer
        /// </summary>
        /// <param name="serviceUrl"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterModel> GetDataSetWriterAsync(string serviceUrl,
            string dataSetWriterId, CancellationToken ct);
    }
}