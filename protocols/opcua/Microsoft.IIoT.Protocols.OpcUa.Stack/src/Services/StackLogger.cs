// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa.Services {
    using Microsoft.Extensions.Logging;
    using Autofac;
    using Opc.Ua;
    using System;

    /// <summary>
    /// Injectable service that registers logger with stack
    /// </summary>
    public sealed class StackLogger : IStartable, IDisposable {

        /// <summary>
        /// Wrapped logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Create stack logger
        /// </summary>
        /// <param name="logger"></param>
        public StackLogger(ILogger logger) {
            Logger = logger;
        }

        /// <inheritdoc/>
        public void Start() {

            // Disable traditional logging
            Utils.SetTraceMask(0);
            Utils.SetTraceOutput(Utils.TraceOutput.Off);

            // Register callback
            Utils.Tracing.TraceEventHandler += Tracing_TraceEventHandler;
        }

        /// <inheritdoc/>
        public void Dispose() {

            // Unregister callback
            Utils.Tracing.TraceEventHandler -= Tracing_TraceEventHandler;
        }

        /// <summary>
        /// Helper to use when not using autofac di.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static StackLogger Create(ILogger logger) {
            var stackLogger = new StackLogger(logger);
            stackLogger.Start();
            return stackLogger;
        }

        /// <summary>
        /// Log to logger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tracing_TraceEventHandler(object sender, TraceEventArgs e) {
            if (!string.IsNullOrEmpty(e.Format) &&
                ShouldLog(e.TraceMask, out var level, out var traceName)) {
                Logger.Log(level, e.Exception, $"({traceName}) {e.Format}", e.Arguments);
            }
        }

        /// <summary>
        /// Convert to loglevel and trace name string
        /// </summary>
        /// <param name="traceMask"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool ShouldLog(int traceMask, out LogLevel level, out string name) {
            switch (traceMask) {
                case Utils.TraceMasks.Error:
                    level = LogLevel.Error;
                    name = nameof(Utils.TraceMasks.Error);
                    break;
                case Utils.TraceMasks.Information:
                    // level = LogLevel.Info; // TOO VERBOSE
                    level = LogLevel.Trace;
                    name = nameof(Utils.TraceMasks.Information);
                    break;
                case Utils.TraceMasks.StartStop:
                    level = LogLevel.Information;
                    name = nameof(Utils.TraceMasks.StartStop);
                    break;
                case Utils.TraceMasks.Operation:
                    level = LogLevel.Debug;
                    name = nameof(Utils.TraceMasks.Operation);
                    break;
                case Utils.TraceMasks.ExternalSystem:
                    level = LogLevel.Debug;
                    name = nameof(Utils.TraceMasks.ExternalSystem);
                    break;
                case Utils.TraceMasks.StackTrace:
                    level = LogLevel.Trace;
                    name = nameof(Utils.TraceMasks.Service);
                    break;
                case Utils.TraceMasks.Service:
                    level = LogLevel.Trace;
                    name = nameof(Utils.TraceMasks.Service);
                    break;
                case Utils.TraceMasks.Security:
                    level = LogLevel.Trace;
                    name = nameof(Utils.TraceMasks.Security);
                    break;
#if LOG_VERBOSE
                case Utils.TraceMasks.ServiceDetail:
                    level = LogLevel.Trace;
                    name = nameof(Utils.TraceMasks.ServiceDetail);
                    break;
                case Utils.TraceMasks.OperationDetail:
                    level = LogLevel.Trace;
                    name = nameof(Utils.TraceMasks.OperationDetail);
                    break;
#endif
                default:
                    level = LogLevel.Trace;
                    name = null;
                    return false;
            }
            return true;
        }
    }
}
