// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Controllers {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Filters;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Rpc;
    using System;

    /// <summary>
    /// Writer group methods controller
    /// </summary>
    [Version(1)]
    [Version(2)]
    [ExceptionsFilter]
    public class WriterGroupMethodsController : IMethodController {

        /// <summary>
        /// Create controller with service
        /// </summary>
        public WriterGroupMethodsController() {
        }
    }
}
