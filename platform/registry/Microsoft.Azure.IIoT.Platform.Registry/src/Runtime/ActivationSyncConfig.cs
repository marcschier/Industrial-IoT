﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Activation update configuration
    /// </summary>
    public class ActivationSyncConfig : ConfigBase, IActivationSyncConfig {

        /// <summary>
        /// Configuration keys
        /// </summary>
        private const string kUpdateIntervalKey = "SyncInterval";

        /// <inheritdoc/>
        public TimeSpan? ActivationSyncInterval => GetDurationOrNull(kUpdateIntervalKey);

        /// <summary>
        /// Create config
        /// </summary>
        /// <param name="configuration"></param>
        public ActivationSyncConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}