// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge {
    using System.Collections.Generic;

    /// <summary>
    /// Handles changes and propagates to processors
    /// </summary>s
    public interface IDataSetWriterRegistryLoader {

        /// <summary>
        /// Dataset writer identifiers
        /// </summary>
        IDictionary<string, string> LoadState { get; }

        /// <summary>
        /// Dataset writer was added to group
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <returns></returns>
        void OnDataSetWriterChanged(string dataSetWriterId);

        /// <summary>
        /// Dataset writer was added to group
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <returns></returns>
        void OnDataSetWriterRemoved(string dataSetWriterId);
    }
}