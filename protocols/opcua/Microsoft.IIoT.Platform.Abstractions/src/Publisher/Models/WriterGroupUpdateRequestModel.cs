﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Models {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Writer group update request
    /// </summary>
    public class WriterGroupUpdateRequestModel {

        /// <summary>
        /// Name of the writer group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// Message encoding to generate (publisher extension)
        /// </summary>
        public NetworkMessageEncoding? Encoding { get; set; }

        /// <summary>
        /// Updated network message configuration
        /// </summary>
        public WriterGroupMessageSettingsModel MessageSettings { get; set; }

        /// <summary>
        /// Header layout uri
        /// </summary>
        public string HeaderLayoutUri { get; set; }

        /// <summary>
        /// Batch buffer size
        /// </summary>
        public int? BatchSize { get; set; }

        /// <summary>
        /// Publishing interval
        /// </summary>
        public TimeSpan? PublishingInterval { get; set; }

        /// <summary>
        /// Keep alive time
        /// </summary>
        public TimeSpan? KeepAliveTime { get; set; }

        /// <summary>
        /// Priority of the writer group
        /// </summary>
        public byte? Priority { get; set; }

        /// <summary>
        /// Locales to use
        /// </summary>
        public IReadOnlyList<string> LocaleIds { get; set; }
    }
}