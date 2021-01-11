﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage {
    using System;

    /// <summary>
    /// Get a locked file
    /// </summary>
    public interface IFileLock : IAsyncDisposable {

        /// <summary>
        /// The locked file
        /// </summary>
        IFile File { get; }
    }
}
