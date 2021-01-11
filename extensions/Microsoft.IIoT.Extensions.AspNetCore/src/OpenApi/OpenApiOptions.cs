// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.OpenApi {

    /// <summary>
    /// OpenApi / Swagger configuration
    /// </summary>
    public class OpenApiOptions {

        /// <summary>
        /// Whether openapi should be enabled
        /// </summary>
        public bool UIEnabled { get; set; }

        /// <summary>
        /// Whether authentication should be added to openapi ui
        /// </summary>
        public bool WithAuth { get; set; }

        /// <summary>
        /// Open api version (v2 = json, v3 = yaml)
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The Application id for the openapi UI client.
        /// (optional - if not set uses bearer)
        /// </summary>
        public string OpenApiAppId { get; set; }

        /// <summary>
        /// Application secret (optional)
        /// </summary>
        public string OpenApiAppSecret { get; set; }

        /// <summary>
        /// Authorization Url
        /// (optional - if not set uses bearer)
        /// </summary>
        public string OpenApiAuthorizationEndpoint { get; set; }

        /// <summary>
        /// Server host for openapi (optional)
        /// </summary>
        public string OpenApiServerHost { get; set; }
    }
}
