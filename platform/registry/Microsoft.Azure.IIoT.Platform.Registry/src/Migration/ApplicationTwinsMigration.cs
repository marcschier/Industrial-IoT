// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Migration {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Linq;

    /// <summary>
    /// Migrate from device twin repo to defined repo
    /// </summary>
    public sealed class ApplicationTwinsMigration : IMigrationTask {

        /// <summary>
        /// Create migrator
        /// </summary>
        /// <param name="source"></param>
        /// <param name="repo"></param>
        /// <param name="logger"></param>
        public ApplicationTwinsMigration(IDeviceTwinServices source,
            IApplicationRepository repo, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <inheritdoc/>
        public async Task MigrateAsync() {
            string continuation = null;
            do {
                var results = await ListAsync(continuation);
                continuation = results.ContinuationToken;
                foreach (var application in results.Items) {
                    try {
                        var clone = application.Clone();
                        clone.ApplicationId =
                            ApplicationInfoModelEx.CreateApplicationId(application);
                        await _repo.AddAsync(clone);
                    }
                    catch (ResourceConflictException ex) {
                        _logger.Error(ex,
                            "Application {application} already exists - not migrating...",
                            application.ApplicationName);
                        continue;
                    }
                    catch (Exception e) {
                        _logger.Error(e, "Error adding {application} - skip migration...",
                            application.ApplicationName);
                        continue;
                    }
                    // Force delete now
                    await _source.DeleteAsync(application.ApplicationId);
                }
            }
            while (continuation != null);
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoListModel> ListAsync(
            string continuation, int? pageSize = null, CancellationToken ct = default) {
            var query = "SELECT * FROM devices WHERE " +
                $"tags.{nameof(EntityRegistration.DeviceType)} = '{IdentityType.Application}' ";
            var result = await _source.QueryDeviceTwinsAsync(query, continuation, pageSize, ct);
            return new ApplicationInfoListModel {
                ContinuationToken = result.ContinuationToken,
                Items = result.Items
                    .Select(t => t.ToApplicationRegistration())
                    .Select(s => s.ToServiceModel())
                    .ToList()
            };
        }

        private readonly ILogger _logger;
        private readonly IDeviceTwinServices _source;
        private readonly IApplicationRepository _repo;
    }
}
