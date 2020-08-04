// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor {
    using Microsoft.Azure.IIoT.Utils;

    /// <summary>
    /// Storage configuration extension
    /// </summary>
    internal static class StorageConfigEx {

        /// <summary>
        /// Get blob storage connection string
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static string GetStorageConnString(this IStorageConfig config) {
            var account = config.AccountName;
            var key = config.AccountKey;
            var suffix = config.EndpointSuffix;
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(key)) {
                return null;
            }
            return ConnectionString.CreateStorageConnectionString(
                account, suffix, key, "https").ToString();
        }
    }
}
