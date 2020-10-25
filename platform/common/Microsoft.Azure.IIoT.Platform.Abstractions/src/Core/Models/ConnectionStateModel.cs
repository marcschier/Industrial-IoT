﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    using System;

    /// <summary>
    /// State of the connection
    /// </summary>
    public class ConnectionStateModel {

        /// <summary>
        /// Last connection state
        /// </summary>
        public ConnectionStatus State { get; set; }

        /// <summary>
        /// Last operation result
        /// </summary>
        public ServiceResultModel LastResult { get; set; }

        /// <summary>
        /// Last result change
        /// </summary>
        public DateTime? LastResultChange { get; set; }
    }
}