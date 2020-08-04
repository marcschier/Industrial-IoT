// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Rpc {
    using Microsoft.Azure.IIoT.Serializers;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Send property update events as module or device identity
    /// </summary>
    public interface ISettingsReporter {

        /// <summary>
        /// Send property changed notification
        /// </summary>
        /// <param name="propertyId">property id</param>
        /// <param name="value">property value</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ReportAsync(string propertyId, VariantValue value,
            CancellationToken ct = default);

        /// <summary>
        /// Send property changed notifications
        /// </summary>
        /// <param name="properties">property id</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ReportAsync(
            IEnumerable<KeyValuePair<string, VariantValue>> properties,
            CancellationToken ct = default);
    }
}
