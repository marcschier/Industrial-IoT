// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Confluent.Kafka {
    using Serilog.Events;
    using Serilog;

    /// <summary>
    /// Logger extensions
    /// </summary>
    public static class LoggerEx {

        /// <summary>
        /// Handle log message
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        internal static void Log(this ILogger logger, LogMessage msg) {
            LogEventLevel level;
            switch (msg.Level) {
                case SyslogLevel.Emergency:
                case SyslogLevel.Critical:
                case SyslogLevel.Warning:
                case SyslogLevel.Alert:
                    level = LogEventLevel.Warning;
                    break;
                case SyslogLevel.Error:
                    level = LogEventLevel.Error;
                    break;
                case SyslogLevel.Notice:
                case SyslogLevel.Info:
                    level = LogEventLevel.Information;
                    break;
                case SyslogLevel.Debug:
                    level = LogEventLevel.Debug;
                    break;
                default:
                    return;
            }
            logger.Write(level, "[{facility}] {name}: {message}",
                msg.Facility, msg.Name, msg.Message);
        }
    }
}
