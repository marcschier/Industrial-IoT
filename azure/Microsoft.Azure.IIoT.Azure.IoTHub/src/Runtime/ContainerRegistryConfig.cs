// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using System.Reflection;
    using System;

    /// <summary>
    /// Container registry configuration
    /// </summary>
    public class ContainerRegistryConfig : PostConfigureOptionBase<ContainerRegistryOptions> {

        /// <inheritdoc/>
        public ContainerRegistryConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, ContainerRegistryOptions options) {
            if (string.IsNullOrEmpty(options.DockerServer)) {
                options.DockerServer = GetStringOrDefault(PcsVariable.PCS_DOCKER_SERVER);
            }
            if (string.IsNullOrEmpty(options.DockerUser)) {
                options.DockerUser = GetStringOrDefault(PcsVariable.PCS_DOCKER_USER);
            }
            if (string.IsNullOrEmpty(options.DockerPassword)) {
                options.DockerPassword = GetStringOrDefault(PcsVariable.PCS_DOCKER_PASSWORD);
            }
            if (string.IsNullOrEmpty(options.ImagesNamespace)) {
                options.ImagesNamespace = GetStringOrDefault(PcsVariable.PCS_IMAGES_NAMESPACE);
            }
            if (string.IsNullOrEmpty(options.ImagesTag)) {
                options.ImagesTag = GetStringOrDefault(PcsVariable.PCS_IMAGES_TAG,
                    Assembly.GetExecutingAssembly().GetReleaseVersion().ToString(3));
            }
        }
    }
}
