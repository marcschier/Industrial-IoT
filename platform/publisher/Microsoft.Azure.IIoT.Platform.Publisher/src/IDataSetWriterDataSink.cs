// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Opc.Ua;
    using Opc.Ua.Client;
    using System.Collections.Generic;

    /// <summary>
    /// Dataset writer sink
    /// </summary>
    public interface IDataSetWriterDataSink {

        /// <summary>
        /// Write notification
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSet"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// <param name="subscription"></param>
        void OnDataSetNotification(string dataSetWriterId, PublishedDataSetModel dataSet, 
            uint sequenceNumber, NotificationData notification, 
            IList<string> stringTable, Subscription subscription);
    }
}