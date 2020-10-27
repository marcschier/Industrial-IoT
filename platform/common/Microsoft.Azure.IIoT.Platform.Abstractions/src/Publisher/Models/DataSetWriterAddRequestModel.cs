// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Data set writer registration request
    /// </summary>
    public class DataSetWriterAddRequestModel {

        /// <summary>
        /// Name of the published dataset
        /// </summary>
        public string DataSetName { get; set; }

        /// <summary>
        /// Endpoint id to create dataset writer with
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// Dataset writer group the writer is part of or default group.
        /// The writer group must have the same site as the endpoint.
        /// </summary>
        public string WriterGroupId { get; set; }

        /// <summary>
        /// User credentials to use
        /// </summary>
        public CredentialModel User { get; set; }

        /// <summary>
        /// Dataset field content mask
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
