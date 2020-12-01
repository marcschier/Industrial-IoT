// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.Configuration {
    using Microsoft.IIoT.Configuration;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class ConfigurationBuilderEx {

        /// <summary>
        /// Adds .env file environment variables from an .env file that is in current
        /// folder or below up to root.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddFromDotEnvFile(this IConfigurationBuilder builder) {
            return builder.Add(new DotEnvFileSource());
        }
    }
}
