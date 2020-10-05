// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Opc.Ua;
    using Opc.Ua.Encoders;
    using Opc.Ua.Extensions;
    using Opc.Ua.PubSub;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Publisher monitored item message encoder
    /// </summary>
    public class JsonSampleMessageEncoder : INetworkMessageEncoder {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.MonitoredItemMessageJson;

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
        /// Encode single message
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessageModel> Encode(IEnumerable<DataSetWriterMessageModel> messages,
            int maxMessageSize) {

            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetMonitoredItemMessages(messages, encodingContext);
            if (!notifications.Any()) {
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
                    ContentType = ContentMimeType.UaLegacyPublisher,
                    MessageSchema = MessageSchemaTypes.MonitoredItemMessageJson
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
        /// Perform batch encoding
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="maxMessageSize"></param>
        /// <returns></returns>
        private IEnumerable<NetworkMessageModel> EncodeBatch(
            IEnumerable<DataSetWriterMessageModel> messages, int maxMessageSize) {
            maxMessageSize -= 2048; // reserve 2k for message properties - TODO - move to outer.

            // by design all messages are generated in the same session context,
            // therefore it is safe to get the first message's context
            var encodingContext = messages.FirstOrDefault(m => m.ServiceMessageContext != null)
                ?.ServiceMessageContext;
            var notifications = GetMonitoredItemMessages(messages, encodingContext);
            if (!notifications.Any()) {
                yield break;
            }

            // Create individual value encoding strings
            var jsonChunks = notifications
                .Select(notification => {
                    using (var helperWriter = new StringWriter()) {
                        using (var helperEncoder = new JsonEncoderEx(helperWriter, encodingContext) {
                            UseAdvancedEncoding = true,
                            UseUriEncoding = true,
                            UseReversibleEncoding = false
                        }) {
                            notification.Encode(helperEncoder);
                        }
                        return helperWriter.ToString();
                    }
                });

            var messageSize = 1; // array brackets
            var values = 0;
            var content = new StringBuilder("[");
            foreach (var json in jsonChunks) {
                var chunkSizeInBytes = Encoding.UTF8.GetByteCount(json);
                if (chunkSizeInBytes + 2 >= maxMessageSize) {
                    // we cannot fit this value in no matter what. Drop it.
                    // TODO Trace
                    Interlocked.Increment(ref _notificationsDroppedCount);
                    continue;
                }

                if (messageSize + chunkSizeInBytes + 1 >= maxMessageSize) {
                    yield return CreateNetworkMessage(values, content);
                    content.Clear();
                    content.Append('[');
                    messageSize = 1;
                    values = 0;
                }

                // Append body
                content.Append(json);
                messageSize += chunkSizeInBytes;
                content.Append(',');
                messageSize++;
                values++;
                Interlocked.Increment(ref _notificationsProcessedCount);
            }

            if (values > 0) {
                // Write remaining values
                yield return CreateNetworkMessage(values, content);
            }

            // Helper to write out content of the writer
            NetworkMessageModel CreateNetworkMessage(int values, StringBuilder content) {
                // Write out existing buffer
                var body = content.Replace(',', ']', content.Length - 1, 1).ToString();
                //  System.Diagnostics.Debug.Assert(Newtonsoft.Json.JsonConvert.DeserializeObject(body) != null);
                var encoded = new NetworkMessageModel {
                    Body = Encoding.UTF8.GetBytes(body),
                    ContentEncoding = "utf-8",
                    Timestamp = DateTime.UtcNow,
                    ContentType = ContentMimeType.UaJson,
                    MessageSchema = MessageSchemaTypes.MonitoredItemMessageJson
                };
                System.Diagnostics.Debug.Assert(encoded.Body.Length <= maxMessageSize);
                AvgMessageSize = ((AvgMessageSize * MessagesProcessedCount) + encoded.Body.Length) /
                    (MessagesProcessedCount + 1);
                AvgNotificationsPerMessage = ((AvgNotificationsPerMessage * MessagesProcessedCount) +
                    values) / (MessagesProcessedCount + 1);
                Interlocked.Increment(ref _messagesProcessedCount);
                return encoded;
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