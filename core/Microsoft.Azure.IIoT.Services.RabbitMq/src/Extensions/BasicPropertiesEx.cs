// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace RabbitMQ.Client {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Extensions
    /// </summary>
    internal static class BasicPropertiesEx {

        /// <summary>
        /// Convert to dictionary
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="sequenceNumber"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(
            this IBasicProperties properties, ulong sequenceNumber) {
            var result = properties?.Headers?
                .ToDictionary(k => k.Key, v => {
                    if (v.Value is byte[] b) {
                        return Encoding.UTF8.GetString(b);
                    }
                    return v.Value?.ToString();
                }) ?? new Dictionary<string, string>();

            result.Add("x-sequenceNumber", sequenceNumber.ToString());
            // ...
            return result;
        }
    }
}
