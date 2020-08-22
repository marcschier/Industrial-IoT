// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb {

    /// <summary>
    /// Configuration for Lite db
    /// </summary>
    public interface ILiteDbConfig {

        /// <summary>
        /// Connection string to use
        /// </summary>
        string DbConnectionString { get; }
    }
}
