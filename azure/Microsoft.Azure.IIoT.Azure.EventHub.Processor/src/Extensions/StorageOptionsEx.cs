// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor {
    using Microsoft.Azure.IIoT.Utils;

    /// <summary>
    /// Storage options extension
    /// </summary>
    internal static class StorageOptionsEx {

        /// <summary>
        /// Get blob storage connection string
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static string GetStorageConnString(this StorageOptions options) {
            var account = options.AccountName;
            var key = options.AccountKey;
            var suffix = options.EndpointSuffix;
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(key)) {
                return null;
            }
            return ConnectionString.CreateStorageConnectionString(
                account, suffix, key, "https").ToString();
        }
    }
}
