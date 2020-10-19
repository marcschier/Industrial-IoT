// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Services {
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// Receive updates to writer group registry and update the engine as result.
    /// This encapsulates the work that happens between Publisher Registry, IoT Hub
    /// and Edge module, where the notifications change the twin state and cause
    /// the action here.
    /// </summary>
    public class SimpleWriterGroupEngineHost : IWriterGroupRegistryListener,
        IDataSetWriterRegistryListener {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="sourceFactory"></param>
        /// <param name="sinkFactory"></param>
        /// <param name="writerGroupEvents"></param>
        /// <param name="dataSetWriterEvents"></param>
        public SimpleWriterGroupEngineHost(IDataSetWriterRegistry registry,
            Func<IWriterGroupDataSource> sourceFactory, Func<IWriterGroupDataSink> sinkFactory,
            IPublisherEvents<IWriterGroupRegistryListener> writerGroupEvents,
            IPublisherEvents<IDataSetWriterRegistryListener> dataSetWriterEvents) {

            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _sourceFactory = sourceFactory ?? throw new ArgumentNullException(nameof(sourceFactory));
            _sinkFactory = sinkFactory ?? throw new ArgumentNullException(nameof(sinkFactory));

            if (writerGroupEvents is null) {
                throw new ArgumentNullException(nameof(writerGroupEvents));
            }
            if (dataSetWriterEvents is null) {
                throw new ArgumentNullException(nameof(dataSetWriterEvents));
            }

            writerGroupEvents.Register(this);
            dataSetWriterEvents.Register(this);
        }

        /// <inheritdoc/>
        public async Task OnDataSetWriterAddedAsync(PublisherOperationContextModel context,
            DataSetWriterInfoModel dataSetWriter) {
            if (_engines.TryGetValue(dataSetWriter.WriterGroupId, out var group)) {

                // Obtain real writer from registry and add
                var writer = await _registry.GetDataSetWriterAsync(
                    dataSetWriter.DataSetWriterId).ConfigureAwait(false);
                group.AddWriter(writer);
            }
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterRemovedAsync(PublisherOperationContextModel context,
            DataSetWriterInfoModel dataSetWriter) {
            if (_engines.TryGetValue(dataSetWriter.WriterGroupId, out var group)) {

                // Remove writer from activated group
                group.RemoveWriter(dataSetWriter.DataSetWriterId);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterStateChangeAsync(PublisherOperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter) {
            // No op
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnDataSetWriterUpdatedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter) {
            // Same as what the edge module does remotely
            var writer = await _registry.GetDataSetWriterAsync(dataSetWriterId).ConfigureAwait(false);
            foreach (var group in _engines.Values
                .Where(v => v.Writers.Any(w => w.DataSetWriterId == dataSetWriterId))) {
                group.AddWriter(writer);
            }
        }

        /// <inheritdoc/>
        public Task OnWriterGroupAddedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            _engines.TryAdd(writerGroup.WriterGroupId, new WriterGroupEngine {
                Group = writerGroup
            });
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnWriterGroupUpdatedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (_engines.TryGetValue(writerGroup.WriterGroupId, out var group)) {
                group.Group = writerGroup;
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnWriterGroupActivatedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (_engines.TryGetValue(writerGroup.WriterGroupId, out var group)) {
                group.Connect(_sourceFactory.Invoke(), _sinkFactory.Invoke());
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnWriterGroupDeactivatedAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            if (_engines.TryGetValue(writerGroup.WriterGroupId, out var group)) {
                group.Disconnect();
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnWriterGroupRemovedAsync(PublisherOperationContextModel context,
            string writerGroupId) {
            _engines.TryRemove(writerGroupId, out _);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnWriterGroupStateChangeAsync(PublisherOperationContextModel context,
            WriterGroupInfoModel writerGroup) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Data plane engine.  Collects the content of writer group 
        /// information and applies it to a new source instance on 
        /// activation. Kills the writer group data source on deactivation.
        /// </summary>
        private class WriterGroupEngine {

            /// <summary>
            /// Group model clone
            /// </summary>
            public WriterGroupInfoModel Group {
                get => _group;
                set {
                    _group = value.Clone();
                    ApplyUpdate();
                }
            }

            /// <summary>
            /// Current set of writers
            /// </summary>
            internal HashSet<DataSetWriterModel> Writers { get; }

            /// <summary>
            /// Create engine
            /// </summary>
            public WriterGroupEngine() {
                Writers = new HashSet<DataSetWriterModel>(Compare.Using<DataSetWriterModel>(
                    (a, b) => a.DataSetWriterId == b.DataSetWriterId));
            }

            /// <summary>
            /// Connect a writer group source to a sink on activation
            /// </summary>
            /// <param name="source"></param>
            /// <param name="sink"></param>
            public void Connect(IWriterGroupDataSource source, IWriterGroupDataSink sink) {
                _source = source;
                _sink = sink;
                ApplyUpdate();
                _source.AddWriters(Writers);
            }

            /// <summary>
            /// Disconnect sink from source
            /// </summary>
            public void Disconnect() {
                // _engine.RemoveAllWriters();
                (_sink as IDisposable).Dispose();
                (_source as IDisposable).Dispose();
                _source = null;
                _sink = null;
            }

            /// <summary>
            /// Add writer 
            /// </summary>
            /// <param name="writer"></param>
            public void AddWriter(DataSetWriterModel writer) {
                Writers.Remove(writer); // Remove and add to update
                Writers.Add(writer);
                if (_source != null) {
                    _source.AddWriters(writer.YieldReturn());
                }
            }

            /// <summary>
            /// Remove writer from engine
            /// </summary>
            /// <param name="dataSetWriterId"></param>
            public void RemoveWriter(string dataSetWriterId) {
                if (_source != null) {
                    _source.RemoveWriters(dataSetWriterId.YieldReturn());
                }
                Writers.RemoveWhere(w => w.DataSetWriterId == dataSetWriterId);
            }

            /// <summary>
            /// Perform update of state on the engine
            /// </summary>
            private void ApplyUpdate() {
                if (_source == null) {
                    return;
                }

                // Apply now
                _sink.WriterGroupId = _group.WriterGroupId;
                _sink.MaxNetworkMessageSize = _group.MaxNetworkMessageSize;
                _sink.BatchSize = _group.BatchSize;
                _sink.PublishingInterval = _group.PublishingInterval;
                _sink.Encoding = _group.Encoding;
                _sink.Schema = _group.Schema;
                _sink.HeaderLayoutUri = _group.HeaderLayoutUri;
                _sink.DataSetOrdering = _group.MessageSettings?.DataSetOrdering;
                _sink.MessageContentMask = _group.MessageSettings?.NetworkMessageContentMask;
                _sink.PublishingOffset = _group.MessageSettings?.PublishingOffset?.ToList();

                if (_source is SimpleWriterGroupDataSource s) {
                    // If simple, make sure we set writer group id
                    s.WriterGroupId = _group.WriterGroupId;
                }

                _source.Priority = _group.Priority;
                _source.GroupVersion = _group.MessageSettings?.GroupVersion;
                _source.KeepAliveTime = _group.KeepAliveTime;
                _source.SamplingOffset = _group.MessageSettings?.SamplingOffset;
            }

            private IWriterGroupDataSource _source;
            private IWriterGroupDataSink _sink;
            private WriterGroupInfoModel _group;
        }

        private readonly ConcurrentDictionary<string, WriterGroupEngine> _engines =
            new ConcurrentDictionary<string, WriterGroupEngine>();
        private readonly IDataSetWriterRegistry _registry;
        private readonly Func<IWriterGroupDataSource> _sourceFactory;
        private readonly Func<IWriterGroupDataSink> _sinkFactory;
    }
}
