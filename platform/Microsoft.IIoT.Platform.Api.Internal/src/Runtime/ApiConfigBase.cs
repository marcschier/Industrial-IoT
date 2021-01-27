// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Api.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Base api configuration
    /// </summary>
    public abstract class ApiConfigBase : ConfigureOptionBase {

        /// <inheritdoc/>
        public ApiConfigBase(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
