// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configure a specific container to open
    /// </summary>
    internal sealed class CollectionFactoryConfig : PostConfigureOptionBase<CollectionFactoryOptions> {

        /// <inheritdoc/>
        public CollectionFactoryConfig(IConfiguration configuration = null) 
            : base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, CollectionFactoryOptions options) {
            if (string.IsNullOrEmpty(options.DatabaseName)) {
                options.DatabaseName = "iiot_opc";
            }
            if (string.IsNullOrEmpty(options.ContainerName)) {
                options.ContainerName = name;
            }
            if (string.IsNullOrEmpty(options.ContainerName)) {
                options.ContainerName = "iiot_opc";
            }
        }
    }
}
