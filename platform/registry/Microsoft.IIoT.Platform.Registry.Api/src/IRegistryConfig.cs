// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api {

    /// <summary>
    /// Configuration for service
    /// </summary>
    public interface IRegistryConfig {

        /// <summary>
        /// Opc registry service url
        /// </summary>
        string RegistryServiceUrl { get; }
    }
}
