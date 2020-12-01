// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Services {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Writes to a dataset writer sink
    /// </summary>
    public interface IDataSetWriterDataSource {

        /// <summary>
        /// Dataset writer id
        /// </summary>
        string DataSetWriterId { get; set; }

        /// <summary>
        /// Configuration updates
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        Task ConfigureAsync(PublishedDataSetModel dataSet);
    }
}