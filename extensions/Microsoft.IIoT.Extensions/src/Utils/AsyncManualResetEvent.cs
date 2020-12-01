// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Utils {
    /// <summary>
    /// Manual reset event
    /// </summary>
    public class AsyncManualResetEvent : AsyncEvent<bool> {

        /// <summary>
        /// Signal
        /// </summary>
        public void Set() {
            Set(true);
        }
    }
}
