﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Enum definition
    /// </summary>
    public class EnumDefinitionModel {

        /// <summary>
        /// The fields of the enum
        /// </summary>
        public List<EnumFieldModel> Fields { get; set; }
    }
}
