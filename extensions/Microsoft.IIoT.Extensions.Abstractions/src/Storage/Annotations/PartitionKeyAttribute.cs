// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Storage {
    using System;

    /// <summary>
    /// Partition key
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false, Inherited = true)]
    public class PartitionKeyAttribute : Attribute {
    }
}
