// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Opc.Ua;
    using System.Collections.Generic;

    /// <summary>
    /// Data set writer message encoder and decoder
    /// </summary>
    public interface IDataSetWriterNotificationEncoder {

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="header"></param>
        /// <param name="payload"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// <param name="context"></param>
        void Decode(byte[] header, byte[] payload, out string dataSetWriterId, 
            out uint sequenceNumber, out NotificationData notification,
            out IList<string> stringTable, ref ServiceMessageContext context);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// <param name="header"></param>
        /// <param name="payload"></param>
        /// <param name="context"></param>
        void Encode(string dataSetWriterId, uint sequenceNumber,
            NotificationData notification, IList<string> stringTable,
            out byte[] header, out byte[] payload, ref ServiceMessageContext context);
    }
}