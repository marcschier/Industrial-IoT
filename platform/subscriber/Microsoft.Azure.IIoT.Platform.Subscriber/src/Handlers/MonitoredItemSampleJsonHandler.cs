// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Subscriber.Handlers {
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.Subscriber;
    using Microsoft.Azure.IIoT.Platform.Subscriber.Models;
    using Opc.Ua;
    using Opc.Ua.Extensions;
    using Opc.Ua.PubSub;
    using Opc.Ua.Encoders;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher message handling
    /// </summary>
    public sealed class MonitoredItemSampleJsonHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.MonitoredItemMessageJson;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="encoder"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public MonitoredItemSampleJsonHandler(IVariantEncoderFactory encoder,
            IEnumerable<ISubscriberMessageProcessor> handlers, ILogger logger) {
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
                while (decoder.ReadEncodeable(null, typeof(MonitoredItemMessage))
                     is MonitoredItemMessage message) {
                    var type = BuiltInType.Null;
                    var codec = _encoder.Create(context);
                    var sample = new MonitoredItemMessageModel {
                        PublisherId = (message.ExtensionFields != null &&
                            message.ExtensionFields.TryGetValue("PublisherId", out var publisherId))
                                ? publisherId : message.ApplicationUri ?? message.EndpointUrl,
                        DataSetWriterId = (message.ExtensionFields != null &&
                            message.ExtensionFields.TryGetValue("DataSetWriterId", out var dataSetWriterId))
                                ? dataSetWriterId : message.EndpointUrl ?? message.ApplicationUri,
                        NodeId = message.NodeId.AsString(context),
                        DisplayName = message.DisplayName,
                        Timestamp = message.Timestamp,
                        SequenceNumber = message.SequenceNumber,
                        Value = message?.Value == null
                                    ? null : codec.Encode(message.Value.WrappedValue, out type),
                        DataType = type == BuiltInType.Null
                                    ? null : type.ToString(),
                        Status = (message?.Value?.StatusCode.Code == StatusCodes.Good)
                            ? null : StatusCode.LookupSymbolicId(message.Value.StatusCode.Code),
                        SourceTimestamp = (message?.Value?.SourceTimestamp == DateTime.MinValue)
                            ? null : message?.Value?.SourceTimestamp,
                        SourcePicoseconds = (message?.Value?.SourcePicoseconds == 0)
                            ? null : message?.Value?.SourcePicoseconds,
                        ServerTimestamp = (message?.Value?.ServerTimestamp == DateTime.MinValue)
                            ? null : message?.Value?.ServerTimestamp,
                        ServerPicoseconds = (message?.Value?.ServerPicoseconds == 0)
                            ? null : message?.Value?.ServerPicoseconds,
                        VariableId = (message.ExtensionFields != null &&
                            message.ExtensionFields.TryGetValue("EndpointId", out var endpointId))
                                ? endpointId : message.ApplicationUri ?? message.EndpointUrl,
                    };
                    await Task.WhenAll(_handlers.Select(h => h.HandleSampleAsync(sample))).ConfigureAwait(false);
                }
            }
            catch (Exception ex) {
                _logger.Error(ex,"Publishing messages failed - skip");
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly IVariantEncoderFactory _encoder;
        private readonly ILogger _logger;
        private readonly List<ISubscriberMessageProcessor> _handlers;
    }
}
