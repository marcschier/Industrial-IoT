// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Metadata for the published dataset
    /// </summary>
    public class DataSetMetaDataModel {

        /// <summary>
        /// Name of the dataset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the dataset
        /// </summary>
        public LocalizedTextModel Description { get; set; }

        /// <summary>
        /// Metadata for the data set fiels
        /// </summary>
        public IReadOnlyList<FieldMetaDataModel> Fields { get; set; }

        /// <summary>
        /// Dataset class id
        /// </summary>
        public Guid DataSetClassId { get; set; }

        /// <summary>
        /// Dataset version
        /// </summary>
        public ConfigurationVersionModel ConfigurationVersion { get; set; }

        /// <summary>
        /// Namespaces in the metadata description
        /// </summary>
        public IReadOnlyList<string> Namespaces { get; set; }

        /// <summary>
        /// Structure data types
        /// </summary>
        public IReadOnlyList<StructureDescriptionModel> StructureDataTypes { get; set; }

        /// <summary>
        /// Enum data types
        /// </summary>
        public IReadOnlyList<EnumDescriptionModel> EnumDataTypes { get; set; }

        /// <summary>
        /// Simple data type.
        /// </summary>
        public IReadOnlyList<SimpleTypeDescriptionModel> SimpleDataTypes { get; set; }
    }
}
