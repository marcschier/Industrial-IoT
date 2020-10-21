﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using System;

    /// <summary>
    /// State of the writer group
    /// </summary>
    public class WriterGroupStateModel {

        /// <summary>
        /// State indicator
        /// </summary>
        public WriterGroupStatus LastState { get; set; }

        /// <summary>
        /// Last state change
        /// </summary>
        public DateTime LastStateChange { get; set; }
    }
}