// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Storage.Models {

    /// <summary>
    /// Publisher identity types
    /// </summary>
    public static class IdentityType {

        /// <summary>
        /// Writer group identity
        /// </summary>
        public const string WriterGroup = "WriterGroup";

        /// <summary>
        /// Dataset identity
        /// </summary>
        public const string DataSetWriter = "DataSetWriter";

        /// <summary>
        /// Dataset item
        /// </summary>
        public const string DataSetEntity = "DataSetEntity";
    }
}
