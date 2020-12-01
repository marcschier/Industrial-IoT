// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub {

    /// <summary>
    /// Container registry client configuration
    /// </summary>
    public class ContainerRegistryOptions {

        /// <summary>
        /// Server url
        /// </summary>
        public string DockerServer { get; set; }

        /// <summary>
        /// User
        /// </summary>
        public string DockerUser { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string DockerPassword { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string ImagesNamespace { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public string ImagesTag { get; set; }
    }
}