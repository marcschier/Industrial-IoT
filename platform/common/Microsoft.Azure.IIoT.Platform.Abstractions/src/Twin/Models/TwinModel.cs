// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Twin model
    /// </summary>
    public class TwinModel {

        /// <summary>
        /// Connection identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        public ConnectionModel Connection { get; set; }

        /// <summary>
        /// The last state of this connection
        /// </summary>
        public ConnectionStateModel ConnectionState { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        public OperationContextModel Created { get; set; }

        /// <summary>
        /// Updated
        /// </summary>
        public OperationContextModel Updated { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
