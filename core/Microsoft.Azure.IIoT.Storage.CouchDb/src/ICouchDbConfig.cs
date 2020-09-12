// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb {

    /// <summary>
    /// Configuration for Couchdb
    /// </summary>
    public interface ICouchDbConfig {

        /// <summary>
        /// Host to use
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// User name
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Key to use
        /// </summary>
        string Key { get; }
    }
}
