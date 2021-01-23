﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Data set writer registration update request
    /// </summary>
    public class DataSetWriterUpdateRequestModel {

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// New Name of the published dataset
        /// </summary>
        public string DataSetName { get; set; }

        /// <summary>
        /// Dataset writer group the writer should move to.
        /// The writer group must have the same site as the endpoint
        /// the writer was registered on.
        /// </summary>
        public string WriterGroupId { get; set; }

        /// <summary>
        /// New user credentials to use - set empty
        /// credential model to remove current.
        /// </summary>
        public CredentialModel User { get; set; }

        /// <summary>
        /// Updated content mask or 0 to set back to default.
        /// </summary>
        public DataSetFieldContentMask? DataSetFieldContentMask { get; set; }

        /// <summary>
        /// Data set message settings
        /// </summary>
        public DataSetWriterMessageSettingsModel MessageSettings { get; set; }

        /// <summary>
        /// Extension fields in the dataset
        /// </summary>
        public IReadOnlyDictionary<string, string> ExtensionFields { get; set; }

        /// <summary>
        /// Subscription settings (publisher extension)
        /// </summary>
        public PublishedDataSetSourceSettingsModel SubscriptionSettings { get; set; }
    }
}
