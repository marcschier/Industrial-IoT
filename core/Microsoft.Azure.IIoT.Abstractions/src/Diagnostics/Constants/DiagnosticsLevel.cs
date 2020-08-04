// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using System;

    /// <summary>
    /// Diagnostics settings
    /// </summary>
    [Flags]
    public enum DiagnosticsLevel {

        /// <summary>
        /// Non PII usage telemetry is sent to Microsoft.
        /// </summary>
        Public = 0x1,

        /// <summary>
        /// Push metrics
        /// </summary>
        PushMetrics = 0x2,

        /// <summary>
        /// Disable metrics sending
        /// </summary>
        NoMetrics = 0x10,

        /// <summary>
        /// Disable traces
        /// </summary>
        NoTraces = 0x20,

        /// <summary>
        /// Disable Logs
        /// </summary>
        NoLogs = 0x40,

        // ...

        /// <summary>
        /// Disable all
        /// </summary>
        Disabled = 0xf0
    }
}
