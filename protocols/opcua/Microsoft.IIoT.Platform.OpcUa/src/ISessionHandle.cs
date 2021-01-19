// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa {
    using Microsoft.IIoT.Platform.Core.Models;
    using Opc.Ua.Client;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Represents session handle
    /// </summary>
    public interface ISessionHandle : IDisposable {

        /// <summary>
        /// Connection
        /// </summary>
        ConnectionModel Connection { get; }

        /// <summary>
        /// State
        /// </summary>
        ConnectionStatus State { get; }

        /// <summary>
        /// Session
        /// </summary>
        Session Session { get; }

        /// <summary>
        /// Get access to the raw session
        /// </summary>
        Task<Session> AcquireSessionAsync();
    }
}
