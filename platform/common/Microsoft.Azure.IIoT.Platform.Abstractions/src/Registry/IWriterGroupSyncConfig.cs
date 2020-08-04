// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Registry {
    using System;

    /// <summary>
    /// Configures group synchronization
    /// </summary>
    public interface IWriterGroupSyncConfig {

        /// <summary>
        /// Registry sync interval
        /// </summary>
        TimeSpan WriterGroupSyncInterval { get; }
    }
}