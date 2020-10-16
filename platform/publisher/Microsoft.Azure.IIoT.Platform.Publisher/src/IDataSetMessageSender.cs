// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Opc.Ua;
    using System.Collections.Generic;

    /// <summary>
    /// Dataset message sender
    /// </summary>
    public interface IDataSetMessageSender {

        /// <summary>
        /// Dataset writer group identifier
        /// </summary>
        string WriterGroupId { get; set; }

        /// <summary>
        /// Write notification
        /// </summary>
        /// <param name="dataSetWriter"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// <param name="messageContext"></param>
        void Write(DataSetWriterModel dataSetWriter,
            DataChangeNotification notification, 
            IList<string> stringTable,
            ServiceMessageContext messageContext);

        /// <summary>
        /// Write notification
        /// </summary>
        /// <param name="dataSetWriter"></param>
        /// <param name="notification"></param>
        /// <param name="stringTable"></param>
        /// <param name="messageContext"></param>
        void Write(DataSetWriterModel dataSetWriter,
            EventNotificationList notification, 
            IList<string> stringTable,
            ServiceMessageContext messageContext);
    }
}