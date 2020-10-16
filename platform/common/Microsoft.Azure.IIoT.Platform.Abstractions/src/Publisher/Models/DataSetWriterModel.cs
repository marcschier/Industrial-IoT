﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    /// <summary>
    /// Data set writer
    /// </summary>
    public class DataSetWriterModel {

        /// <summary>
        /// Dataset writer id
        /// </summary>
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// Published dataset inline definition
        /// </summary>
        public PublishedDataSetModel DataSet { get; set; }

        /// <summary>
        /// Dataset field content mask
        /// </summary>
        public DataSetFieldContentMask? DataSetFieldContentMask { get; set; }

        /// <summary>
        /// Data set message settings
        /// </summary>
        public DataSetWriterMessageSettingsModel MessageSettings { get; set; }
    }
}
