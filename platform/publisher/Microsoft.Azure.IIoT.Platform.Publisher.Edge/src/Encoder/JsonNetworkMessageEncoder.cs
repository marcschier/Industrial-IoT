// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Models;
    using Microsoft.Azure.IIoT.Platform.Core;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Opc.Ua;
    using Opc.Ua.Encoders;
    using Opc.Ua.Extensions;
    using Opc.Ua.PubSub;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Creates pub/sub encoded messages
    /// </summary>
    public class JsonNetworkMessageEncoder : INetworkMessageEncoder {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.NetworkMessageJson;

        /// <inheritdoc/>
        public long NotificationsDroppedCount { get; private set; }

        /// <inheritdoc/>
        public long NotificationsProcessedCount { get; private set; }

        /// <inheritdoc/>
        public long MessagesProcessedCount { get; private set; }

        /// <inheritdoc/>
        public double AvgNotificationsPerMessage { get; private set; }

        /// <inheritdoc/>
        public double AvgMessageSize { get; private set; }

        /// <inheritdoc/>
        public IList<NetworkMessageModel> EncodeBatch(string writerGroupId,
            IList<DataSetWriterMessageModel> messages,
            string headerLayoutUri, NetworkMessageContentMask? contentMask,
            Publisher.Models.DataSetOrderingType? order, int maxMessageSize) {
            return EncodeBatch(writerGroupId, messages, contentMask, maxMessageSize).ToList();
        }

        /// <inheritdoc/>
        public IList<NetworkMessageModel> Encode(string writerGroupId,
            IList<DataSetWriterMessageModel> messages,
            string headerLayoutUri, NetworkMessageContentMask? contentMask,
            Publisher.Models.DataSetOrderingType? order, int maxMessageSize) {
            return Encode(writerGroupId, messages, contentMask, maxMessageSize).ToList();
        }

        /// <summary>
        /// Encode batching
        /// </summary>
        /// <param name="writerGroupId"></param>
        /// <param name="messages"></param>
        /// <param name="contentMask"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessageModel> EncodeBatch(string writerGroupId,
            IEnumerable<DataSetWriterMessageModel> messages, NetworkMessageContentMask? contentMask,
            int maxMessageSize) {

            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetNetworkMessages(writerGroupId, contentMask, messages, encodingContext);
            if (notifications.Count() == 0) {
                yield break;
            }
            var current = notifications.GetEnumerator();
            var processing = current.MoveNext();
            var messageSize = 2; // array brackets
            maxMessageSize -= 2048; // reserve 2k for header
            var chunk = new Collection<NetworkMessage>();
            while (processing) {
                var notification = current.Current;
                var messageCompleted = false;
                if (notification != null) {
                    var helperWriter = new StringWriter();
                    var helperEncoder = new JsonEncoderEx(helperWriter, encodingContext) {
                        UseAdvancedEncoding = true,
                        UseUriEncoding = true,
                        UseReversibleEncoding = false
                    };
                    notification.Encode(helperEncoder);
                    helperEncoder.Close();
                    var notificationSize = Encoding.UTF8.GetByteCount(helperWriter.ToString());
                    if (notificationSize > maxMessageSize) {
                        // we cannot handle this notification. Drop it.
                        // TODO Trace
                        NotificationsDroppedCount++;
                        processing = current.MoveNext();
                    }
                    else {
                        messageCompleted = maxMessageSize < (messageSize + notificationSize);
                        if (!messageCompleted) {
                            NotificationsProcessedCount++;
                            chunk.Add(notification);
                            processing = current.MoveNext();
                            messageSize += notificationSize + (processing ? 1 : 0);
                        }
                    }
                }
                if (!processing || messageCompleted) {
                    var writer = new StringWriter();
                    var encoder = new JsonEncoderEx(writer, encodingContext,
                        JsonEncoderEx.JsonEncoding.Array) {
                        UseAdvancedEncoding = true,
                        UseUriEncoding = true,
                        UseReversibleEncoding = false
                    };
                    foreach(var element in chunk) {
                        encoder.WriteEncodeable(null, element);
                    }
                    encoder.Close();
                    var encoded = new NetworkMessageModel {
                        Body = Encoding.UTF8.GetBytes(writer.ToString()),
                        ContentEncoding = "utf-8",
                        Timestamp = DateTime.UtcNow,
                        ContentType = ContentMimeType.Json,
                        MessageSchema = MessageSchemaTypes.NetworkMessageJson
                    };
                    AvgMessageSize = ((AvgMessageSize * MessagesProcessedCount) + encoded.Body.Length) /
                        (MessagesProcessedCount + 1);
                    AvgNotificationsPerMessage = ((AvgNotificationsPerMessage * MessagesProcessedCount) +
                        chunk.Count) / (MessagesProcessedCount + 1);
                        MessagesProcessedCount++;
                    chunk.Clear();
                    messageSize = 2;
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
            if (notifications.Count() == 0) {
                yield break;
            }
            foreach (var networkMessage in notifications) {
                var writer = new StringWriter();
                var encoder = new JsonEncoderEx(writer, encodingContext) {
                    UseAdvancedEncoding = true,
                    UseUriEncoding = true,
                    UseReversibleEncoding = false
                };
                networkMessage.Encode(encoder);
                encoder.Close();
                var encoded = new NetworkMessageModel {
                    Body = Encoding.UTF8.GetBytes(writer.ToString()),
                    ContentEncoding = "utf-8",
                    Timestamp = DateTime.UtcNow,
                    ContentType = ContentMimeType.Json,
                    MessageSchema = MessageSchemaTypes.NetworkMessageJson
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
                    Interlocked.Add(ref _notificationsDroppedCount, (message?.Notifications?.Count() ?? 0));
                }
                yield break;
            }

            // TODO: Honor single message
            // TODO: Group by writer
            foreach (var message in messages) {
                var networkMessage = new NetworkMessage {
                    MessageContentMask = contentMask.ToStackType(MessageEncoding.Json),
                    PublisherId = writerGroupId,
                    DataSetClassId = message.Writer?.DataSet?
                        .DataSetMetaData?.DataSetClassId.ToString(),
                    MessageId = message.SequenceNumber.ToString()
                };
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
                    var dataSetMessage = new DataSetMessage {
                        DataSetWriterId = message.Writer.DataSetWriterId,
                        MetaDataVersion = new ConfigurationVersionDataType {
                            MajorVersion = message.Writer?.DataSet?.DataSetMetaData?
                                .ConfigurationVersion?.MajorVersion ?? 1,
                            MinorVersion = message.Writer?.DataSet?.DataSetMetaData?
                                .ConfigurationVersion?.MinorVersion ?? 0
                        },
                        MessageContentMask = (message.Writer?.MessageSettings?.DataSetMessageContentMask)
                            .ToStackType(MessageEncoding.Json),
                        Timestamp = message.TimeStamp ?? DateTime.UtcNow,
                        SequenceNumber = message.SequenceNumber,
                        Status = payload.Values.Any(s => StatusCode.IsNotGood(s.StatusCode)) ?
                            StatusCodes.Bad : StatusCodes.Good,
                        Payload = new DataSet(payload, (uint)message.Writer?.DataSetFieldContentMask.ToStackType())
                    };
                    networkMessage.Messages.Add(dataSetMessage);
                }
                yield return networkMessage;
            }
        }
        private long _notificationsDroppedCount;
        private long _notificationsProcessedCount;
        private long _messagesProcessedCount;
    }
}