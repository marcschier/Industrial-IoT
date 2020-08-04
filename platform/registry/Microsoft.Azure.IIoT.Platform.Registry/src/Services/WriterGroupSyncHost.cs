// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Serilog;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Synchronize Publisher registry twins with writer groups and and writers
    /// </summary>
    public class WriterGroupSyncHost : AbstractRunHost {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="writers"></param>
        /// <param name="publisher"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public WriterGroupSyncHost(IWriterGroupRegistry groups, IDataSetWriterRegistry writers,
            IWriterGroupSync publisher, ILogger logger, IWriterGroupSyncConfig config = null) :
            base(logger, "Publisher registry synchronization",
                config?.WriterGroupSyncInterval ?? TimeSpan.FromHours(6), TimeSpan.FromMinutes(5)) {

            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken ct) {
            _logger.Information("Synchronizing publisher registry with databases...");
            string continuation = null;
            do {
                var result = await _groups.ListWriterGroupsAsync(continuation, null, ct);
                continuation = result.ContinuationToken;
                foreach (var group in result.WriterGroups) {

                    // Get all writers in the group
                    var writers = await _writers.QueryAllDataSetWritersAsync(
                        new DataSetWriterInfoQueryModel {
                            WriterGroupId = group.WriterGroupId
                        }, ct);

                    _logger.Information("Synchronizing writer group {group}...",
                        group.WriterGroupId);
                    await _publisher.SynchronizeWriterGroupAsync(group, writers, ct);
                    _logger.Information("Writer group {group} synchronized.",
                        group.WriterGroupId);
                }
            }
            while (!string.IsNullOrEmpty(continuation));
            _logger.Information("Publisher device registry synchronized with databases.");
        }

        private readonly IWriterGroupSync _publisher;
        private readonly IWriterGroupRegistry _groups;
        private readonly IDataSetWriterRegistry _writers;
        private readonly ILogger _logger;
    }
}