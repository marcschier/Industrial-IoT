// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Identity.Services {
    using Microsoft.IIoT.Platform.Identity.Models;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Configures the root user in the user database
    /// </summary>
    public class UserManagerStorageInit : IHostProcess {

        /// <summary>
        /// Create configuration process
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public UserManagerStorageInit(UserManager<UserModel> manager,
            IOptions<RootUserOptions> options, ILogger logger) {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            if (string.IsNullOrEmpty(_options.Value.UserName)) {
                _logger.LogDebug("Skipping root user configuration.");
                return;
            }
            var rootUser = new UserModel {
                Id = _options.Value.UserName
            };
            try {
                var exists = await _manager.FindByIdAsync(_options.Value.UserName).ConfigureAwait(false);
                if (exists != null) {
                    return;
                }

                if (string.IsNullOrEmpty(_options.Value.Password)) {
                    await _manager.CreateAsync(rootUser).ConfigureAwait(false);
                }
                else {
                    await _manager.CreateAsync(rootUser, _options.Value.Password).ConfigureAwait(false);
                }
                _logger.LogInformation("Root user {user} added", _options.Value.UserName);
            }
            catch (ResourceConflictException) { }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to add root user");
            }
        }

        /// <inheritdoc/>
        public Task StopAsync() {
            return Task.CompletedTask;
        }

        private readonly UserManager<UserModel> _manager;
        private readonly IOptions<RootUserOptions> _options;
        private readonly ILogger _logger;
    }
}