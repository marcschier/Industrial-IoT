// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Opc.Ua;
    using Opc.Ua.Extensions;
    using Opc.Ua.PubSub;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Globalization;

    /// <summary>
    /// Creates pub/sub encoded messages
    /// </summary>
    public class UadpNetworkMessageEncoder : INetworkMessageEncoder {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.NetworkMessageUadp;

        /// <inheritdoc/>
        public long NotificationsDroppedCount => _notificationsDroppedCount;

        /// <inheritdoc/>
        public long NotificationsProcessedCount => _notificationsProcessedCount;

        /// <inheritdoc/>
        public long MessagesProcessedCount => _messagesProcessedCount;

        /// <inheritdoc/>
        public double AvgNotificationsPerMessage { get; private set; }

        /// <inheritdoc/>
        public double AvgMessageSize { get; private set; }

        /// <inheritdoc/>
        public IList<NetworkMessageModel> EncodeBatch(string writerGroupId,
            IList<DataSetWriterMessageModel> messages,
            string headerLayoutProfile, NetworkMessageContentMask? contentMask,
            Publisher.Models.DataSetOrderingType? order, int maxMessageSize) {
            return EncodeBatch(writerGroupId, messages, contentMask, maxMessageSize).ToList();
        }

        /// <inheritdoc/>
        public IList<NetworkMessageModel> Encode(string writerGroupId,
            IList<DataSetWriterMessageModel> messages,
            string headerLayoutProfile, NetworkMessageContentMask? contentMask,
            Publisher.Models.DataSetOrderingType? order, int maxMessageSize) {
            return Encode(writerGroupId, messages, contentMask, maxMessageSize).ToList();
        }

        /// <summary>
        /// Encode batches
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="messages"></param>
        /// <param name="contentMask"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessageModel> EncodeBatch(string writerGroupId,
            IEnumerable<DataSetWriterMessageModel> messages,
            NetworkMessageContentMask? contentMask, int maxMessageSize) {

            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetNetworkMessages(writerGroupId, contentMask, messages, encodingContext);
            if (!notifications.Any()) {
                yield break;
            }
            var current = notifications.GetEnumerator();
            var processing = current.MoveNext();
            var messageSize = 4; // array length size
            maxMessageSize -= 2048; // reserve 2k for header
            var chunk = new Collection<NetworkMessage>();
            while (processing) {
                var notification = current.Current;
                var messageCompleted = false;
                if (notification != null) {
                    using var helperEncoder = new BinaryEncoder(encodingContext);
                    helperEncoder.WriteEncodeable(null, notification);
                    var notificationSize = helperEncoder.CloseAndReturnBuffer().Length;
                    if (notificationSize > maxMessageSize) {
                        // we cannot handle this notification. Drop it.
                        // TODO Trace
                        Interlocked.Increment(ref _notificationsDroppedCount);
                        processing = current.MoveNext();
                    }
                    else {
                        messageCompleted = maxMessageSize < (messageSize + notificationSize);

                        if (!messageCompleted) {
                            chunk.Add(notification);
                            Interlocked.Increment(ref _notificationsProcessedCount);
                            processing = current.MoveNext();
                            messageSize += notificationSize;
                        }
                    }
                }
                if (!processing || messageCompleted) {
                    using var encoder = new BinaryEncoder(encodingContext);
                    encoder.WriteBoolean(null, true); // is Batch
                    encoder.WriteEncodeableArray(null, chunk);
                    var encoded = new NetworkMessageModel {
                        Body = encoder.CloseAndReturnBuffer(),
                        Timestamp = DateTime.UtcNow,
                        ContentType = ContentMimeType.Uadp,
                        MessageSchema = MessageSchemaTypes.NetworkMessageUadp
                    };
                    AvgMessageSize = ((AvgMessageSize * MessagesProcessedCount) + encoded.Body.Length) /
                        (MessagesProcessedCount + 1);
                    AvgNotificationsPerMessage = ((AvgNotificationsPerMessage * MessagesProcessedCount) +
                        chunk.Count) / (MessagesProcessedCount + 1);
                    Interlocked.Increment(ref _messagesProcessedCount);
                    chunk.Clear();
                    messageSize = 4;
                    yield return encoded;
                }
            }
        }

        /// <summary>
        /// Encode message
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="messages"></param>
        /// <param name="contentMask"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessageModel> Encode(string writerGroupId,
            IEnumerable<DataSetWriterMessageModel> messages, NetworkMessageContentMask? contentMask,
            int maxMessageSize) {
            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetNetworkMessages(writerGroupId, contentMask, messages, encodingContext);
            if (!notifications.Any()) {
                yield break;
            }
            foreach (var networkMessage in notifications) {
                using var encoder = new BinaryEncoder(encodingContext);
                encoder.WriteBoolean(null, false); // is not Batch
                encoder.WriteEncodeable(null, networkMessage);
                networkMessage.Encode(encoder);
                var encoded = new NetworkMessageModel {
                    Body = encoder.CloseAndReturnBuffer(),
                    Timestamp = DateTime.UtcNow,
                    ContentType = ContentMimeType.Uadp,
                    MessageSchema = MessageSchemaTypes.NetworkMessageUadp
                };
                if (encoded.Body.Length > maxMessageSize) {
                    // this message is too large to be processed. Drop it
                    // TODO Trace
                    Interlocked.Increment(ref _notificationsProcessedCount);
                    yield break;
                }
                Interlocked.Increment(ref _notificationsProcessedCount);
                AvgMessageSize = ((AvgMessageSize * MessagesProcessedCount) + encoded.Body.Length) /
                    (MessagesProcessedCount + 1);
                AvgNotificationsPerMessage = ((AvgNotificationsPerMessage * MessagesProcessedCount) + 1) /
                    (MessagesProcessedCount + 1);
                Interlocked.Increment(ref _messagesProcessedCount);
                yield return encoded;
            }
        }

        /// <summary>
        /// Produce network messages from the data set message model
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="contentMask"></param>
        /// <param name="messages"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessage> GetNetworkMessages(string writerGroupId,
            NetworkMessageContentMask? contentMask, IEnumerable<DataSetWriterMessageModel> messages,
            ServiceMessageContext context) {
            if (context?.NamespaceUris == null) {
                // declare all notifications in messages dropped
                foreach (var message in messages) {
                    Interlocked.Add(ref _notificationsDroppedCount, message?.Notifications?.Count() ?? 0);
                }
                yield break;
            }

            // TODO: Honor single message
            // TODO: Group by writer
            foreach (var message in messages) {
                var networkMessage = new NetworkMessage {
                    MessageContentMask = contentMask.ToStackType(MessageEncoding.Uadp),
                    PublisherId = writerGroupId,
                    DataSetClassId = message.Writer?.DataSet?
                        .DataSetMetaData?.DataSetClassId.ToString(),
                    MessageId = message.SequenceNumber.ToString(CultureInfo.InvariantCulture)
                };
                var dataSetMessages = new List<DataSetMessage>();
                var notificationQueues = message.Notifications.GroupBy(m => m.NodeId)
                    .Select(c => new Queue<MonitoredItemNotificationModel>(c.ToArray())).ToArray();
                while (notificationQueues.Where(q => q.Any()).Any()) {
                    var payload = notificationQueues
                        .Select(q => q.Any() ? q.Dequeue() : null)
                            .Where(s => s != null)
                                .ToDictionary(
                                    s => s.NodeId.ToExpandedNodeId(context.NamespaceUris)
                                        .AsString(message.ServiceMessageContext),
                                    s => s.Value);
                    var dataSetMessage = new DataSetMessage(
                        new DataSet(payload, (uint)message.Writer?.DataSetFieldContentMask.ToStackType())) {
                        DataSetWriterId = message.Writer.DataSetWriterId,
                        MetaDataVersion = new ConfigurationVersionDataType {
                            MajorVersion = message.Writer?.DataSet?.DataSetMetaData?
                                .ConfigurationVersion?.MajorVersion ?? 1,
                            MinorVersion = message.Writer?.DataSet?.DataSetMetaData?
                                .ConfigurationVersion?.MinorVersion ?? 0
                        },
                        MessageContentMask = (message.Writer?.MessageSettings?.DataSetMessageContentMask)
                            .ToStackType(MessageEncoding.Uadp),
                        Timestamp = message.TimeStamp ?? DateTime.UtcNow,
                        SequenceNumber = message.SequenceNumber,
                        Status = payload.Values.Any(s => StatusCode.IsNotGood(s.StatusCode)) ?
                            StatusCodes.Bad : StatusCodes.Good,
                    };
                    dataSetMessages.Add(dataSetMessage);
                }
                networkMessage.Messages = dataSetMessages;
                yield return networkMessage;
            }
        }

        private long _notificationsDroppedCount;
        private long _notificationsProcessedCount;
        private long _messagesProcessedCount;
    }
}