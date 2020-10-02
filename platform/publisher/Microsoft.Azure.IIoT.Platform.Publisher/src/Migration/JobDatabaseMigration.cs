// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Migration {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Storage;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// Database job repository migration task
    /// </summary>
    public class JobDatabaseMigration : IMigrationTask {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="database"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="batch"></param>
        public JobDatabaseMigration(IDatabaseServer database, ILogger logger,
            IWriterGroupBatchOperations batch, IItemContainerConfig config = null) {
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _batch = batch ?? throw new ArgumentNullException(nameof(batch));
        }

        /// <inheritdoc/>
        public async Task MigrateAsync() {
            IItemContainer _documents;
            try {
                var dbs = await _database.OpenAsync(_config?.DatabaseName ??
                    "iiot_opc").ConfigureAwait(false);
                _documents = await dbs.OpenContainerAsync(_config?.ContainerName ??
                    "iiot_opc").ConfigureAwait(false);
                var query = _documents.CreateQuery<JobDocument>()
                    .Where(x => x.ClassType == JobDocument.ClassTypeName)
                    .GetResults();
                // Read results
                while (query.HasMore()) {
                    var results = await query.ReadAsync().ConfigureAwait(false);
                    foreach (var document in results) {
                        var group = ToServiceModel(document.Value);
                        if (group != null) {
                            await _batch.ImportWriterGroupAsync(group).ConfigureAwait(false);
                        }
                        // Force delete now
                        await _documents.DeleteAsync<JobDocument>(document.Id).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to open container - not migrating");
                return;
            }
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private WriterGroupModel ToServiceModel(JobDocument value) {
            var model = value.JobConfiguration.Job.WriterGroup;
            if (model == null) {
                return null;
            }
            return new WriterGroupModel {
                Name = model.Name,
                WriterGroupId = model.WriterGroupId,
                Schema = value.JobConfiguration.Job.MessagingMode == MessagingMode.Samples ?
                    MessageSchema.Samples : MessageSchema.PubSub,
                BatchSize = value.JobConfiguration.Job.Engine?.BatchSize,
                PublishingInterval = value.JobConfiguration?.Job?.Engine?.BatchTriggerInterval,
                DataSetWriters = model.DataSetWriters?.Select(w => w.Clone()).ToList(),
                Encoding = model.MessageType == MessageType.Json ?
                    MessageEncoding.Json : MessageEncoding.Uadp,
                MessageSettings = model.MessageSettings.Clone(),
                HeaderLayoutUri = null,
                MaxNetworkMessageSize = null,
                GenerationId = null,
                KeepAliveTime = null,
                State = null,
                LocaleIds = null,
                Priority = null,
                SecurityGroupId = null,
                SecurityKeyServices = null,
                SecurityMode = null,
                SiteId = null // Note null in v1
            };
        }

        /// <summary> Job document </summary>
        [DataContract]
        internal class JobDocument {
            /// <summary> id </summary>
            [DataMember(Name = "id")]
            public string Id { get; set; }
            /// <summary> Etag </summary>
            [DataMember(Name = "_etag")]
            public string ETag { get; set; }
            /// <summary> Document type </summary>
            [DataMember]
            public string ClassType { get; set; } = ClassTypeName;
            /// <summary/>
            public const string ClassTypeName = "Job";
            /// <summary> Identifier of the job document </summary>
            [DataMember]
            public string JobId { get; set; }
            /// <summary> Name </summary>
            [DataMember]
            public string Name { get; set; }
            /// <summary> Configuration type </summary>
            [DataMember]
            public string Type { get; set; }
            /// <summary> Job configuration </summary>
            [DataMember]
            public JobConfigModel JobConfiguration { get; set; }
            /// <summary> Updated at </summary>
            [DataMember]
            public DateTime Updated { get; set; }
            /// <summary> Created at </summary>
            [DataMember]
            public DateTime Created { get; set; }
        }

        /// <summary> Job model </summary>
        [DataContract]
        internal class JobConfigModel {
            /// <summary> Identifier of the job document </summary>
            [DataMember]
            public string JobId { get; set; }
            /// <summary> Job description </summary>
            [DataMember]
            public WriterGroupJobModel Job { get; set; }
        }

        /// <summary> Pub sub writer group job </summary>
        [DataContract]
        internal class WriterGroupJobModel {
            /// <summary> Writer group configuration </summary>
            [DataMember]
            public WriterGroupV1Model WriterGroup { get; set; }
            /// <summary> Injected connection string </summary>
            [DataMember]
            public string ConnectionString { get; set; }
            /// <summary> Messaging mode to use </summary>
            [DataMember]
            public MessagingMode? MessagingMode { get; set; }
            /// <summary>  Engine configuration </summary>
            [DataMember]
            public EngineConfigurationModel Engine { get; set; }
        }

        /// <summary> Message encoding </summary>
        [DataContract]
        internal enum MessageType {
            /// <summary> Ua Json encoding </summary>
            [EnumMember]
            Json,
            /// <summary>Uadp encoding</summary>
            [EnumMember]
            Uadp,
        }

        /// <summary> Writer group model </summary>
        [DataContract]
        internal class WriterGroupV1Model {
            /// <summary> Dataset writer group identifier </summary>
            [DataMember]
            public string WriterGroupId { get; set; }
            /// <summary> Network message types to generate </summary>
            [DataMember]
            public MessageType? MessageType { get; set; }
            /// <summary> The data set writers generating - Not changed </summary>
            [DataMember]
            public List<DataSetWriterModel> DataSetWriters { get; set; }
            /// <summary> Network message configuration - Not changed </summary>
            [DataMember]
            public WriterGroupMessageSettingsModel MessageSettings { get; set; }
            /// <summary> Name of the writer group  </summary>
            [DataMember]
            public string Name { get; set; }
        }

        /// <summary> Message mode </summary>
        [DataContract]
        internal enum MessagingMode {
            /// <summary> Network messages (default) </summary>
            [EnumMember]
            PubSub,
            /// <summary> Monitored item messages </summary>
            [EnumMember]
            Samples
        }

        /// <summary> Engine configuration </summary>
        [DataContract]
        internal class EngineConfigurationModel {
            /// <summary> Batch buffer size </summary>
            [DataMember]
            public int? BatchSize { get; set; }
            /// <summary> Diagnostics setting </summary>
            [DataMember]
            public TimeSpan? BatchTriggerInterval { get; set; }
            /// <summary> IoT Hub Maximum message size </summary>
            [DataMember]
            public int? MaxMessageSize { get; set; }
            /// <summary> Diagnostics setting </summary>
            [DataMember]
            public TimeSpan? DiagnosticsInterval { get; set; }
        }

        private readonly ILogger _logger;
        private readonly IItemContainerConfig _config;
        private readonly IDatabaseServer _database;
        private readonly IWriterGroupBatchOperations _batch;
    }
}