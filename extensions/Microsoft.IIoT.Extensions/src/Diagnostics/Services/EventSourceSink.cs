// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Diagnostics {
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics.Tracing;
    using System.Text;

    /// <summary>
    /// Event source logger
    /// </summary>
    public class EventSourceSink : IEventSourceSubscriber {

        /// <summary>
        /// Level
        /// </summary>
        public EventLevel Level { get; set; }

        /// <summary>
        /// Create bridge
        /// </summary>
        /// <param name="logger"></param>
        public EventSourceSink(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public virtual void OnEvent(EventWrittenEventArgs eventData) {
            WriteEvent(ToLogLevel(eventData.Level), eventData);
        }

        /// <summary>
        /// Write event to logger
        /// </summary>
        /// <param name="level"></param>
        /// <param name="eventData"></param>
        protected void WriteEvent(LogLevel level, EventWrittenEventArgs eventData) {
            var parameters = new object[eventData.Payload.Count + 4];
            parameters[0] = eventData.EventName;
            parameters[1] = eventData.Level;
            for (var i = 0; i < eventData.Payload.Count; i++) {
                parameters[2 + i] = eventData.Payload[i];
            }
            parameters[2 + eventData.Payload.Count + 0] = eventData.Message;
            parameters[2 + eventData.Payload.Count + 1] = eventData;
            var template = new StringBuilder();
            template.Append("[{event}] {level}: ");
            foreach (var name in eventData.PayloadNames) {
                template.Append('{');
                template.Append(name);
                template.Append("} ");
            }
            if (eventData.Message != null) {
                template.Append("{msg}");
            }
            _logger.Log(level, template.ToString(), parameters);
        }

        /// <summary>
        /// Convert to log event
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected static LogLevel ToLogLevel(EventLevel level) {
            switch (level) {
                case EventLevel.Critical:
                    return LogLevel.Critical;
                case EventLevel.Warning:
                    return LogLevel.Warning;
                case EventLevel.Error:
                    return LogLevel.Error;
                case EventLevel.Informational:
                    return LogLevel.Information;
                case EventLevel.Verbose:
                    return LogLevel.Trace;
                default:
                    return LogLevel.Debug;
            }
        }

        /// <summary>
        /// Convert to event level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected static EventLevel ToEventLevel(LogLevel level) {
            switch (level) {
                case LogLevel.Critical:
                    return EventLevel.Critical;
                case LogLevel.Error:
                    return EventLevel.Error;
                case LogLevel.Warning:
                    return EventLevel.Warning;
                case LogLevel.Debug:
                case LogLevel.Information:
                    return EventLevel.Informational;
                case LogLevel.Trace:
                    return EventLevel.Verbose;
                default:
                    return EventLevel.LogAlways;
            }
        }

        /// <summary> Logger </summary>
        protected readonly ILogger _logger;
    }
}