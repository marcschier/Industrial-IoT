// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Storage.Services {
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Injectable collection factory
    /// </summary>
    public class CollectionFactory : ICollectionFactory {

        /// <summary>
        /// Create container factory
        /// </summary>
        /// <param name="server"></param>
        /// <param name="options"></param>
        public CollectionFactory(IDatabaseServer server,
            IOptionsSnapshot<CollectionFactoryOptions> options) {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public async Task<IDocumentCollection> OpenAsync(string name) {
            var option = string.IsNullOrEmpty(name) ?
                _options.Value : _options.Get(name);
            var database = await _server.OpenAsync(
                option.DatabaseName ?? name).ConfigureAwait(false);
            return await database.OpenContainerAsync(
                option.ContainerName ?? name, option).ConfigureAwait(false);
        }

        private readonly IDatabaseServer _server;
        private readonly IOptionsSnapshot<CollectionFactoryOptions> _options;
    }
}
