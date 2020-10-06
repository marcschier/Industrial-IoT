// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Runtime {
    using Microsoft.Azure.IIoT.Api.Runtime;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration - wraps a configuration root
    /// </summary>
    public class DirectoryConfig : ApiConfigBase, IDirectoryConfig {

        /// <summary>
        /// Registry configuration
        /// </summary>
        private const string kDirectoryServiceUrlKey = "DirectoryServiceUrl";

        /// <summary>Directory endpoint url</summary>
        public string DirectoryServiceUrl => GetStringOrDefault(
            kDirectoryServiceUrlKey,
            () => GetStringOrDefault(PcsVariable.PCS_DIRECTORY_SERVICE_URL,
                () => GetDefaultUrl("9043", "directory")));

        /// <inheritdoc/>
        public DirectoryConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
