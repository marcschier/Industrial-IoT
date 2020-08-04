// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting {
    using System;

    /// <summary>
    /// Ignore method or property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method,
        AllowMultiple = true)]
    public class IgnoreAttribute : Attribute {
    }
}
