// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using System;

    /// <summary>
    /// Setting sync configuration
    /// </summary>
    public interface ISettingsSyncConfig {

        /// <summary>
        /// Update settings interval
        /// </summary>
        TimeSpan? SettingSyncInterval { get; }
    }
}
