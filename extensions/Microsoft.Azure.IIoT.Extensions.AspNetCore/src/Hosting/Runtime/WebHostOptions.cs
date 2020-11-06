// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting {

    /// <summary>
    /// Host configuration
    /// </summary>
    public class WebHostOptions {

        /// <summary>
        /// Whether to use https redirect and hsts
        /// </summary>
        public bool UseHttpsRedirect { get; set; }

        /// <summary>
        /// URL path base that service should be running on.
        /// </summary>
        public string ServicePathBase { get; set; }
    }
}
