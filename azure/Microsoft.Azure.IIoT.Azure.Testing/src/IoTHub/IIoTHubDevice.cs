// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.Testing.IoTHub {
    using Microsoft.Azure.IIoT.Hub.Models;

    /// <summary>
    /// Storage record for device plus twin
    /// </summary>
    public interface IIoTHubDevice {

        /// <summary>
        /// Device
        /// </summary>
        DeviceModel Device { get; }

        /// <summary>
        /// Twin model
        /// </summary>
        DeviceTwinModel Twin { get; }
    }
}
