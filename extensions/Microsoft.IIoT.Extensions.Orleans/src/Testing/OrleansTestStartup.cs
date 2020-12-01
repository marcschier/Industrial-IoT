// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Testing {
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using global::Orleans;
    using global::Orleans.Hosting;
    using global::Orleans.TestingHost;

    /// <summary>
    /// Test cluster configuration
    /// </summary>
    public class OrleansTestStartup<TStartup> : IClientBuilderConfigurator,
        ISiloConfigurator, IHostConfigurator where TStartup : OrleansStartup, new() {

        /// <inheritdoc/>
        public void Configure(IConfiguration configuration,
            IClientBuilder clientBuilder) {
            _startup.ConfigureClient(clientBuilder);
        }

        /// <inheritdoc/>
        public void Configure(ISiloBuilder siloBuilder) {
            siloBuilder
                .UseInMemoryReminderService()
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("MemoryStore");
            _startup.ConfigureSilo(siloBuilder);
        }

        /// <inheritdoc/>
        public void Configure(IHostBuilder hostBuilder) {
            _startup.ConfigureHost(hostBuilder);
        }

        private readonly TStartup _startup = new TStartup();
    }
}