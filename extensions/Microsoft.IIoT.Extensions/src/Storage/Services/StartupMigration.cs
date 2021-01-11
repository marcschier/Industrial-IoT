﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Storage.Services {
    using Autofac;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Run migrations on startup
    /// </summary>
    public sealed class StartupMigration : IStartable {

        /// <summary>
        /// Startup migration
        /// </summary>
        /// <param name="migrations"></param>
        /// <param name="logger"></param>
        public StartupMigration(IEnumerable<IMigrationTask> migrations, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _migrations = migrations?.ToList() ?? new List<IMigrationTask>();
        }

        /// <inheritdoc/>
        public void Start() {
            try {
                Task.WaitAll(_migrations.Select(m => m.MigrateAsync()).ToArray());
                _logger.LogInformation("Startup migration completed.");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to migrate");
            }
        }

        private readonly ILogger _logger;
        private readonly List<IMigrationTask> _migrations;
    }
}
