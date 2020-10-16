// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Opc.Ua;
    using Opc.Ua.Extensions;
    using Opc.Ua.PubSub;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Publisher monitored item message encoder
    /// </summary>
    public class BinarySampleMessageEncoder : INetworkMessageEncoder {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.MonitoredItemMessageBinary;

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
            return EncodeBatch(messages, maxMessageSize).ToList();
        }

        /// <inheritdoc/>
        public IList<NetworkMessageModel> Encode(string writerGroupId,
            IList<DataSetWriterMessageModel> messages,
            string headerLayoutProfile, NetworkMessageContentMask? contentMask,
            Publisher.Models.DataSetOrderingType? order, int maxMessageSize) {
            return Encode(messages, maxMessageSize).ToList();
        }

        /// <summary>
        /// Encode batches
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        public IEnumerable<NetworkMessageModel> EncodeBatch(
            IEnumerable<DataSetWriterMessageModel> messages, int maxMessageSize) {
            if (messages is null) {
                throw new ArgumentNullException(nameof(messages));
            }

            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetMonitoredItemMessages(messages, encodingContext);
            if (!notifications.Any()) {
                yield break;
            }
            var current = notifications.GetEnumerator();
            var processing = current.MoveNext();
            var messageSize = 4; // array length size
            maxMessageSize -= 2048; // reserve 2k for header
            var chunk = new Collection<MonitoredItemMessage>();
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
                        ContentType = ContentMimeType.UaBinary,
                        MessageSchema = MessageSchemaTypes.MonitoredItemMessageBinary
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
        /// Encode single messages
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        public IEnumerable<NetworkMessageModel> Encode(
            IEnumerable<DataSetWriterMessageModel> messages, int maxMessageSize) {
            if (messages is null) {
                throw new ArgumentNullException(nameof(messages));
            }

            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetMonitoredItemMessages(messages, encodingContext);
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
                    ContentType = ContentMimeType.UaBinary,
                    MessageSchema = MessageSchemaTypes.MonitoredItemMessageBinary
                };
                if (encoded.Body.Length > maxMessageSize) {
                    // this message is too large to be processed. Drop it
                    // TODO Trace
                    Interlocked.Increment(ref _notificationsDroppedCount);
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
        /// Produce Monitored Item Messages from the data set message model
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="context"></param>
        private IEnumerable<MonitoredItemMessage> GetMonitoredItemMessages(
            IEnumerable<DataSetWriterMessageModel> messages, ServiceMessageContext context) {
            if (context?.NamespaceUris == null) {
                // declare all notifications in messages dropped
                foreach (var message in messages) {
                    Interlocked.Add(ref _notificationsDroppedCount, message?.Notifications?.Count() ?? 0);
                }
                yield break;
            }
            foreach (var message in messages) {
                foreach (var notification in message.Notifications) {
                    var result = new MonitoredItemMessage {
                        MessageContentMask = (message.Writer?.MessageSettings?
                            .DataSetMessageContentMask).ToMonitoredItemMessageMask(
                                message.Writer?.DataSetFieldContentMask),
                        ApplicationUri = message.ApplicationUri,
                        EndpointUrl = message.EndpointUrl,
                        ExtensionFields = message.Writer?.DataSet?.ExtensionFields,
                        NodeId = notification.NodeId.ToExpandedNodeId(context.NamespaceUris),
                        Timestamp = message.TimeStamp ?? DateTime.UtcNow,
                        Value = notification.Value,
                        DisplayName = notification.DisplayName,
                        SequenceNumber = notification.SequenceNumber.GetValueOrDefault(0)
                    };
                    // force published timestamp into to source timestamp for the legacy heartbeat compatibility
                    if (notification.IsHeartbeat &&
                        ((result.MessageContentMask & (uint)MonitoredItemMessageContentMask.Timestamp) == 0) &&
                        ((result.MessageContentMask & (uint)MonitoredItemMessageContentMask.SourceTimestamp) != 0)) {
                        result.Value.SourceTimestamp = result.Timestamp;
                    }
                    yield return result;
                }
            }
        }
        private long _notificationsDroppedCount;
        private long _notificationsProcessedCount;
        private long _messagesProcessedCount;
    }
}