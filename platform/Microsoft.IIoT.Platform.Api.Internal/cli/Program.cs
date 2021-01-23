// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Api.Cli {
    using Microsoft.IIoT.Platform.Api.Runtime;
    using Microsoft.IIoT.Platform.Registry.Api;
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using Microsoft.IIoT.Platform.Vault.Api;
    using Microsoft.IIoT.Platform.Vault.Api.Clients;
    using Microsoft.IIoT.Platform.Vault.Api.Models;
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Http.SignalR.Clients;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Authentication.Runtime;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Prometheus;

    /// <summary>
    /// Api command line interface
    /// </summary>
    public sealed class Program : IDisposable {

        /// <summary>
        /// Configure Dependency injection
        /// </summary>
        public static IContainer ConfigureContainer(
            IConfiguration configuration, bool useMsgPack) {
            var builder = new ContainerBuilder();

            builder.AddConfiguration(configuration);
            builder.RegisterType<ApiConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<AadApiClientConfig>()
                .AsImplementedInterfaces();

            // Register logger
            builder.AddDiagnostics(builder => builder.AddDebug());
            builder.RegisterModule<NewtonSoftJsonModule>();
            if (useMsgPack) {
                builder.RegisterModule<MessagePackModule>();
            }

            // Register http client module ...
            builder.RegisterModule<HttpClientModule>();
            // ... as well as signalR client (needed for api)
            builder.RegisterType<SignalRHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // Use bearer authentication
            builder.RegisterModule<NativeClientAuthentication>();

            builder.RegisterType<VaultServiceClient>()
                .AsImplementedInterfaces();

            // ... with client event callbacks
            builder.RegisterType<RegistryServiceEvents>()
                .AsImplementedInterfaces().SingleInstance();

            return builder.Build();
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args) {

            // Load hosting configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddFromKeyVault(allowInteractiveLogon: true)
                .Build();

            using (var scope = new Program(config,
                args.Any(arg => arg.EqualsIgnoreCase("--useMsgPack")))) {
                scope.RunAsync(args).Wait();
            }
        }


        /// <summary>
        /// Configure Dependency injection
        /// </summary>
        public Program(IConfiguration configuration, bool useMsgPack) {
            var container = ConfigureContainer(configuration, useMsgPack);
            _scope = container.BeginLifetimeScope();
            _registry = _scope.Resolve<IRegistryServiceApi>();
            _vault = _scope.Resolve<IVaultServiceApi>();
            _serializer = _scope.Resolve<IJsonSerializer>();
            if (_scope.TryResolve(out _metrics)) {
                _metrics.Start();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _metrics?.Stop();
            _scope.Dispose();
        }

        /// <summary>
        /// Run cli
        /// </summary>
        public async Task RunAsync(string[] args) {
            if (args is null) {
                throw new ArgumentNullException(nameof(args));
            }

            var interactive = false;
            do {
                if (interactive) {
                    Console.Write("> ");
                    args = CliOptions.ParseAsCommandLine(Console.ReadLine());
                }
                try {
                    if (args.Length < 1) {
                        throw new ArgumentException("Need a command!");
                    }

                    CliOptions options;
                    var command = args[0];
                    switch (command) {
                        case "exit":
                            interactive = false;
                            break;
                        case "console":
                            Console.WriteLine(@"
  ___               _                 _            _           _           ___           _____
 |_ _|  _ __     __| |  _   _   ___  | |_   _ __  (_)   __ _  | |         |_ _|   ___   |_   _|
  | |  | '_ \   / _` | | | | | / __| | __| | '__| | |  / _` | | |  _____   | |   / _ \    | |
  | |  | | | | | (_| | | |_| | \__ \ | |_  | |    | | | (_| | | | |_____|  | |  | (_) |   | |
 |___| |_| |_|  \__,_|  \__,_| |___/  \__| |_|    |_|  \__,_| |_|         |___|  \___/    |_|
");
                            interactive = true;
                            break;
                        case "status":
                            options = new CliOptions(args);
                            await GetStatusAsync().ConfigureAwait(false);
                            break;
                        case "monitor":
                            options = new CliOptions(args);
                            await MonitorAllAsync().ConfigureAwait(false);
                            break;
                        case "certificates":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "root":
                                    await CreateRootCertificateAsync(options).ConfigureAwait(false);
                                    break;
                                case "child":
                                    await CreateCertificateGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateCertificateGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "delete":
                                    await DeleteCertificateGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListCertificateGroupsAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectCertificateGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetCertificateGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "renew":
                                    await RenewIssuerCertAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintCertificateGroupsHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "requests":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "sign":
                                    await SigningRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "keypair":
                                    await KeyPairRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "approve":
                                    await ApproveRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "reject":
                                    await RejectRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "accept":
                                    await AcceptRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "delete":
                                    await DeleteRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListRequestsAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetRequestAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryRequestsAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintRequestsHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "trust":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "create":
                                    await AddTrustRelationshipAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetTrustedCertificatesAsync(options).ConfigureAwait(false);
                                    break;
                                case "delete":
                                    await RemoveTrustRelationshipAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintTrustHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "discoverers":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "get":
                                    await GetDiscovererAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorDiscoverersAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListDiscoverersAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectDiscovererAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryDiscoverersAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintDiscoverersHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "supervisors":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "get":
                                    await GetSupervisorAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorSupervisorsAsync().ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListSupervisorsAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectSupervisorAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QuerySupervisorsAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintSupervisorsHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "publishers":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "get":
                                    await GetPublisherAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdatePublisherAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorPublishersAsync().ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListPublishersAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectPublisherAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryPublishersAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintPublishersHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "gateways":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "get":
                                    await GetGatewayAsync(options).ConfigureAwait(false);
                                    break;
                                case "sites":
                                    await ListSitesAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateGatewayAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorGatewaysAsync().ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListGatewaysAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectGatewayAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryGatewaysAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintGatewaysHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;

                        case "-?":
                        case "-h":
                        case "--help":
                        case "help":
                            PrintHelp();
                            break;
                        default:
                            throw new ArgumentException($"Unknown command {command}.");
                    }
                }
                catch (ArgumentException e) {
                    Console.WriteLine(e.Message);
                    if (!interactive) {
                        PrintHelp();
                        return;
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("==================");
                    Console.WriteLine(e);
                    Console.WriteLine("==================");
                }
            }
            while (interactive);
        }
        private string _publisherId;

        /// <summary>
        /// Get publisher id
        /// </summary>
        private string GetPublisherId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_publisherId != null) {
                if (id == null) {
                    return _publisherId;
                }
                _publisherId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -i/--id option.");
        }

        /// <summary>
        /// Select publisher registration
        /// </summary>
        private async Task SelectPublisherAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _publisherId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_publisherId);
            }
            else {
                var publisherId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(publisherId)) {
                    var result = await _registry.ListAllPublishersAsync().ConfigureAwait(false);
                    publisherId = ConsoleEx.Select(result.Select(r => r.Id));
                    if (string.IsNullOrEmpty(publisherId)) {
                        Console.WriteLine("Nothing selected - publisher selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {publisherId}.");
                    }
                }
                _publisherId = publisherId;
            }
        }

        /// <summary>
        /// List publisher registrations
        /// </summary>
        private async Task ListPublishersAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllPublishersAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListPublishersAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query publisher registrations
        /// </summary>
        private async Task QueryPublishersAsync(CliOptions options) {
            var query = new PublisherQueryApiModel {
                Connected = options.IsProvidedOrNull("-c", "--connected"),
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.QueryAllPublishersAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.QueryPublishersAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get publisher
        /// </summary>
        private async Task GetPublisherAsync(CliOptions options) {
            var result = await _registry.GetPublisherAsync(GetPublisherId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update publisher
        /// </summary>
        private async Task UpdatePublisherAsync(CliOptions options) {
            await _registry.UpdatePublisherAsync(GetPublisherId(options),
                new PublisherUpdateApiModel {
                    LogLevel = options.GetValueOrDefault<TraceLogLevel>(
                        "-l", "--log-level", null)
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Monitor publishers
        /// </summary>
        private async Task MonitorPublishersAsync() {
            var events = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribePublisherEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _gatewayId;

        /// <summary>
        /// Get gateway id
        /// </summary>
        private string GetGatewayId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_gatewayId != null) {
                if (id == null) {
                    return _gatewayId;
                }
                _gatewayId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -i/--id option.");
        }

        /// <summary>
        /// List sites
        /// </summary>
        private async Task ListSitesAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllSitesAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListSitesAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Select gateway registration
        /// </summary>
        private async Task SelectGatewayAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _gatewayId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_gatewayId);
            }
            else {
                var gatewayId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(gatewayId)) {
                    var result = await _registry.ListAllGatewaysAsync().ConfigureAwait(false);
                    gatewayId = ConsoleEx.Select(result.Select(r => r.Id));
                    if (string.IsNullOrEmpty(gatewayId)) {
                        Console.WriteLine("Nothing selected - gateway selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {gatewayId}.");
                    }
                }
                _gatewayId = gatewayId;
            }
        }

        /// <summary>
        /// List gateway registrations
        /// </summary>
        private async Task ListGatewaysAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllGatewaysAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListGatewaysAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query gateway registrations
        /// </summary>
        private async Task QueryGatewaysAsync(CliOptions options) {
            var query = new GatewayQueryApiModel {
                Connected = options.IsProvidedOrNull("-c", "--connected"),
                SiteId = options.GetValueOrDefault<string>("-s", "--siteId", null)
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.QueryAllGatewaysAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.QueryGatewaysAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get gateway
        /// </summary>
        private async Task GetGatewayAsync(CliOptions options) {
            var result = await _registry.GetGatewayAsync(GetGatewayId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update gateway
        /// </summary>
        private async Task UpdateGatewayAsync(CliOptions options) {
            await _registry.UpdateGatewayAsync(GetGatewayId(options),
                new GatewayUpdateApiModel {
                    SiteId = options.GetValueOrDefault<string>("-s", "--siteId", null),
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Monitor gateways
        /// </summary>
        private async Task MonitorGatewaysAsync() {
            var events = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribeGatewayEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _supervisorId;

        /// <summary>
        /// Get supervisor id
        /// </summary>
        private string GetSupervisorId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_supervisorId != null) {
                if (id == null) {
                    return _supervisorId;
                }
                _supervisorId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -i/--id option.");
        }

        /// <summary>
        /// Select supervisor registration
        /// </summary>
        private async Task SelectSupervisorAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _supervisorId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_supervisorId);
            }
            else {
                var supervisorId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(supervisorId)) {
                    var result = await _registry.ListAllSupervisorsAsync().ConfigureAwait(false);
                    supervisorId = ConsoleEx.Select(result.Select(r => r.Id));
                    if (string.IsNullOrEmpty(supervisorId)) {
                        Console.WriteLine("Nothing selected - supervisor selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {supervisorId}.");
                    }
                }
                _supervisorId = supervisorId;
            }
        }

        /// <summary>
        /// List supervisor registrations
        /// </summary>
        private async Task ListSupervisorsAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllSupervisorsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListSupervisorsAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query supervisor registrations
        /// </summary>
        private async Task QuerySupervisorsAsync(CliOptions options) {
            var query = new SupervisorQueryApiModel {
                Connected = options.IsProvidedOrNull("-c", "--connected"),
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.QueryAllSupervisorsAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.QuerySupervisorsAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get supervisor
        /// </summary>
        private async Task GetSupervisorAsync(CliOptions options) {
            var result = await _registry.GetSupervisorAsync(GetSupervisorId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Monitor supervisors
        /// </summary>
        private async Task MonitorSupervisorsAsync() {
            var events = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribeSupervisorEventsAsync(
                PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _discovererId;

        /// <summary>
        /// Get discoverer id
        /// </summary>
        private string GetDiscovererId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_discovererId != null) {
                if (id == null) {
                    return _discovererId;
                }
                _discovererId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -i/--id option.");
        }

        /// <summary>
        /// Select discoverer registration
        /// </summary>
        private async Task SelectDiscovererAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _discovererId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_discovererId);
            }
            else {
                var discovererId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(discovererId)) {
                    var result = await _registry.ListAllDiscoverersAsync().ConfigureAwait(false);
                    discovererId = ConsoleEx.Select(result.Select(r => r.Id));
                    if (string.IsNullOrEmpty(discovererId)) {
                        Console.WriteLine("Nothing selected - discoverer selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {discovererId}.");
                    }
                }
                _discovererId = discovererId;
            }
        }

        /// <summary>
        /// List discoverer registrations
        /// </summary>
        private async Task ListDiscoverersAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllDiscoverersAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListDiscoverersAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query discoverer registrations
        /// </summary>
        private async Task QueryDiscoverersAsync(CliOptions options) {
            var query = new DiscovererQueryApiModel {
                Connected = options.IsProvidedOrNull("-c", "--connected")
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.QueryAllDiscoverersAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.QueryDiscoverersAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get discoverer
        /// </summary>
        private async Task GetDiscovererAsync(CliOptions options) {
            var result = await _registry.GetDiscovererAsync(
                GetDiscovererId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Monitor discoverers
        /// </summary>
        private async Task MonitorDiscoverersAsync(CliOptions options) {
            var devents = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var discovererId = options.GetValueOrDefault<string>("-i", "--id", null);
            var complete = await devents.SubscribeDiscovererEventsAsync(
                    PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Monitor all
        /// </summary>
        private async Task MonitorAllAsync() {
            var devents = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var supervisor = await devents.SubscribeSupervisorEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                var publisher = await devents.SubscribePublisherEventsAsync(PrintEvent).ConfigureAwait(false);
                try {
                    var discoverers = await devents.SubscribeDiscovererEventsAsync(PrintEvent).ConfigureAwait(false);
                    try {
                        Console.ReadKey();
                    }
                    finally {
                        await discoverers.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally {
                    await publisher.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally {
                await supervisor.DisposeAsync().ConfigureAwait(false);
            }
        }


        private string _requestId;

        /// <summary>
        /// Get request id
        /// </summary>
        private string GetRequestId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_requestId != null) {
                if (id == null) {
                    return _requestId;
                }
                _requestId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -i/--id option.");
        }

        /// <summary>
        /// Select request registration
        /// </summary>
        private async Task SelectRequestAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _requestId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_requestId);
            }
            else {
                var requestId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(requestId)) {
                    var result = await _vault.ListAllRequestsAsync().ConfigureAwait(false);
                    requestId = ConsoleEx.Select(result.Select(r => r.RequestId));
                    if (string.IsNullOrEmpty(requestId)) {
                        Console.WriteLine("Nothing selected - request selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {requestId}.");
                    }
                }
                _requestId = requestId;
            }
        }

        /// <summary>
        /// Query requests
        /// </summary>
        private async Task QueryRequestsAsync(CliOptions options) {
            var query = new CertificateRequestQueryRequestApiModel {
                EntityId = options.GetValueOrDefault<string>("-e", "--entityid", null),
                State = options.GetValueOrDefault<CertificateRequestState>("-s", "--state", null)
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _vault.QueryAllRequestsAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _vault.QueryRequestsAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// List requests
        /// </summary>
        private async Task ListRequestsAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _vault.ListAllRequestsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _vault.ListRequestsAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get request
        /// </summary>
        private async Task GetRequestAsync(CliOptions options) {
            var result = await _vault.GetRequestAsync(GetRequestId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Delete request
        /// </summary>
        private async Task DeleteRequestAsync(CliOptions options) {
            await _vault.DeleteRequestAsync(GetRequestId(options)).ConfigureAwait(false);
        }

        /// <summary>
        /// Accept request
        /// </summary>
        private async Task AcceptRequestAsync(CliOptions options) {
            await _vault.AcceptRequestAsync(GetRequestId(options)).ConfigureAwait(false);
        }

        /// <summary>
        /// Reject request
        /// </summary>
        private async Task RejectRequestAsync(CliOptions options) {
            await _vault.RejectRequestAsync(GetRequestId(options)).ConfigureAwait(false);
        }

        /// <summary>
        /// Approve request
        /// </summary>
        private async Task ApproveRequestAsync(CliOptions options) {
            await _vault.ApproveRequestAsync(GetRequestId(options)).ConfigureAwait(false);
        }

        /// <summary>
        /// Start and finish keypair request
        /// </summary>
        private async Task KeyPairRequestAsync(CliOptions options) {
            if (options.IsProvidedOrNull("-f", "--finish") == true) {
                var result = await _vault.FinishKeyPairRequestAsync(GetRequestId(options)).ConfigureAwait(false);
                PrintResult(options, result);
            }
            else {
                var result = await _vault.StartNewKeyPairRequestAsync(new StartNewKeyPairRequestApiModel {
                    CertificateType = options.GetValue<TrustGroupType>("-t", "--type"),
                    EntityId = options.GetValue<string>("-e", "--entityId"),
                    GroupId = options.GetValue<string>("-g", "--groupId"),
                    SubjectName = options.GetValueOrDefault<string>("-s", "--subject", null),
                    DomainNames = options.GetValueOrDefault<string>("-d", "--domain", null)?
                        .YieldReturn().ToList()
                }).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Start and finish signing request
        /// </summary>
        private async Task SigningRequestAsync(CliOptions options) {
            if (options.IsProvidedOrNull("-f", "--finish") == true) {
                var result = await _vault.FinishSigningRequestAsync(GetRequestId(options)).ConfigureAwait(false);
                PrintResult(options, result);
            }
            else {
                var result = await _vault.StartSigningRequestAsync(new StartSigningRequestApiModel {
                    CertificateRequest = _serializer.FromObject(
                        options.GetValue<byte[]>("-c", "--csr")),
                    EntityId = options.GetValue<string>("-e", "--entityId"),
                    GroupId = options.GetValue<string>("-g", "--groupId")
                }).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Remove trust relationship
        /// </summary>
        private async Task RemoveTrustRelationshipAsync(CliOptions options) {
            await _vault.RemoveTrustRelationshipAsync(
                options.GetValue<string>("-e", "--entityId"),
                options.GetValue<string>("-t", "--trustedId")).ConfigureAwait(false);
        }

        /// <summary>
        /// Get trusted certificates
        /// </summary>
        private async Task GetTrustedCertificatesAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _vault.ListAllTrustedCertificatesAsync(
                    options.GetValue<string>("-e", "--entityId")).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _vault.ListTrustedCertificatesAsync(
                    options.GetValue<string>("-e", "--entityId"),
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Add trust relationship
        /// </summary>
        private async Task AddTrustRelationshipAsync(CliOptions options) {
            await _vault.AddTrustRelationshipAsync(
                options.GetValue<string>("-e", "--entityId"),
                options.GetValue<string>("-t", "--trustedId")).ConfigureAwait(false);
        }
        private string _groupId;

        /// <summary>
        /// Get group id
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetGroupId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_groupId != null) {
                if (id == null) {
                    return _groupId;
                }
                _groupId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -i/--id option.");
        }

        /// <summary>
        /// Select group registration
        /// </summary>
        private async Task SelectCertificateGroupAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _groupId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_groupId);
            }
            else {
                var groupId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(groupId)) {
                    var result = await _vault.ListAllGroupsAsync().ConfigureAwait(false);
                    groupId = ConsoleEx.Select(result);
                    if (string.IsNullOrEmpty(groupId)) {
                        Console.WriteLine("Nothing selected - group selection cleared.");
                    }
                }
                _groupId = groupId;
            }
        }

        /// <summary>
        /// List groups
        /// </summary>
        private async Task ListCertificateGroupsAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _vault.ListAllGroupsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _vault.ListGroupsAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get root group
        /// </summary>
        private async Task CreateRootCertificateAsync(CliOptions options) {
            var result = await _vault.CreateRootAsync(new TrustGroupRootCreateRequestApiModel {
                IssuedKeySize = options.GetValueOrDefault<ushort>("-s", "--keysize", null),
                IssuedLifetime = options.GetValueOrDefault<TimeSpan>("-l", "--lifetime", null),
                IssuedSignatureAlgorithm = options.GetValueOrDefault<SignatureAlgorithm>(
                        "-a", "--algorithm", null),
                Name = options.GetValue<string>("-n", "--name"),
                SubjectName = options.GetValue<string>("-s", "--subject")
            }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Create group
        /// </summary>
        private async Task CreateCertificateGroupAsync(CliOptions options) {
            var result = await _vault.CreateGroupAsync(new TrustGroupRegistrationRequestApiModel {
                IssuedKeySize = options.GetValueOrDefault<ushort>("-s", "--keysize", null),
                IssuedLifetime = options.GetValueOrDefault<TimeSpan>("-l", "--lifetime", null),
                IssuedSignatureAlgorithm = options.GetValueOrDefault<SignatureAlgorithm>(
                        "-a", "--algorithm", null),
                Name = options.GetValueOrDefault<string>("-n", "--name", null),
                ParentId = options.GetValue<string>("-p", "--parent"),
                SubjectName = options.GetValue<string>("-s", "--subject")
            }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Get group
        /// </summary>
        private async Task GetCertificateGroupAsync(CliOptions options) {
            var result = await _vault.GetGroupAsync(GetGroupId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Delete group
        /// </summary>
        private async Task DeleteCertificateGroupAsync(CliOptions options) {
            await _vault.DeleteGroupAsync(GetGroupId(options)).ConfigureAwait(false);
        }

        /// <summary>
        /// Renew issuer cert
        /// </summary>
        private async Task RenewIssuerCertAsync(CliOptions options) {
            var result = await _vault.RenewIssuerCertificateAsync(GetGroupId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update group
        /// </summary>
        private async Task UpdateCertificateGroupAsync(CliOptions options) {
            await _vault.UpdateGroupAsync(GetGroupId(options),
                new TrustGroupUpdateRequestApiModel {
                    IssuedKeySize = options.GetValueOrDefault<ushort>("-s", "--keysize", null),
                    IssuedLifetime = options.GetValueOrDefault<TimeSpan>("-l", "--lifetime", null),
                    IssuedSignatureAlgorithm = options.GetValueOrDefault<SignatureAlgorithm>(
                        "-a", "--algorithm", null),
                    Name = options.GetValueOrDefault<string>("-n", "--name", null)
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get status
        /// </summary>
        private async Task GetStatusAsync() {
            Console.WriteLine("Registry:  " + await _registry.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("Vault:     " + await _vault.GetServiceStatusAsync().ConfigureAwait(false));
        }


        /// <summary>
        /// Print result
        /// </summary>
        private void PrintResult<T>(CliOptions options, T result) {
            Console.WriteLine("==================");
            Console.WriteLine(_serializer.SerializeToString(result,
                options.GetValueOrDefault("-F", "--format", SerializeOption.Indented)));
            Console.WriteLine("==================");
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(SupervisorEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(GatewayEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(DiscovererEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(PublisherEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintHelp() {
            Console.WriteLine(
                @"
aziiotcli - Allows to script Industrial IoT Services api.
usage:      aziiotcli command [options]

Commands and Options

     console     Run in interactive mode. Enter commands after the >
     exit        Exit interactive mode and thus the cli.
     status      Print status of services
     monitor     Monitor all events from all services

     gateways    Manage edge gateways
     publishers  Manage publisher modules
     supervisors Manage twin modules
     discoverers Manage discovery modules

     groups      Manage trust groups (Experimental)
     trust       Manage trust between above entities (Experimental)
     requests    Manage certificate requests (Experimental)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintGatewaysHelp() {
            Console.WriteLine(
                @"
Manage and configure Edge Gateways

Commands and Options

     select      Select gateway as -i/--id argument in all calls.
        with ...
        -i, --id        Gateway id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to gateways.

     update      Update gateway
        with ...
        -i, --id        Id of gateway to retrieve (mandatory)
        -s, --siteId    Updated site of the gateway.

     sites       List gateway sites
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all sites (unpaged)
        -F, --format    Json format for result

     list        List gateways
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all gateways (unpaged)
        -F, --format    Json format for result

     query       Find gateways
        -c, --connected Only return connected or disconnected.
        -s, --siteId    Site of the gateways.
        -P, --page-size Size of page
        -A, --all       Return all endpoints (unpaged)
        -F, --format    Json format for result

     get         Get gateway info
        with ...
        -i, --id        Id of gateway to retrieve (mandatory)
        -F, --format    Json format for result

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintPublishersHelp() {
            Console.WriteLine(
                @"
Manage and configure Publisher modules

Commands and Options

     select      Select publisher as -i/--id argument in all calls.
        with ...
        -i, --id        Publisher id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to publishers.

     update      Update publisher
        with ...
        -i, --id        Id of publisher to retrieve (mandatory)
        -l, --log-level Set publisher module logging level

     list        List publishers
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all publishers (unpaged)
        -F, --format    Json format for result

     query       Find publishers
        -c, --connected Only return connected or disconnected.
        -P, --page-size Size of page
        -A, --all       Return all endpoints (unpaged)
        -F, --format    Json format for result

     get         Get publisher
        with ...
        -i, --id        Id of publisher to retrieve (mandatory)
        -F, --format    Json format for result

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintSupervisorsHelp() {
            Console.WriteLine(
                @"
Manage and configure Twin modules (endpoint supervisors)

Commands and Options

     select      Select supervisor as -i/--id argument in all calls.
        with ...
        -i, --id        Supervisor id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to supervisors.

     list        List supervisors
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all supervisors (unpaged)
        -F, --format    Json format for result

     query       Find supervisors
        -c, --connected Only return connected or disconnected.
        -P, --page-size Size of page
        -A, --all       Return all supervisors (unpaged)
        -F, --format    Json format for result

     get         Get supervisor
        with ...
        -i, --id        Id of supervisor to retrieve (mandatory)
        -F, --format    Json format for result

     status      Get supervisor runtime status
        with ...
        -i, --id        Id of supervisor to get status of (mandatory)
        -F, --format    Json format for result

     update      Update supervisor
        with ...
        -i, --id        Id of supervisor to update (mandatory)
        -l, --log-level Set supervisor module logging level

     reset       Reset supervisor
        with ...
        -i, --id        Id of supervisor to reset (mandatory)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintDiscoverersHelp() {
            Console.WriteLine(
                @"
Manage and configure discovery modules

Commands and Options

     select      Select discoverer as -i/--id argument in all calls.
        with ...
        -i, --id        Discoverer id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to discoverer twins.

     monitor     Monitor discovery progress of specified discoverer.
        with ...
        -i, --id        Discoverer to monitor

     list        List discoverers
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all discoverers (unpaged)
        -F, --format    Json format for result

     query       Find discoverers
        -c, --connected Only return connected or disconnected.
        -d, --discovery Discovery state.
        -P, --page-size Size of page
        -A, --all       Return all discoverers (unpaged)
        -F, --format    Json format for result

     get         Get discoverer
        with ...
        -i, --id        Id of discoverer to retrieve (mandatory)
        -F, --format    Json format for result

     update      Update discoverer
        with ...
        -i, --id        Id of discoverer to update (mandatory)
        -d, --discovery Set discoverer discovery mode
        -l, --log-level Set discoverer module logging level
        -a, --activate  Activate all endpoints during onboarding.
        -p, --port-ranges
                        Port ranges to scan.
        -r, --address-ranges
                        Address range to scan.
        -P, --port-probes
                        Max port probes to use.
        -R, --address-probes
                        Max networking probes to use.

     scan        Run a scan
        with ...
        -i, --id        Id of discoverer to run scanning on (mandatory)
        -d, --discovery Set discoverer discovery mode
        -a, --activate  Activate all endpoints during onboarding.
        -I, --idle-time Idle time between scans in seconds
        -p, --port-ranges
                        Port ranges to scan.
        -r, --address-ranges
                        Address range to scan.
        -P, --max-port-probes
                        Max port probes to use.
        -R, --max-address-probes
                        Max networking probes to use.
        -T, --address-probe-timeout
                        Network probe timeout in milliseconds
        -t, --port-probe-timeout
                        Port probe timeout in milliseconds

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintCertificateGroupsHelp() {
            Console.WriteLine(
                @"
Manage entity trust groups

Commands and Options

     select      Select group as -i/--id argument in all calls.
        with ...
        -i, --id        Group id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     list        List groups
        with ...
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     root        Create new root group
        with ...
        -n, --name      Name of the group (mandatory)
        -s, --subject   Subject distinguished name (mandatory)
        -a, --algorithm Signature algorithm
        -l, --lifetime  Issued certificate life times
        -s, --keysize   Issued Key size
        -F, --format    Json format for result

     child       Create new sub group
        with ...
        -p, --parent    Parent id for the group (mandatory)
        -n, --name      Name of the group (mandatory)
        -s, --subject   Subject distinguished name (mandatory)
        -a, --algorithm Signature algorithm
        -l, --lifetime  Issued certificate life times
        -s, --keysize   Issued Key size
        -F, --format    Json format for result

     delete      Delete group
        with ...
        -i, --id        Id of group to delete (mandatory)

     delete      Renew group issuer certificate
        with ...
        -i, --id        Id of group for renewal (mandatory)

     update      Update group information
        with ...
        -i, --id        Id of the group to update (mandatory)
        -n, --name      Name of the group (mandatory)
        -s, --subject   Subject distinguished name (mandatory)
        -a, --algorithm Signature algorithm
        -l, --lifetime  Issued certificate life times

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintRequestsHelp() {
            Console.WriteLine(
                @"
Submit and manage Certificate requests

Commands and Options

     select      Select request as -i/--id argument in all calls.
        with ...
        -i, --id        Request id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     list        List requests
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all requests (unpaged)
        -F, --format    Json format for result

     query       Find requests
        -s, --state     State of request
        -e, --entityId  Entity id for which request was submitted
        -P, --page-size Size of page
        -A, --all       Return all endpoints (unpaged)
        -F, --format    Json format for result

     sign        Submit certificate signing request
        with ...
        -g, --groupId   Group to submit request to (mandatory)
        -e, --entityId  Entity id to create key for (mandatory)
        or ...
        -f, --finish    Retrieve finished signing result, then
        -i, --id        Id of request to finish (mandatory)

     get         Get request
        with ...
        -i, --id        Id of request to retrieve (mandatory)
        -F, --format    Json format for result

     approve     Approve request
        with ...
        -i, --id        Id of request to approve (mandatory)

     reject      Reject request
        with ...
        -i, --id        Id of request to reject (mandatory)

     accept      Accept request
        with ...
        -i, --id        Id of request to retrieve (mandatory)

     delete      Delete request
        with ...
        -i, --id        Id of request to retrieve (mandatory)

     keypair     Submit key pair generation request
        with ...
        -g, --groupId   Group to submit request to (mandatory)
        -e, --entityId  Entity id to create key for (mandatory)
        -t, --type      Type of certificate to generate (mandatory)
        -a, --subjct    Subject name (mandatory)
        -d, --domain    Domain name (mandatory)
        or ...
        -f, --finish    Retrieve finished signing result, then
        -i, --id        Id of request to finish (mandatory)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintTrustHelp() {
            Console.WriteLine(
                @"
Manage trust between entities

Commands and Options

     create      Add trust relationship
        with ...
        -e, --entityId  Id of entity (mandatory)
        -t, --trustedId Id of trusted entity (mandatory)

     get         Get certificates the entity trusts.
        with ...
        -e, --entityId  Id of entity (mandatory)
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     delete      Remove trust relationship
        with ...
        -e, --entityId  Id of entity (mandatory)
        -t, --trustedId Id of entity not to trust (mandatory)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        private readonly ILifetimeScope _scope;
        private readonly IRegistryServiceApi _registry;
        private readonly IVaultServiceApi _vault;
        private readonly IMetricServer _metrics;
        private readonly IJsonSerializer _serializer;
    }
}
