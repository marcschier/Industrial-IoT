// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Handlers {
    using Microsoft.IIoT.Platform.Publisher;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.OpcUa;
    using Microsoft.IIoT.Messaging;
    using Opc.Ua;
    using Opc.Ua.PubSub;
    using Opc.Ua.Encoders;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher message handling
    /// </summary>
    public sealed class NetworkMessageJsonHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.NetworkMessageJson;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="encoder"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public NetworkMessageJsonHandler(IVariantEncoderFactory encoder,
            IEnumerable<IDataSetWriterMessageProcessor> handlers, ILogger logger) {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlers = handlers?.ToList() ?? throw new ArgumentNullException(nameof(handlers));
        }

        /// <inheritdoc/>
        public async Task HandleAsync(string source, byte[] payload,
            IDictionary<string, string> properties, Func<Task> checkpoint) {

            try {
                var context = new ServiceMessageContext();
                using var decoder = new JsonDecoderEx(new MemoryStream(payload), context);
                while (decoder.ReadEncodeable(null, typeof(NetworkMessage)) is NetworkMessage message) {
                    foreach (var dataSetMessage in message.Messages) {
                        var dataset = new PublishedDataSetItemMessageModel {
                            DataSetWriterId = dataSetMessage.DataSetWriterId,
                            SequenceNumber = dataSetMessage.SequenceNumber,
                            Extensions = new Dictionary<string, string> {
                                ["MessageId"] = message.MessageId,
                                ["PublisherId"] = message.PublisherId,
                                ["Status"] = StatusCode.LookupSymbolicId(dataSetMessage.Status.Code),
                                ["MetaDataVersion"] = dataSetMessage.MetaDataVersion.MajorVersion +
                                    "." + dataSetMessage.MetaDataVersion.MinorVersion,
                            },
                            Timestamp = dataSetMessage.Timestamp,
                        };
                        foreach (var datapoint in dataSetMessage.Payload) {
                            var codec = _encoder.Create(context);
                            var type = BuiltInType.Null;
                            var msg = dataset; // TODO: .Clone();
                            msg.VariableId = datapoint.Key;
                            msg.Value = new DataValueModel {
                                Value = datapoint.Value == null
                                    ? null : codec.Encode(datapoint.Value.WrappedValue, out type),
                                DataType = type == BuiltInType.Null
                                    ? null : type.ToString(),
                                Status = (datapoint.Value?.StatusCode.Code == StatusCodes.Good)
                                    ? null : StatusCode.LookupSymbolicId(datapoint.Value.StatusCode.Code),
                                SourceTimestamp = (datapoint.Value?.SourceTimestamp == DateTime.MinValue)
                                    ? null : datapoint.Value?.SourceTimestamp,
                                SourcePicoseconds = (datapoint.Value?.SourcePicoseconds == 0)
                                    ? null : datapoint.Value?.SourcePicoseconds,
                                ServerTimestamp = (datapoint.Value?.ServerTimestamp == DateTime.MinValue)
                                    ? null : datapoint.Value?.ServerTimestamp,
                                ServerPicoseconds = (datapoint.Value?.ServerPicoseconds == 0)
                                    ? null : datapoint.Value?.ServerPicoseconds
                            };
                            await Task.WhenAll(_handlers
                                .Select(h => h.HandleMessageAsync(msg))).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Subscriber json network message handling failed - skip");
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly IVariantEncoderFactory _encoder;
        private readonly ILogger _logger;
        private readonly List<IDataSetWriterMessageProcessor> _handlers;
    }
}
