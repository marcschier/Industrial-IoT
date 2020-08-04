// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Devices.Client.Exceptions {
    using Microsoft.Azure.IIoT.Exceptions;
    using System;

    /// <summary>
    /// IoT Hub exception extension
    /// </summary>
    public static class ClientExceptionEx {

        /// <summary>
        /// Translate exception
        /// </summary>
        public static Exception Translate(this Exception ex) {
            switch (ex) {
                case DeviceNotFoundException dnf:
                    return new ResourceNotFoundException(dnf.Message, dnf);
                case UnauthorizedException ua:
                    return new ResourceUnauthorizedException(ua.Message, ua);
                case DeviceMaximumQueueDepthExceededException dm:
                    return new ResourceTooLargeException(dm.Message, dm); // TODO: Revisit
                case DeviceAlreadyExistsException dae:
                    return new ConflictingResourceException(dae.Message, dae);
                case QuotaExceededException qee:
                    return new ResourceExhaustionException(qee.Message, qee);
                case DeviceMessageLockLostException dle:
                    return new ResourceInvalidStateException(dle.Message, dle);
                case MessageTooLargeException mtl:
                    return new MessageSizeLimitException(mtl.Message, mtl);
                case ServerBusyException sb:
                    return new TemporarilyBusyException(sb.Message, sb);
                case IotHubThrottledException te:
                    return new TemporarilyBusyException(te.Message, te);
                case DeviceDisabledException dd:
                    return new ResourceUnauthorizedException(dd.Message, dd);
            }
            return ex;
        }
    }
}
