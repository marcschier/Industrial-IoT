﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a method invoker
    /// </summary>
    public interface IMethodInvoker : IDisposable {

        /// <summary>
        /// Name of method to handle
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// Invoke method and return result.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="contentType"></param>
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<byte[]> InvokeAsync(string target, byte[] payload,
            string contentType, IMethodHandler context);
    }
}
