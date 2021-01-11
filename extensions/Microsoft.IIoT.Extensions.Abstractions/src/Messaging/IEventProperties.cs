// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System.Collections.Generic;

    /// <summary>
    /// Event properties
    /// </summary>
    public interface IEventProperties : IEnumerable<KeyValuePair<string, string>> {

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string this[string key] { get; }

        /// <summary>
        /// Try to get the value - key is case insensitive
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue(string key, out string value);
    }
}
