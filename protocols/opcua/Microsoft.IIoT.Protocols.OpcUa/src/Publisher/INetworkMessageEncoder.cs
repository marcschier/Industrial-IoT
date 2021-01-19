// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Encoder to encode data set writer messages
    /// </summary>
    public interface INetworkMessageEncoder {

        /// <summary>
        /// Messaging schema the encoder produces
        /// </summary>
        string MessageSchema { get; }

        /// <summary>
        /// Number of notifications that are too big to be processed to IotHub Messages
        /// </summary>
        long NotificationsDroppedCount { get; }

        /// <summary>
        /// Number of successfully processed notifications from OPC client
        /// </summary>
        long NotificationsProcessedCount { get; }

        /// <summary>
        /// Number of successfully processed messages
        /// </summary>
        long MessagesProcessedCount { get; }

        /// <summary>
        /// Average notifications in a message
        /// </summary>
        double AvgNotificationsPerMessage { get; }

        /// <summary>
        /// Average notifications in a message
        /// </summary>
        double AvgMessageSize { get; }

        /// <summary>
        /// Encodes the list of messages into single message NetworkMessageModel list
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="message"></param>
        /// <param name="headerLayoutProfile"></param>
        /// <param name="contentMask"></param>
        /// <param name="order"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        IList<NetworkMessageModel> Encode(string writerGroupId,
            IList<DataSetWriterMessageModel> message,
            string headerLayoutProfile, NetworkMessageContentMask? contentMask,
            DataSetOrderingType? order, int maxMessageSize);

        /// <summary>
        /// Encodes the list of messages into batched message list
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="headerLayoutProfile"></param>
        /// <param name="contentMask"></param>
        /// <param name="order"></param>
        /// <param name="messages"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        IList<NetworkMessageModel> EncodeBatch(string writerGroupId,
            IList<DataSetWriterMessageModel> messages,
            string headerLayoutProfile, NetworkMessageContentMask? contentMask,
            DataSetOrderingType? order, int maxMessageSize);
    }
}