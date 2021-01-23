// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Services {
    using Microsoft.IIoT.Protocols.OpcUa;
    using Microsoft.IIoT.Protocols.OpcUa.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.Extensions.Logging;
    using Opc.Ua.Encoders;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Create an archive of the address space and historic values.
    /// </summary>
    public sealed class AddressSpaceArchiver : IDisposable {

        /// <summary>
        /// Create archiver
        /// </summary>
        /// <param name="client"></param>
        /// <param name="connection"></param>
        /// <param name="archive"></param>
        /// <param name="contentType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="maxValues"></param>
        /// <param name="logger"></param>
        public AddressSpaceArchiver(IConnectionServices client, ConnectionModel connection,
            IArchive archive, string contentType, DateTime? startTime, DateTime? endTime,
            int? maxValues, ILogger logger) {

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _archive = archive ?? throw new ArgumentNullException(nameof(connection));

            _contentType = contentType ?? ContentMimeType.UaJson;
            _startTime = startTime ?? DateTime.UtcNow.AddDays(-1);
            _endTime = endTime ?? DateTime.UtcNow;
            _maxValues = maxValues ?? short.MaxValue;
        }

        /// <summary>
        /// Create archiver
        /// </summary>
        /// <param name="client"></param>
        /// <param name="connection"></param>
        /// <param name="archive"></param>
        /// <param name="logger"></param>
        public AddressSpaceArchiver(IConnectionServices client, ConnectionModel connection,
            IArchive archive, ILogger logger) :
            this(client, connection, archive, null, null, null, null, logger) {
        }

        /// <inheritdoc/>
        public void Dispose() {
            _archive.Dispose();
        }

        /// <inheritdoc/>
        public async Task ArchiveAsync(CancellationToken ct) {
            var diagnostics = new List<OperationResultModel>();

            // Write manifest

            // Write nodes
            IEnumerable<string> historyNodes = null;
            using (var stream = _archive.GetStream("_nodes", FileMode.CreateNew))
            using (var encoder = new BrowsedNodeStreamEncoder(_client, _connection, stream,
                _contentType, null, _logger)) {
                await encoder.EncodeAsync(ct).ConfigureAwait(false);

                historyNodes = encoder.HistoryNodes;
                diagnostics.AddRange(encoder.Diagnostics);
            }

            if (historyNodes != null) {
                foreach (var nodeId in historyNodes) {
                    using (var stream = _archive.GetStream("_history/" + nodeId,
                        FileMode.CreateNew))
                    using (var encoder = new HistoricValueStreamEncoder(_client, _connection,
                        stream, _contentType, nodeId, _logger,
                        _startTime, _endTime, _maxValues)) {
                        await encoder.EncodeAsync(ct).ConfigureAwait(false);
                        diagnostics.AddRange(encoder.Diagnostics);
                    }
                }
            }

            using (var stream = _archive.GetStream("_diagnostics", FileMode.CreateNew))
            using (var encoder = new ModelEncoder(stream, _contentType)) {
                foreach (var operation in diagnostics) {
                    encoder.WriteEncodeable(null, operation, operation.GetType());
                }
            }
        }

        private readonly IConnectionServices _client;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly int _maxValues;
        private readonly IArchive _archive;
        private readonly string _contentType;
        private readonly ConnectionModel _connection;
        private readonly ILogger _logger;
    }
}
