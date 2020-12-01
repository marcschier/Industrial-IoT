// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Diagnostics {
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Log utils
    /// </summary>
    public static class Log {

        /// <summary>
        /// Console logger
        /// </summary>
        /// <returns></returns>
        public static ILogger Console(LogLevel? level = null) {
            if (level == null) {
#if DEBUG
                level = LogLevel.Debug;
#else
                level = LogLevel.Information;
#endif
            }
            return LoggerFactory.Create(builder => {
                builder.AddConsole();
                builder.AddDebug();
            }).CreateLogger(typeof(Log));
        }
    }
}
