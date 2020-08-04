// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Entity activation services
    /// </summary>
    public interface IActivationServices<T> {

        /// <summary>
        /// Activate entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="secret"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ActivateAsync(T id, string secret,
            CancellationToken ct = default);

        /// <summary>
        /// Deactivate entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeactivateAsync(T id,
            CancellationToken ct = default);
    }
}
