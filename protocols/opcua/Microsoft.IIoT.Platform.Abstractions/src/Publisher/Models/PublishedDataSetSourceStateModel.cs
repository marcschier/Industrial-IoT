﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// State of the dataset writer source
    /// </summary>
    public class PublishedDataSetSourceStateModel {

        /// <summary>
        /// Connection state
        /// </summary>
        public ConnectionStateModel ConnectionState { get; set; }

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