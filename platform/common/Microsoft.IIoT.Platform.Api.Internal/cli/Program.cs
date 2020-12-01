// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Api.Cli {
    using Microsoft.IIoT.Api.Runtime;
    using Microsoft.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using Microsoft.IIoT.Platform.Publisher.Api;
    using Microsoft.IIoT.Platform.Publisher.Api.Clients;
    using Microsoft.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.IIoT.Platform.Discovery.Api;
    using Microsoft.IIoT.Platform.Discovery.Api.Clients;
    using Microsoft.IIoT.Platform.Discovery.Api.Models;
    using Microsoft.IIoT.Platform.Registry.Api;
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using Microsoft.IIoT.Platform.Twin.Api;
    using Microsoft.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.IIoT.Platform.Twin.Api.Models;
    using Microsoft.IIoT.Platform.Vault.Api;
    using Microsoft.IIoT.Platform.Vault.Api.Clients;
    using Microsoft.IIoT.Platform.Vault.Api.Models;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Http.SignalR;
    using Microsoft.IIoT.Utils;
    using Microsoft.IIoT.Authentication.Runtime;
    using Microsoft.IIoT.Serializers;
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

            // Register twin, vault, and registry services clients
            builder.RegisterType<TwinServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscoveryServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<VaultServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherServiceClient>()
                .AsImplementedInterfaces();

            // ... with client event callbacks
            builder.RegisterType<RegistryServiceEvents>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DiscoveryServiceEvents>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinServiceEvents>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherServiceEvents>()
                .AsImplementedInterfaces();

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
            _twin = _scope.Resolve<ITwinServiceApi>();
            _directory = _scope.Resolve<IRegistryServiceApi>();
            _registry = _scope.Resolve<IDiscoveryServiceApi>();
            _history = _scope.Resolve<IHistoryServiceApi>();
            _publisher = _scope.Resolve<IPublisherServiceApi>();
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
                        case "apps":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "register":
                                    await RegisterApplicationAsync(options).ConfigureAwait(false);
                                    break;
                                case "add":
                                    await RegisterServerAsync(options).ConfigureAwait(false);
                                    break;
                                case "discover":
                                    await DiscoverServersAsync(options).ConfigureAwait(false);
                                    break;
                                case "cancel":
                                    await CancelDiscoveryAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateApplicationAsync(options).ConfigureAwait(false);
                                    break;
                                case "unregister":
                                    await UnregisterApplicationAsync(options).ConfigureAwait(false);
                                    break;
                                case "purge":
                                    await PurgeDisabledApplicationsAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListApplicationsAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorApplicationsAsync().ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectApplicationAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryApplicationsAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetApplicationAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintApplicationsHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "endpoints":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "get":
                                    await GetEndpointAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListEndpointsAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorEndpointsAsync().ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectEndpointsAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryEndpointsAsync(options).ConfigureAwait(false);
                                    break;
                                case "validate":
                                    await GetEndpointCertificateAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintEndpointsHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "twins":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
#if FALSE
                                case "activate":
                                    await ActivateEndpointsAsync(options).ConfigureAwait(false);
                                    break;
                                case "deactivate":
                                    await DeactivateEndpointsAsync(options).ConfigureAwait(false);
                                    break;
#endif
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintTwinsHelp();
                                    break;
                            }
                            break;
                        case "groups":
                        case "writergroups":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "create":
                                    await AddWriterGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateWriterGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "delete":
                                    await DeleteWriterGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListWriterGroupsAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryWriterGroupsAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorWriterGroupsAsync().ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectWriterGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetWriterGroupAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintWriterGroupsHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "writers":
                        case "datasets":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "create":
                                    await AddDataSetWriterAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateDataSetWriterAsync(options).ConfigureAwait(false);
                                    break;
                                case "delete":
                                    await RemoveDataSetWriterAsync(options).ConfigureAwait(false);
                                    break;
                                case "list":
                                    await ListDataSetWritersAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryDataSetWritersAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorDataSetWritersAsync().ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectDataSetWriterAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetDataSetWriterAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintDataSetWritersHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "variables":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "add":
                                    await AddDataSetVariableAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await ListDataSetVariablesAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateDataSetVariableAsync(options).ConfigureAwait(false);
                                    break;
                                case "remove":
                                    await RemoveDataSetVariableAsync(options).ConfigureAwait(false);
                                    break;
                                case "delete":
                                    await RemoveDataSetWriterAsync(options).ConfigureAwait(false);
                                    break;
                                case "query":
                                    await QueryDataSetVariablesAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorDataSetVariableAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintDataSetVariablesHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
                            break;
                        case "events":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "add":
                                    await AddEventDataSetAsync(options).ConfigureAwait(false);
                                    break;
                                case "get":
                                    await GetEventDataSetAsync(options).ConfigureAwait(false);
                                    break;
                                case "update":
                                    await UpdateEventDataSetAsync(options).ConfigureAwait(false);
                                    break;
                                case "remove":
                                    await RemoveEventDataSetAsync(options).ConfigureAwait(false);
                                    break;
                                case "monitor":
                                    await MonitorEventDataSetAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintEventDataSetHelp();
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown command {command}.");
                            }
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
                                case "update":
                                    await UpdateDiscovererAsync(options).ConfigureAwait(false);
                                    break;
                                case "scan":
                                    await DiscovererScanAsync(options).ConfigureAwait(false);
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
                                case "update":
                                    await UpdateSupervisorAsync(options).ConfigureAwait(false);
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
                        case "nodes":
                            if (args.Length < 2) {
                                throw new ArgumentException("Need a command!");
                            }
                            command = args[1];
                            options = new CliOptions(args, 2);
                            switch (command) {
                                case "browse":
                                    await BrowseAsync(options).ConfigureAwait(false);
                                    break;
                                case "select":
                                    await SelectNodeAsync(options).ConfigureAwait(false);
                                    break;
                                case "read":
                                    await ReadAsync(options).ConfigureAwait(false);
                                    break;
                                case "write":
                                    await WriteAsync(options).ConfigureAwait(false);
                                    break;
                                case "metadata":
                                    await MethodMetadataAsync(options).ConfigureAwait(false);
                                    break;
                                case "call":
                                    await MethodCallAsync(options).ConfigureAwait(false);
                                    break;
                                case "-?":
                                case "-h":
                                case "--help":
                                case "help":
                                    PrintNodesHelp();
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

        private string _nodeId;

        /// <summary>
        /// Get endpoint id
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetNodeId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-n", "--nodeid", null);
            if (_nodeId != null) {
                if (id == null) {
                    return _nodeId;
                }
                _nodeId = null;
            }
            if (id != null) {
                return id;
            }
            if (!shouldThrow) {
                return null;
            }
            throw new ArgumentException("Missing -n/--nodeId option.");
        }

        /// <summary>
        /// Select node id
        /// </summary>
        private async Task SelectNodeAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _nodeId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_nodeId);
            }
            else {
                var nodeId = options.GetValueOrDefault<string>("-n", "--nodeid", null);
                if (string.IsNullOrEmpty(nodeId)) {
                    var id = GetEndpointId(options, false);
                    if (!string.IsNullOrEmpty(id)) {
                        var results = await _twin.NodeBrowseAsync(id, new BrowseRequestApiModel {
                            TargetNodesOnly = true,
                            NodeId = _nodeId
                        }).ConfigureAwait(false);
                        var node = ConsoleEx.Select(results.References.Select(r => r.Target),
                            n => n.BrowseName);
                        if (node != null) {
                            nodeId = node.NodeId;
                        }
                    }
                    if (string.IsNullOrEmpty(nodeId)) {
                        Console.WriteLine("Nothing selected.");
                        return;
                    }
                    Console.WriteLine($"Selected {nodeId}.");
                }
                _nodeId = nodeId;
            }
        }

        /// <summary>
        /// Call method
        /// </summary>
        private async Task MethodCallAsync(CliOptions options) {
            var result = await _twin.NodeMethodCallAsync(
                GetEndpointId(options),
                new MethodCallRequestApiModel {
                    MethodId = GetNodeId(options),
                    ObjectId = options.GetValue<string>("-o", "--objectid")

                    // ...
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Read value
        /// </summary>
        private async Task MethodMetadataAsync(CliOptions options) {
            var result = await _twin.NodeMethodGetMetadataAsync(
                GetEndpointId(options),
                new MethodMetadataRequestApiModel {
                    MethodId = GetNodeId(options)
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Write value
        /// </summary>
        private async Task WriteAsync(CliOptions options) {
            var result = await _twin.NodeValueWriteAsync(
                GetEndpointId(options),
                new ValueWriteRequestApiModel {
                    NodeId = GetNodeId(options),
                    DataType = options.GetValueOrDefault<string>("-t", "--datatype", null),
                    Value = _serializer.FromObject(options.GetValue<string>("-v", "--value"))
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Read value
        /// </summary>
        private async Task ReadAsync(CliOptions options) {
            var result = await _twin.NodeValueReadAsync(
                GetEndpointId(options),
                new ValueReadRequestApiModel {
                    NodeId = GetNodeId(options)
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Browse nodes
        /// </summary>
        private async Task BrowseAsync(CliOptions options) {
            var id = GetEndpointId(options);
            var silent = options.IsSet("-s", "--silent");
            var all = options.IsSet("-A", "--all");
            var recursive = options.IsSet("-r", "--recursive");
            var readDuringBrowse = options.IsProvidedOrNull("-v", "--readvalue");
            var request = new BrowseRequestApiModel {
                TargetNodesOnly = options.IsProvidedOrNull("-t", "--targets"),
                ReadVariableValues = readDuringBrowse,
                MaxReferencesToReturn = options.GetValueOrDefault<uint>("-x", "--maxrefs", null),
                Direction = options.GetValueOrDefault<BrowseDirection>("-d", "--direction", null)
            };
            var nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                options.GetValueOrDefault<string>("-n", "--nodeid", null)
            };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nodesRead = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var errors = 0;
            var sw = Stopwatch.StartNew();
            while (nodes.Count > 0) {
                request.NodeId = nodes.First();
                nodes.Remove(request.NodeId);
                try {
                    var result = await (all ?
                        _twin.NodeBrowseAsync(id, request) :
                        _twin.NodeBrowseFirstAsync(id, request)).ConfigureAwait(false);
                    visited.Add(request.NodeId);
                    if (!silent) {
                        PrintResult(options, result);
                    }
                    if (readDuringBrowse ?? false) {
                        continue;
                    }
                    // Do recursive browse
                    if (recursive) {
                        foreach (var r in result.References) {
                            if (!visited.Contains(r.ReferenceTypeId)) {
                                nodes.Add(r.ReferenceTypeId);
                            }
                            if (!visited.Contains(r.Target.NodeId)) {
                                nodes.Add(r.Target.NodeId);
                            }
                            if (nodesRead.Contains(r.Target.NodeId)) {
                                continue; // We have read this one already
                            }
                            if (!r.Target.NodeClass.HasValue ||
                                r.Target.NodeClass.Value != NodeClass.Variable) {
                                continue;
                            }
                            if (!silent) {
                                Console.WriteLine($"Reading {r.Target.NodeId}");
                            }
                            try {
                                nodesRead.Add(r.Target.NodeId);
                                var read = await _twin.NodeValueReadAsync(id,
                                    new ValueReadRequestApiModel {
                                        NodeId = r.Target.NodeId
                                    }).ConfigureAwait(false);
                                if (!silent) {
                                    PrintResult(options, read);
                                }
                            }
                            catch (Exception ex) {
                                Console.WriteLine($"Reading {r.Target.NodeId} resulted in {ex}");
                                errors++;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Console.WriteLine($"Browse {request.NodeId} resulted in {e}");
                    errors++;
                }
            }
            Console.WriteLine($"Browse took {sw.Elapsed}. Visited " +
                $"{visited.Count} nodes and read {nodesRead.Count} of them with {errors} errors.");
        }

        private string _dataSetWriterId;

        /// <summary>
        /// Get writer id
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetDataSetWriterId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_dataSetWriterId != null) {
                if (id == null) {
                    return _dataSetWriterId;
                }
                _dataSetWriterId = null;
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
        /// Select writer registration
        /// </summary>
        private async Task SelectDataSetWriterAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _dataSetWriterId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_dataSetWriterId);
            }
            else {
                var dataSetWriterId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(dataSetWriterId)) {
                    var result = await _publisher.ListAllDataSetWritersAsync().ConfigureAwait(false);
                    dataSetWriterId = ConsoleEx.Select(result.Select(a => a.DataSetWriterId));
                    if (string.IsNullOrEmpty(dataSetWriterId)) {
                        Console.WriteLine("Nothing selected - writer selection cleared.");
                    }
                }
                _dataSetWriterId = dataSetWriterId;
            }
        }

        /// <summary>
        /// Register new dataset writer
        /// </summary>
        private async Task AddDataSetWriterAsync(CliOptions options) {
            var result = await _publisher.AddDataSetWriterAsync(new DataSetWriterAddRequestApiModel {
                DataSetFieldContentMask =
                    options.GetValueOrDefault<DataSetFieldContentMask?>("-c", "--content", null),
                EndpointId = options.GetValue<string>("-e", "--endpoint"),
                WriterGroupId = options.GetValueOrDefault<string>("-g", "--group", null),
                DataSetName = options.GetValueOrDefault<string>("-n", "--name", null),
                // User = null,
                // ExtensionFields = null,
                MessageSettings = BuildDataSetWriterMessageSettings(options),
                SubscriptionSettings = BuildDataSetWriterSubscriptionSettings(options)
            }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Read full dataset writer model which includes all
        /// dataset members if there are any.
        /// </summary>
        private async Task GetDataSetWriterAsync(CliOptions options) {
            var result = await _publisher.GetDataSetWriterAsync(GetDataSetWriterId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update an existing application, e.g. server
        /// certificate, or additional capabilities.
        /// </summary>
        private async Task UpdateDataSetWriterAsync(CliOptions options) {
            await _publisher.UpdateDataSetWriterAsync(GetDataSetWriterId(options),
                new DataSetWriterUpdateRequestApiModel {
                    GenerationId = options.GetValueOrDefault<string>("-g", "--genid", null),
                    DataSetFieldContentMask =
                        options.GetValueOrDefault<DataSetFieldContentMask?>("-c", "--content", null),
                    WriterGroupId = options.GetValueOrDefault<string>("-g", "--group", null),
                    DataSetName = options.GetValueOrDefault<string>("-n", "--name", null),
                    // User = null,
                    // ExtensionFields = null,
                    MessageSettings = BuildDataSetWriterMessageSettings(options),
                    SubscriptionSettings = BuildDataSetWriterSubscriptionSettings(options)
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// List all dataset writers or continue find query.
        /// </summary>
        private async Task ListDataSetWritersAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _publisher.ListAllDataSetWritersAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _publisher.ListDataSetWritersAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Find dataset writers
        /// </summary>
        private async Task QueryDataSetWritersAsync(CliOptions options) {
            var query = new DataSetWriterInfoQueryApiModel {
                DataSetName = options.GetValueOrDefault<string>("-n", "--name", null),
                EndpointId = options.GetValueOrDefault<string>("-e", "--endpoint", null),
                WriterGroupId = options.GetValueOrDefault<string>("-g", "--group", null)
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _publisher.QueryAllDataSetWritersAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _publisher.QueryDataSetWritersAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Unregister dataset writer and linked items.
        /// </summary>
        private async Task RemoveDataSetWriterAsync(CliOptions options) {
            await _publisher.RemoveDataSetWriterAsync(GetDataSetWriterId(options),
                options.GetValue<string>("-g", "--genid")).ConfigureAwait(false);
        }

        /// <summary>
        /// Monitor writer registrations
        /// </summary>
        private async Task MonitorDataSetWritersAsync() {
            var events = _scope.Resolve<IPublisherServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribeDataSetWriterEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Register new event based dataset
        /// </summary>
        private async Task AddEventDataSetAsync(CliOptions options) {
            var result = await _publisher.AddEventDataSetAsync(GetDataSetWriterId(options),
                new DataSetAddEventRequestApiModel {
                    DiscardNew = options.GetValueOrDefault<bool>("-d", "--discard", null),
                    EventNotifier = options.GetValueOrDefault<string>("-n", "--notifier", null),
                    BrowsePath = options.GetValueOrDefault<string[]>("-p", "--path", null),
                    QueueSize = options.GetValueOrDefault<uint>("-q", "--queue", null),
                    MonitoringMode = options.GetValueOrDefault<MonitoringMode?>("-m", "--mode", null),
                    TriggerId = options.GetValueOrDefault<string>("-t", "--triggerid", null),
                    Filter = null, // TODO
                    SelectedFields = null // TODO
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Read full event set if any.
        /// </summary>
        private async Task GetEventDataSetAsync(CliOptions options) {
            var result = await _publisher.GetEventDataSetAsync(GetDataSetWriterId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update an existing event set.
        /// </summary>
        private async Task UpdateEventDataSetAsync(CliOptions options) {
            await _publisher.UpdateEventDataSetAsync(GetDataSetWriterId(options),
                new DataSetUpdateEventRequestApiModel {
                    GenerationId = options.GetValue<string>("-g", "--genid"),
                    DiscardNew = options.GetValueOrDefault<bool>("-d", "--discard", null),
                    QueueSize = options.GetValueOrDefault<uint>("-q", "--queue", null),
                    MonitoringMode = options.GetValueOrDefault<MonitoringMode?>("-m", "--mode", null),
                    TriggerId = options.GetValueOrDefault<string>("-t", "--triggerid", null),
                    Filter = null, // TODO
                    SelectedFields = null // TODO
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Monitor samples from variable
        /// </summary>
        private async Task MonitorEventDataSetAsync(CliOptions options) {
            var dataSetWriterId = GetDataSetWriterId(options);
            var events = _scope.Resolve<IPublisherServiceEvents>();
            Console.WriteLine("Press any key to stop.");

            var finish = await events.SubscribeEventDataSetMessagesAsync(
                dataSetWriterId, PrintSample).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await finish.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Unregister eventset and remove from dataset.
        /// </summary>
        private async Task RemoveEventDataSetAsync(CliOptions options) {
            await _publisher.RemoveEventDataSetAsync(GetDataSetWriterId(options),
                options.GetValue<string>("-g", "--genid")).ConfigureAwait(false);
        }

        /// <summary>
        /// Register new dataset variable
        /// </summary>
        private async Task AddDataSetVariableAsync(CliOptions options) {
            var result = await _publisher.AddDataSetVariableAsync(GetDataSetWriterId(options),
                new DataSetAddVariableRequestApiModel {
                    PublishedVariableDisplayName = options.GetValueOrDefault<string>("-d", "--name", null),
                    PublishedVariableNodeId = options.GetValue<string>("-n", "--nodeid"),
                    DiscardNew = options.GetValueOrDefault<bool>("-D", "--discard", null),
                    Attribute = options.GetValueOrDefault<NodeAttribute?>("-a", "--attribute", null),
                    DataChangeFilter = options.GetValueOrDefault<DataChangeTriggerType?>("-f", "--filter", null),
                    BrowsePath = options.GetValueOrDefault<string[]>("-p", "--path", null),
                    QueueSize = options.GetValueOrDefault<uint>("-q", "--queue", null),
                    DeadbandType = options.GetValueOrDefault<DeadbandType?>("-B", "--dbtype", null),
                    DeadbandValue = options.GetValueOrDefault<double?>("-b", "--deadband", null),
                    HeartbeatInterval = options.GetValueOrDefault<TimeSpan>("-h", "--heartbeat", null),
                    SamplingInterval = options.GetValueOrDefault<TimeSpan>("-s", "--sampling", null),
                    IndexRange = options.GetValueOrDefault<string>("-r", "--range", null),
                    Order = options.GetValueOrDefault<int>("-o", "--order", null),
                    //  MetaDataProperties = null, // TODO
                    //  SubstituteValue = null // TODO
                    MonitoringMode = options.GetValueOrDefault<MonitoringMode?>("-m", "--mode", null),
                    TriggerId = options.GetValueOrDefault<string>("-t", "--triggerid", null),
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update an existing variable info.
        /// </summary>
        private async Task UpdateDataSetVariableAsync(CliOptions options) {
            await _publisher.UpdateDataSetVariableAsync(GetDataSetWriterId(options),
                options.GetValue<string>("-v", "--variable"),
                new DataSetUpdateVariableRequestApiModel {
                    GenerationId = options.GetValueOrDefault<string>("-g", "--genid", null),
                    PublishedVariableDisplayName = options.GetValueOrDefault<string>("-d", "--name", null),
                    DiscardNew = options.GetValueOrDefault<bool>("-D", "--discard", null),
                    DataChangeFilter = options.GetValueOrDefault<DataChangeTriggerType?>("-f", "--filter", null),
                    QueueSize = options.GetValueOrDefault<uint>("-q", "--queue", null),
                    DeadbandType = options.GetValueOrDefault<DeadbandType?>("-B", "--dbtype", null),
                    DeadbandValue = options.GetValueOrDefault<double?>("-b", "--deadband", null),
                    HeartbeatInterval = options.GetValueOrDefault<TimeSpan>("-h", "--heartbeat", null),
                    SamplingInterval = options.GetValueOrDefault<TimeSpan>("-s", "--sampling", null),
                    //  MetaDataProperties = null, // TODO
                    //  SubstituteValue = null // TODO
                    MonitoringMode = options.GetValueOrDefault<MonitoringMode?>("-m", "--mode", null),
                    TriggerId = options.GetValueOrDefault<string>("-t", "--triggerid", null),
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// List all dataset variables or continue find query.
        /// </summary>
        private async Task ListDataSetVariablesAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _publisher.ListAllDataSetVariablesAsync(GetDataSetWriterId(options)).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _publisher.ListDataSetVariablesAsync(GetDataSetWriterId(options),
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query variables in dataset.
        /// </summary>
        private async Task QueryDataSetVariablesAsync(CliOptions options) {
            var query = new PublishedDataSetVariableQueryApiModel {
                PublishedVariableDisplayName = options.GetValueOrDefault<string>("-d", "--name", null),
                Attribute = options.GetValueOrDefault<NodeAttribute?>("-a", "--attribute", null),
                PublishedVariableNodeId = options.GetValue<string>("-n", "--nodeid"),
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _publisher.QueryAllDataSetVariablesAsync(
                    GetDataSetWriterId(options), query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _publisher.QueryDataSetVariablesAsync(
                    GetDataSetWriterId(options), query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Monitor samples from variable
        /// </summary>
        private async Task MonitorDataSetVariableAsync(CliOptions options) {
            var dataSetWriterId = GetDataSetWriterId(options);
            var events = _scope.Resolve<IPublisherServiceEvents>();
            Console.WriteLine("Press any key to stop.");

            var variableId = options.GetValue<string>("-v", "--variableId");
            var finish = await events.SubscribeDataSetVariableMessagesAsync(
                dataSetWriterId, variableId, PrintSample).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await finish.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Unregister dataset variable and remove from dataset.
        /// </summary>
        private async Task RemoveDataSetVariableAsync(CliOptions options) {
            await _publisher.RemoveDataSetVariableAsync(GetDataSetWriterId(options),
                options.GetValue<string>("-v", "--variable"),
                options.GetValue<string>("-g", "--genid")).ConfigureAwait(false);
        }

        private string _writerGroupId;

        /// <summary>
        /// Get writer group id
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetWriterGroupId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_writerGroupId != null) {
                if (id == null) {
                    return _writerGroupId;
                }
                _writerGroupId = null;
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
        /// Select writer group registration
        /// </summary>
        private async Task SelectWriterGroupAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _writerGroupId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_writerGroupId);
            }
            else {
                var writerGroupId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(writerGroupId)) {
                    var result = await _publisher.ListAllWriterGroupsAsync().ConfigureAwait(false);
                    writerGroupId = ConsoleEx.Select(result.Select(a => a.WriterGroupId));
                    if (string.IsNullOrEmpty(writerGroupId)) {
                        Console.WriteLine("Nothing selected - group selection cleared.");
                    }
                }
                _writerGroupId = writerGroupId;
            }
        }

        /// <summary>
        /// List writer groups
        /// </summary>
        private async Task ListWriterGroupsAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _publisher.ListAllWriterGroupsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _publisher.ListWriterGroupsAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Create writer group
        /// </summary>
        private async Task AddWriterGroupAsync(CliOptions options) {
            var result = await _publisher.AddWriterGroupAsync(new WriterGroupAddRequestApiModel {
                BatchSize = options.GetValueOrDefault<int>("-b", "--batchsize", null),
                Name = options.GetValueOrDefault<string>("-n", "--name", null),
                PublishingInterval = options.GetValueOrDefault<TimeSpan>("-p", "--publish", null),
                HeaderLayoutUri = options.GetValueOrDefault<string>("-h", "--header", null),
                KeepAliveTime = options.GetValueOrDefault<TimeSpan>("-k", "--keepalive", null),
                Encoding = options.GetValueOrDefault<NetworkMessageEncoding?>("-e", "--encoding", null),
                Priority = options.GetValueOrDefault<byte>("-P", "--priority", null),
                // LocaleIds = ...
                MessageSettings = BuildWriterGroupMessageSettings(options)
            }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Get writer group
        /// </summary>
        private async Task GetWriterGroupAsync(CliOptions options) {
            var result = await _publisher.GetWriterGroupAsync(GetWriterGroupId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Delete writer group
        /// </summary>
        private async Task DeleteWriterGroupAsync(CliOptions options) {
            await _publisher.RemoveWriterGroupAsync(GetWriterGroupId(options),
                options.GetValue<string>("-g", "--genid")).ConfigureAwait(false);
        }

        /// <summary>
        /// Update writer group
        /// </summary>
        private async Task UpdateWriterGroupAsync(CliOptions options) {
            await _publisher.UpdateWriterGroupAsync(GetWriterGroupId(options),
                new WriterGroupUpdateRequestApiModel {
                    GenerationId = options.GetValue<string>("-g", "--genid"),
                    BatchSize = options.GetValueOrDefault<int>("-b", "--batchsize", null),
                    Name = options.GetValueOrDefault<string>("-n", "--name", null),
                    PublishingInterval = options.GetValueOrDefault<TimeSpan>("-P", "--publish", null),
                    HeaderLayoutUri = options.GetValueOrDefault<string>("-h", "--header", null),
                    KeepAliveTime = options.GetValueOrDefault<TimeSpan>("-k", "--keepalive", null),
                    Encoding = options.GetValueOrDefault<NetworkMessageEncoding?>("-e", "--encoding", null),
                    Priority = options.GetValueOrDefault<byte>("-p", "--priority", null),
                    // LocaleIds = ...
                    MessageSettings = BuildWriterGroupMessageSettings(options)
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Query writer group registrations
        /// </summary>
        private async Task QueryWriterGroupsAsync(CliOptions options) {
            var query = new WriterGroupInfoQueryApiModel {
                GroupVersion = options.GetValueOrDefault<uint>("-v", "--group-version", null),
                Encoding = options.GetValueOrDefault<NetworkMessageEncoding?>("-e", "--encoding", null),
                Name = options.GetValueOrDefault<string>("-n", "--name", null),
                Priority = options.GetValueOrDefault<byte>("-p", "--priority", null),
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _publisher.QueryAllWriterGroupsAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _publisher.QueryWriterGroupsAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Monitor writer group registration
        /// </summary>
        private async Task MonitorWriterGroupsAsync() {
            var events = _scope.Resolve<IPublisherServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribeWriterGroupEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
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
                    var result = await _directory.ListAllPublishersAsync().ConfigureAwait(false);
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
                var result = await _directory.ListAllPublishersAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.ListPublishersAsync(
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
                var result = await _directory.QueryAllPublishersAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.QueryPublishersAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get publisher
        /// </summary>
        private async Task GetPublisherAsync(CliOptions options) {
            var result = await _directory.GetPublisherAsync(GetPublisherId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update publisher
        /// </summary>
        private async Task UpdatePublisherAsync(CliOptions options) {
            await _directory.UpdatePublisherAsync(GetPublisherId(options),
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
                var result = await _directory.ListAllSitesAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.ListSitesAsync(
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
                    var result = await _directory.ListAllGatewaysAsync().ConfigureAwait(false);
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
                var result = await _directory.ListAllGatewaysAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.ListGatewaysAsync(
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
                var result = await _directory.QueryAllGatewaysAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.QueryGatewaysAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get gateway
        /// </summary>
        private async Task GetGatewayAsync(CliOptions options) {
            var result = await _directory.GetGatewayAsync(GetGatewayId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Update gateway
        /// </summary>
        private async Task UpdateGatewayAsync(CliOptions options) {
            await _directory.UpdateGatewayAsync(GetGatewayId(options),
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
                    var result = await _directory.ListAllSupervisorsAsync().ConfigureAwait(false);
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
                var result = await _directory.ListAllSupervisorsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.ListSupervisorsAsync(
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
                var result = await _directory.QueryAllSupervisorsAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.QuerySupervisorsAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get supervisor
        /// </summary>
        private async Task GetSupervisorAsync(CliOptions options) {
            var result = await _directory.GetSupervisorAsync(GetSupervisorId(options)).ConfigureAwait(false);
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

        /// <summary>
        /// Update supervisor
        /// </summary>
        private async Task UpdateSupervisorAsync(CliOptions options) {
            _ = BuildDiscoveryConfig(options);
            await _directory.UpdateSupervisorAsync(GetSupervisorId(options),
                new SupervisorUpdateApiModel {
                    LogLevel = options.GetValueOrDefault<TraceLogLevel>(
                        "-l", "--log-level", null)
                }).ConfigureAwait(false);
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
                    var result = await _directory.ListAllDiscoverersAsync().ConfigureAwait(false);
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
                var result = await _directory.ListAllDiscoverersAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.ListDiscoverersAsync(
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
                var result = await _directory.QueryAllDiscoverersAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _directory.QueryDiscoverersAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get discoverer
        /// </summary>
        private async Task GetDiscovererAsync(CliOptions options) {
            var result = await _directory.GetDiscovererAsync(
                GetDiscovererId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Monitor discoverers
        /// </summary>
        private async Task MonitorDiscoverersAsync(CliOptions options) {
            var revents = _scope.Resolve<IDiscoveryServiceEvents>();
            var devents = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            IAsyncDisposable complete;
            var discovererId = options.GetValueOrDefault<string>("-i", "--id", null);
            if (discovererId != null) {
                // If specified - monitor progress
                complete = await revents.SubscribeDiscoveryProgressByDiscovererIdAsync(
                    discovererId, PrintProgress).ConfigureAwait(false);
            }
            else {
                complete = await devents.SubscribeDiscovererEventsAsync(
                    PrintEvent).ConfigureAwait(false);
            }
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Update discoverer
        /// </summary>
        private async Task UpdateDiscovererAsync(CliOptions options) {
            var config = BuildDiscoveryConfig(options);
            await _directory.UpdateDiscovererAsync(GetDiscovererId(options),
                new DiscovererUpdateApiModel {
                    LogLevel = options.GetValueOrDefault<TraceLogLevel>(
                        "-l", "--log-level", null)
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Start and monitor discovery
        /// </summary>
        private async Task DiscovererScanAsync(CliOptions options) {
            var discovererId = GetDiscovererId(options);
            var events = _scope.Resolve<IDiscoveryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var discovery = await events.SubscribeDiscoveryProgressByDiscovererIdAsync(
                discovererId, PrintProgress).ConfigureAwait(false);
            try {
                var config = BuildDiscoveryConfig(options);
                var mode = options.GetValueOrDefault("-d", "--discovery",
                    config == null ? DiscoveryMode.Fast : DiscoveryMode.Scan);
                if (config == null) {
                    config = new DiscoveryConfigApiModel();
                }
                if (mode == DiscoveryMode.Off) {
                    throw new ArgumentException("-d/--discovery Off is not supported");
                }
                await _registry.SetDiscoveryModeAsync(discovererId, mode, config).ConfigureAwait(false);
                Console.ReadKey();
                await _registry.SetDiscoveryModeAsync(discovererId, DiscoveryMode.Off,
                    new DiscoveryConfigApiModel()).ConfigureAwait(false);
            }
            catch {
                await discovery.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _applicationId;

        /// <summary>
        /// Get application id
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetApplicationId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_applicationId != null) {
                if (id == null) {
                    return _applicationId;
                }
                _applicationId = null;
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
        /// Select application registration
        /// </summary>
        private async Task SelectApplicationAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _applicationId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_applicationId);
            }
            else {
                var applicationId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(applicationId)) {
                    var result = await _registry.ListAllApplicationsAsync().ConfigureAwait(false);
                    applicationId = ConsoleEx.Select(result.Select(r => r.ApplicationId));
                    if (string.IsNullOrEmpty(applicationId)) {
                        Console.WriteLine("Nothing selected - application selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {applicationId}.");
                    }
                }
                _applicationId = applicationId;
            }
        }

        /// <summary>
        /// Registers application
        /// </summary>
        private async Task RegisterApplicationAsync(CliOptions options) {
            var discoveryUrl = options.GetValueOrDefault<string>("-d", "--discoveryUrl", null);
            var result = await _registry.RegisterAsync(
                new ApplicationRegistrationRequestApiModel {
                    ApplicationUri = options.GetValue<string>("-u", "--url"),
                    ApplicationName = options.GetValueOrDefault<string>("-n", "--name", null),
                    GatewayServerUri = options.GetValueOrDefault<string>("-g", "--gwuri", null),
                    ApplicationType = options.GetValueOrDefault<ApplicationType>("-t", "--type", null),
                    ProductUri = options.GetValueOrDefault<string>("-p", "--product", null),
                    DiscoveryProfileUri = options.GetValueOrDefault<string>("-r", "--dpuri", null),
                    DiscoveryUrls = string.IsNullOrEmpty(discoveryUrl) ? null :
                        new HashSet<string> { discoveryUrl }
                }).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Registers server
        /// </summary>
        private async Task RegisterServerAsync(CliOptions options) {
            IDiscoveryServiceEvents events = null;
            var id = options.GetValueOrDefault("-i", "--id", Guid.NewGuid().ToString());
            if (options.IsSet("-m", "--monitor")) {
                events = _scope.Resolve<IDiscoveryServiceEvents>();
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                var discovery = await events.SubscribeDiscoveryProgressByRequestIdAsync(
                    id, async ev => {
                        await PrintProgress(ev).ConfigureAwait(false);
                        switch (ev.EventType) {
                            case DiscoveryProgressType.Error:
                            case DiscoveryProgressType.Cancelled:
                            case DiscoveryProgressType.Finished:
                                tcs.TrySetResult(true);
                                break;
                        }
                    }).ConfigureAwait(false);
                try {
                    await RegisterServerAsync(options, id).ConfigureAwait(false);
                    await tcs.Task.ConfigureAwait(false); // For completion
                }
                finally {
                    await discovery.DisposeAsync().ConfigureAwait(false);
                }
            }
            else {
                await RegisterServerAsync(options, id).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Discover servers
        /// </summary>
        private async Task RegisterServerAsync(CliOptions options, string id) {
            var activate = options.IsSet("-a", "--activate");
            await _registry.RegisterAsync(
                new ServerRegistrationRequestApiModel {
                    Id = id,
                    DiscoveryUrl = options.GetValue<string>("-u", "--url")
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Discover servers
        /// </summary>
        private async Task DiscoverServersAsync(CliOptions options) {
            IDiscoveryServiceEvents events = null;
            var id = options.GetValueOrDefault("-i", "--id", Guid.NewGuid().ToString());
            if (options.IsSet("-m", "--monitor")) {
                events = _scope.Resolve<IDiscoveryServiceEvents>();
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var discovery = await events.SubscribeDiscoveryProgressByRequestIdAsync(
                    id, async ev => {
                        await PrintProgress(ev).ConfigureAwait(false);
                        switch (ev.EventType) {
                            case DiscoveryProgressType.Error:
                            case DiscoveryProgressType.Cancelled:
                            case DiscoveryProgressType.Finished:
                                tcs.TrySetResult(true);
                                break;
                        }
                    }).ConfigureAwait(false);
                try {
                    await DiscoverServersAsync(options, id).ConfigureAwait(false);
                    await tcs.Task.ConfigureAwait(false); // For completion
                }
                finally {
                    await discovery.DisposeAsync().ConfigureAwait(false);
                }

            }
            else {
                await DiscoverServersAsync(options, id).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Discover servers
        /// </summary>
        private async Task DiscoverServersAsync(CliOptions options, string id) {
            await _registry.DiscoverAsync(
                new DiscoveryRequestApiModel {
                    Id = id,
                    Discovery = options.GetValueOrDefault("-d", "--discovery", DiscoveryMode.Fast),
                    Configuration = BuildDiscoveryConfig(options)
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancel discovery
        /// </summary>
        private async Task CancelDiscoveryAsync(CliOptions options) {
            await _registry.CancelAsync(
                new DiscoveryCancelApiModel {
                    Id = options.GetValue<string>("-i", "--id")
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update application
        /// </summary>
        private async Task UpdateApplicationAsync(CliOptions options) {
            await _registry.UpdateApplicationAsync(GetApplicationId(options),
                new ApplicationInfoUpdateApiModel {
                    GenerationId = options.GetValue<string>("-g", "--genid"),
                    ApplicationName = options.GetValueOrDefault<string>("-n", "--name", null),
                    GatewayServerUri = options.GetValueOrDefault<string>("-g", "--gwuri", null),
                    ProductUri = options.GetValueOrDefault<string>("-p", "--product", null),
                    DiscoveryProfileUri = options.GetValueOrDefault<string>("-r", "--dpuri", null)
                    // ...
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Unregister application
        /// </summary>
        private async Task UnregisterApplicationAsync(CliOptions options) {

            var id = GetApplicationId(options, false);
            if (id != null) {
                await _registry.UnregisterApplicationAsync(id,
                    options.GetValue<string>("-g", "--genid")).ConfigureAwait(false);
                return;
            }

            var query = new ApplicationInfoQueryApiModel {
                ApplicationUri = options.GetValueOrDefault<string>("-u", "--uri", null),
                ApplicationType = options.GetValueOrDefault<ApplicationType>("-t", "--type", null),
                ApplicationName = options.GetValueOrDefault<string>("-n", "--name", null),
                ProductUri = options.GetValueOrDefault<string>("-p", "--product", null),
                GatewayServerUri = options.GetValueOrDefault<string>("-g", "--gwuri", null),
                DiscoveryProfileUri = options.GetValueOrDefault<string>("-r", "--dpuri", null),
                Locale = options.GetValueOrDefault<string>("-l", "--locale", null)
            };

            // Unregister all applications
            var result = await _registry.QueryAllApplicationsAsync(query).ConfigureAwait(false);
            foreach (var item in result) {
                try {
                    await _registry.UnregisterApplicationAsync(item.ApplicationId,
                        item.GenerationId).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to unregister {item.ApplicationId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Purge disabled applications not seen since specified amount of time.
        /// </summary>
        private Task PurgeDisabledApplicationsAsync(CliOptions options) {
            return _registry.PurgeDisabledApplicationsAsync(
                options.GetValueOrDefault("-f", "--for", TimeSpan.Zero));
        }

        /// <summary>
        /// List applications
        /// </summary>
        private async Task ListApplicationsAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllApplicationsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListApplicationsAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query applications
        /// </summary>
        private async Task QueryApplicationsAsync(CliOptions options) {
            var query = new ApplicationInfoQueryApiModel {
                ApplicationUri = options.GetValueOrDefault<string>("-u", "--uri", null),
                ProductUri = options.GetValueOrDefault<string>("-p", "--product", null),
                GatewayServerUri = options.GetValueOrDefault<string>("-g", "--gwuri", null),
                DiscoveryProfileUri = options.GetValueOrDefault<string>("-r", "--dpuri", null),
                ApplicationType = options.GetValueOrDefault<ApplicationType>("-t", "--type", null),
                ApplicationName = options.GetValueOrDefault<string>("-n", "--name", null),
                Locale = options.GetValueOrDefault<string>("-l", "--locale", null),
                Visibility = options.GetValueOrDefault<EntityVisibility>("-v", "--visibility", null),
                DiscovererId = options.GetValueOrDefault<string>("-D", "--discovererId", null)
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.QueryAllApplicationsAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.QueryApplicationsAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Get application
        /// </summary>
        private async Task GetApplicationAsync(CliOptions options) {
            var result = await _registry.GetApplicationAsync(GetApplicationId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Monitor applications
        /// </summary>
        private async Task MonitorApplicationsAsync() {
            var events = _scope.Resolve<IDiscoveryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribeApplicationEventsAsync(PrintEvent).ConfigureAwait(false);
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
            var revents = _scope.Resolve<IDiscoveryServiceEvents>();
            var devents = _scope.Resolve<IRegistryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var apps = await revents.SubscribeApplicationEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                var endpoint = await revents.SubscribeEndpointEventsAsync(PrintEvent).ConfigureAwait(false);
                try {
                    var supervisor = await devents.SubscribeSupervisorEventsAsync(PrintEvent).ConfigureAwait(false);
                    try {
                        var publisher = await devents.SubscribePublisherEventsAsync(PrintEvent).ConfigureAwait(false);
                        try {
                            var discoverers = await devents.SubscribeDiscovererEventsAsync(PrintEvent).ConfigureAwait(false);
                            try {
                                var supervisors = await _directory.ListAllDiscoverersAsync().ConfigureAwait(false);
                                var discovery = await supervisors
                                    .Select(s => revents.SubscribeDiscoveryProgressByDiscovererIdAsync(
                                        s.Id, PrintProgress)).AsAsyncDisposable().ConfigureAwait(false);
                                try {
                                    Console.ReadKey();
                                }
                                finally {
                                    await discovery.DisposeAsync().ConfigureAwait(false);
                                }
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
                finally {
                    await endpoint.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally {
                await apps.DisposeAsync().ConfigureAwait(false);
            }
        }


        private string _endpointId;

        /// <summary>
        /// Get endpoint id
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetEndpointId(CliOptions options, bool shouldThrow = true) {
            var id = options.GetValueOrDefault<string>("-i", "--id", null);
            if (_endpointId != null) {
                if (id == null) {
                    return _endpointId;
                }
                _endpointId = null;
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
        /// Select endpoint registration
        /// </summary>
        private async Task SelectEndpointsAsync(CliOptions options) {
            if (options.IsSet("-c", "--clear")) {
                _endpointId = null;
            }
            else if (options.IsSet("-s", "--show")) {
                Console.WriteLine(_endpointId);
            }
            else {
                var endpointId = options.GetValueOrDefault<string>("-i", "--id", null);
                if (string.IsNullOrEmpty(endpointId)) {
                    var result = await _registry.ListAllEndpointsAsync().ConfigureAwait(false);
                    endpointId = ConsoleEx.Select(result.Select(r => r.Id));
                    if (string.IsNullOrEmpty(endpointId)) {
                        Console.WriteLine("Nothing selected - endpoint selection cleared.");
                    }
                    else {
                        Console.WriteLine($"Selected {endpointId}.");
                    }
                }
                _endpointId = endpointId;
            }
        }

        /// <summary>
        /// List endpoint registrations
        /// </summary>
        private async Task ListEndpointsAsync(CliOptions options) {
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.ListAllEndpointsAsync().ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.ListEndpointsAsync(
                    options.GetValueOrDefault<string>("-C", "--continuation", null),
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

        /// <summary>
        /// Query endpoints
        /// </summary>
        private async Task QueryEndpointsAsync(CliOptions options) {
            var query = new EndpointInfoQueryApiModel {
                Url = options.GetValueOrDefault<string>("-u", "--uri", null),
                SecurityMode = options
                    .GetValueOrDefault<Platform.Core.Api.Models.SecurityMode>("-m", "--mode", null),
                SecurityPolicy = options.GetValueOrDefault<string>("-l", "--policy", null),
                Visibility = options.GetValueOrDefault<EntityVisibility>("-v", "--visibility", null),
                ApplicationId = options.GetValueOrDefault<string>("-R", "--applicationId", null),
                DiscovererId = options.GetValueOrDefault<string>("-D", "--discovererId", null)
            };
            if (options.IsSet("-A", "--all")) {
                var result = await _registry.QueryAllEndpointsAsync(query).ConfigureAwait(false);
                PrintResult(options, result);
                Console.WriteLine($"{result.Count()} item(s) found...");
            }
            else {
                var result = await _registry.QueryEndpointsAsync(query,
                    options.GetValueOrDefault<int>("-P", "--page-size", null)).ConfigureAwait(false);
                PrintResult(options, result);
            }
        }

#if FALSE
        /// <summary>
        /// Activate endpoints
        /// </summary>
        private async Task ActivateEndpointsAsync(CliOptions options) {

            var id = GetEndpointId(options, false);
            if (id != null) {
                await _registry.ActivateEndpointAsync(id,
                    options.GetValueOrDefault<string>("-g", "--genid", null)).ConfigureAwait(false);
                return;
            }

            // Activate all sign and encrypt endpoints
            var result = await _registry.QueryAllEndpointsAsync(new EndpointInfoQueryApiModel {
                SecurityMode = options.GetValueOrDefault<SecurityMode>("-m", "mode", null),
                ActivationState = EntityActivationState.Deactivated
            }).ConfigureAwait(false);
            foreach (var item in result) {
                try {
                    await _registry.ActivateEndpointAsync(item.Id, item.GenerationId).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to activate {item.Id}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Deactivate endpoints
        /// </summary>
        private async Task DeactivateEndpointsAsync(CliOptions options) {

            var id = GetEndpointId(options, false);
            if (id != null) {
                await _registry.DeactivateEndpointAsync(id,
                    options.GetValueOrDefault<string>("-g", "--genid", null)).ConfigureAwait(false);
                return;
            }

            // Activate all sign and encrypt endpoints
            var result = await _registry.QueryAllEndpointsAsync(new EndpointInfoQueryApiModel {
                SecurityMode = options.GetValueOrDefault<SecurityMode>("-m", "mode", null),
                ActivationState = EntityActivationState.Activated
            }).ConfigureAwait(false);
            foreach (var item in result) {
                try {
                    await _registry.DeactivateEndpointAsync(item.Id, item.GenerationId).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to deactivate {item.Id}: {ex.Message}");
                }
            }
        }
#endif

        /// <summary>
        /// Get endpoint
        /// </summary>
        private async Task GetEndpointAsync(CliOptions options) {
            var result = await _registry.GetEndpointAsync(GetEndpointId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Get endpoint certificate
        /// </summary>
        private async Task GetEndpointCertificateAsync(CliOptions options) {
            var result = await _registry.GetEndpointCertificateAsync(GetEndpointId(options)).ConfigureAwait(false);
            PrintResult(options, result);
        }

        /// <summary>
        /// Monitor endpoints
        /// </summary>
        private async Task MonitorEndpointsAsync() {
            var events = _scope.Resolve<IDiscoveryServiceEvents>();
            Console.WriteLine("Press any key to stop.");
            var complete = await events.SubscribeEndpointEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                Console.ReadKey();
            }
            finally {
                await complete.DisposeAsync().ConfigureAwait(false);
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
            Console.WriteLine("Twin:      " + await _twin.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("Registry:  " + await _registry.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("Publisher: " + await _publisher.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("Vault:     " + await _vault.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("History:   " + await _history.GetServiceStatusAsync().ConfigureAwait(false));
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
        /// Print progress
        /// </summary>
        private static Task PrintProgress(DiscoveryProgressApiModel ev) {
            switch (ev.EventType) {
                case DiscoveryProgressType.Pending:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Total} waiting...");
                    break;
                case DiscoveryProgressType.Started:
                    Console.WriteLine($"{ev.DiscovererId}: Started.");
                    Console.WriteLine("==========================================");
                    break;
                case DiscoveryProgressType.NetworkScanStarted:
                    Console.WriteLine($"{ev.DiscovererId}: Scanning network...");
                    Console.WriteLine("==========================================");
                    break;
                case DiscoveryProgressType.NetworkScanResult:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" {ev.Discovered} addresses found - NEW: {ev.Result}...");
                    break;
                case DiscoveryProgressType.NetworkScanProgress:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" {ev.Discovered} addresses found");
                    break;
                case DiscoveryProgressType.NetworkScanFinished:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" {ev.Discovered} addresses found - complete!");
                    break;
                case DiscoveryProgressType.PortScanStarted:
                    Console.WriteLine($"{ev.DiscovererId}: Scanning ports...");
                    Console.WriteLine("==========================================");
                    break;
                case DiscoveryProgressType.PortScanResult:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" {ev.Discovered} ports found - NEW: {ev.Result}...");
                    break;
                case DiscoveryProgressType.PortScanProgress:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" {ev.Discovered} ports found");
                    break;
                case DiscoveryProgressType.PortScanFinished:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" {ev.Discovered} ports found - complete!");
                    break;
                case DiscoveryProgressType.ServerDiscoveryStarted:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" Finding servers...");
                    Console.WriteLine("==========================================");
                    break;
                case DiscoveryProgressType.EndpointsDiscoveryStarted:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" ... {ev.Discovered} servers found - find " +
                        $"endpoints on {ev.RequestDetails["url"]}...");
                    break;
                case DiscoveryProgressType.EndpointsDiscoveryFinished:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" ... {ev.Discovered} servers found - {ev.Result} " +
                        $"endpoints found on {ev.RequestDetails["url"]}...");
                    break;
                case DiscoveryProgressType.ServerDiscoveryFinished:
                    Console.WriteLine($"{ev.DiscovererId}: {ev.Progress}/{ev.Total} :" +
                        $" ... {ev.Discovered} servers found.");
                    break;
                case DiscoveryProgressType.Cancelled:
                    Console.WriteLine($"{ev.DiscovererId}: Cancelled.");
                    Console.WriteLine("==========================================");
                    break;
                case DiscoveryProgressType.Error:
                    Console.WriteLine($"{ev.DiscovererId}: Failure: {ev.Result}");
                    Console.WriteLine("==========================================");
                    break;
                case DiscoveryProgressType.Finished:
                    Console.WriteLine($"{ev.DiscovererId}: Completed.");
                    Console.WriteLine("==========================================");
                    break;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(WriterGroupEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(DataSetWriterEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(EndpointEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Print event
        /// </summary>
        private Task PrintEvent(ApplicationEventApiModel ev) {
            Console.WriteLine(_serializer.SerializePretty(ev));
            return Task.CompletedTask;
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
        /// Print sample
        /// </summary>
        private Task PrintSample(PublishedDataSetItemMessageApiModel samples) {
            Console.WriteLine(_serializer.SerializeToString(samples));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Build message settings
        /// </summary>
        private static WriterGroupMessageSettingsApiModel BuildWriterGroupMessageSettings(
            CliOptions options) {
            var config = new WriterGroupMessageSettingsApiModel();
            var empty = true;

            var groupVersion = options.GetValueOrDefault<uint>("-V", "--version", null);
            if (groupVersion != null && groupVersion != 0) {
                config.GroupVersion = groupVersion;
                empty = false;
            }
            var offset = options.GetValueOrDefault<double>("-S", "--offset", null);
            if (offset != null && offset != 0) {
                config.SamplingOffset = offset;
                empty = false;
            }

            var mask = options.GetValueOrDefault<NetworkMessageContentMask?>("-C", "--content", null);
            if (mask != null && mask != 0) {
                config.NetworkMessageContentMask = mask;
                empty = false;
            }
            var order = options.GetValueOrDefault<DataSetOrderingType?>("-O", "--order", null);
            if (order != null && order != 0) {
                config.DataSetOrdering = order;
                empty = false;
            }
            return empty ? null : config;
        }

        /// <summary>
        /// Build subscription settings
        /// </summary>
        private static PublishedDataSetSourceSettingsApiModel BuildDataSetWriterSubscriptionSettings(
            CliOptions options) {
            var config = new PublishedDataSetSourceSettingsApiModel();
            var empty = true;

            var resolve = options.GetValueOrDefault<bool>("-R", "--resolve", null);
            if (resolve != null) {
                config.ResolveDisplayName = resolve;
                empty = false;
            }

            var prio = options.GetValueOrDefault<byte>("-P", "--priority", null);
            if (prio != null) {
                config.Priority = prio;
                empty = false;
            }
            var keepAlive = options.GetValueOrDefault<uint>("-K", "--kacount", null);
            if (keepAlive != null) {
                config.MaxKeepAliveCount = keepAlive;
                empty = false;
            }
            var count = options.GetValueOrDefault<uint>("-L", "--lifetime", null);
            if (count != null) {
                config.LifeTimeCount = count;
                empty = false;
            }
            var maxPublish = options.GetValueOrDefault<uint>("-M", "--maxnotif", null);
            if (maxPublish != null) {
                config.MaxNotificationsPerPublish = maxPublish;
                empty = false;
            }
            var publish = options.GetValueOrDefault<TimeSpan>("-p", "--publish", null);
            if (publish != null) {
                config.PublishingInterval = publish;
                empty = false;
            }
            return empty ? null : config;
        }

        /// <summary>
        /// Build message settings
        /// </summary>
        private static DataSetWriterMessageSettingsApiModel BuildDataSetWriterMessageSettings(
            CliOptions options) {
            var config = new DataSetWriterMessageSettingsApiModel();
            var empty = true;
            var messageNumber = options.GetValueOrDefault<ushort>("-N", "--number", null);
            if (messageNumber != null && messageNumber != 0) {
                config.NetworkMessageNumber = messageNumber;
                empty = false;
            }
            var size = options.GetValueOrDefault<ushort>("-s", "--size", null);
            if (size != null && size != 0) {
                config.ConfiguredSize = size;
                empty = false;
            }
            var mask = options.GetValueOrDefault<DataSetContentMask?>("-d", "--dataset", null);
            if (mask != null && mask != 0) {
                config.DataSetMessageContentMask = mask;
                empty = false;
            }
            var offset = options.GetValueOrDefault<ushort>("-o", "--offset", null);
            if (offset != null && offset != 0) {
                config.DataSetOffset = offset;
                empty = false;
            }
            return empty ? null : config;
        }

        /// <summary>
        /// Build discovery config model from options
        /// </summary>
        private static DiscoveryConfigApiModel BuildDiscoveryConfig(CliOptions options) {
            var config = new DiscoveryConfigApiModel();
            var empty = true;

            var addressRange = options.GetValueOrDefault<string>("-r", "--address-ranges", null);
            if (addressRange != null) {
                if (addressRange == "true") {
                    config.AddressRangesToScan = "";
                }
                else {
                    config.AddressRangesToScan = addressRange;
                }
                empty = false;
            }

            var portRange = options.GetValueOrDefault<string>("-p", "--port-ranges", null);
            if (portRange != null) {
                if (portRange == "true") {
                    config.PortRangesToScan = "";
                }
                else {
                    config.PortRangesToScan = portRange;
                }
                empty = false;
            }

            var netProbes = options.GetValueOrDefault<int>("-R", "--address-probes", null);
            if (netProbes != null && netProbes != 0) {
                config.MaxNetworkProbes = netProbes;
                empty = false;
            }

            var portProbes = options.GetValueOrDefault<int>("-P", "--port-probes", null);
            if (portProbes != null) {
                config.MaxPortProbes = portProbes;
                empty = false;
            }

            var netProbeTimeout = options.GetValueOrDefault<int>("-T", "--address-probe-timeout", null);
            if (netProbeTimeout != null) {
                config.NetworkProbeTimeout = TimeSpan.FromMilliseconds(netProbeTimeout.Value);
                empty = false;
            }

            var portProbeTimeout = options.GetValueOrDefault<int>("-t", "--port-probe-timeout", null);
            if (portProbeTimeout != null) {
                config.PortProbeTimeout = TimeSpan.FromMilliseconds(portProbeTimeout.Value);
                empty = false;
            }

            var idleTime = options.GetValueOrDefault<int>("-I", "--idle-time", null);
            if (idleTime != null) {
                config.IdleTimeBetweenScans = TimeSpan.FromSeconds(idleTime.Value);
                empty = false;
            }
            return empty ? null : config;
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

     apps        Manage applications
     endpoints   Manage endpoints
     nodes       Call twin module services on endpoint

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
        private static void PrintApplicationsHelp() {
            Console.WriteLine(
                @"
Manage applications registry.

Commands and Options

     select      Select application as -i/--id argument in all calls.
        with ...
        -i, --id        Application id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to applications.

     list        List applications
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all application infos (unpaged)
        -F, --format    Json format for result

     add         Register server and endpoints through discovery url
        with ...
        -i, --id        Request id for the discovery request.
        -u, --url       Url of the discovery endpoint (mandatory)
        -m, --monitor   Monitor the discovery process to completion.

     discover    Discover applications and endpoints through config.
        with ...
        -i, --id        Request id for the discovery request.
        -d, --discovery Set discovery mode to use
        -m, --monitor   Monitor the discovery process to completion.

     cancel      Cancel application discovery.
        with ...
        -i, --id        Request id of the discovery request (mandatory).

     register    Manually register Application
        with ...
        -u, --url       Uri of the application (mandatory)
        -n  --name      Application name of the application
        -t, --type      Application type (default to Server)
        -p, --product   Product uri of the application
        -d, --discovery Url of the discovery endpoint
        -r, --dpuri     Discovery profile uri
        -g, --gwuri     Gateway uri
        -F, --format    Json format for result

     query       Find applications
        with ...
        -P, --page-size Size of page
        -A, --all       Return all application infos (unpaged)
        -u, --uri       Application uri of the application.
        -r, --dpuri     Discovery profile uri
        -g, --gwuri     Gateway uri
        -n  --name      Application name of the application
        -t, --type      Application type (default to all)
        -s, --state     Application state (default to all)
        -p, --product   Product uri of the application
        -v, --visibility
                        Visibility state of the application..
        -D  --discovererId
                        Onboarded from specified discoverer.
        -F, --format    Json format for result

     get         Get application
        with ...
        -i, --id        Id of application to get (mandatory)
        -F, --format    Json format for result

     update      Update application
        with ...
        -i, --id        Id of application to update (mandatory)
        -n, --name      Application name
        -r, --dpuri     Discovery profile uri
        -g, --gwuri     Gateway uri
        -p, --product   Product uri of the application

     unregister  Unregister application
        with ...
        -i, --id        Id of application to unregister
                        -or- all matching
        -u, --uri       Application uri and/or
        -n  --name      Application name and/or
        -t, --type      Application type and/or
        -p, --product   Product uri and/or
        -r, --dpuri     Discovery profile uri
        -g, --gwuri     Gateway uri
        -s, --state     Application state (default to all)

     purge       Purge applications not seen ...
        with ...
        -f, --for       ... a specified amount of time (mandatory)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintEndpointsHelp() {
            Console.WriteLine(
                @"
Manage endpoints in registry.

Commands and Options

     select      Select endpoint as -i/--id argument in all calls.
        with ...
        -i, --id        Endpoint id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to endpoints.

     list        List endpoints
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all endpoints (unpaged)
        -F, --format    Json format for result

     query       Find endpoints
        -u, --uri       Endpoint uri to seach for
        -m, --mode      Security mode to search for
        -p, --policy    Security policy to match
        -s, --state     Only return endpoints with specified state.
        -v, --visibility
                        Visibility state of the endpoint.
        -R  --applicationId
                        Return endpoints for specified Application.
        -D  --discovererId
                        Onboarded from specified discoverer.
        -P, --page-size Size of page
        -A, --all       Return all endpoints (unpaged)
        -F, --format    Json format for result

     get         Get endpoint
        with ...
        -i, --id        Id of endpoint to retrieve (mandatory)
        -F, --format    Json format for result

     validate    Get endpoint certificate chain and validate
        with ...
        -i, --id        Id of endpoint to retrieve (mandatory)
        -F, --format    Json format for result

     activate    Activate endpoints
        with ...
        -i, --id        Id of endpoint or ...
        -m, --mode      Security mode (default:SignAndEncrypt)

     deactivate  Deactivate endpoints
        with ...
        -i, --id        Id of endpoint or ...
        -m, --mode      Security mode (default:SignAndEncrypt)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintTwinsHelp() {
            Console.WriteLine(
                @"
Manage twins.

Commands and Options

     select      Select twin as -i/--id argument in all calls.
        with ...
        -i, --id        Endpoint id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     monitor     Monitor changes to twins.

     list        List twins
        with ...
        -C, --continuation
                        Continuation from previous result.
        -P, --page-size Size of page
        -A, --all       Return all twins (unpaged)
        -F, --format    Json format for result

     query       Find twins
        -s, --state     Only return twins with specified state.
        -e  --endpointId
                        Return twins for specified Endpoint.
        -P, --page-size Size of page
        -A, --all       Return all twins (unpaged)
        -F, --format    Json format for result

     get         Get twin
        with ...
        -i, --id        Id of twin to retrieve (mandatory)
        -F, --format    Json format for result

     activate    Activate twins
        with ...
        -i, --id        Id of endpoint

     deactivate  Deactivate twins
        with ...
        -i, --id        Id of twin

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintNodesHelp() {
            Console.WriteLine(
                @"
Access address space through configured server endpoint.

Commands and Options

     select      Select node id as -n/--nodeid argument in all calls.
        with ...
        -n, --nodeId    Node id to select.
        -i, --id        Endpoint id to use for selection if browsing.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     browse      Browse nodes on endpoint
        with ...
        -i, --id        Id of endpoint to browse (mandatory)
        -n, --nodeid    Node to browse
        -x, --maxrefs   Max number of references
        -d, --direction Browse direction (Forward, Backward, Both)
        -r, --recursive Browse recursively and read node values
        -v, --readvalue Read node values in browse
        -t, --targets   Only return target nodes
        -s, --silent    Only show errors
        -F, --format    Json format for result

     next        Browse next nodes
        with ...
        -C, --continuation
                        Continuation from previous result.
        -F, --format    Json format for result

     read        Read node value on endpoint
        with ...
        -i, --id        Id of endpoint to read value from (mandatory)
        -n, --nodeid    Node to read value from (mandatory)
        -F, --format    Json format for result

     write       Write node value on endpoint
        with ...
        -i, --id        Id of endpoint to write value on (mandatory)
        -n, --nodeid    Node to write value to (mandatory)
        -t, --datatype  Datatype of value (mandatory)
        -v, --value     Value to write (mandatory)

     metadata    Get Call meta data
        with ...
        -i, --id        Id of endpoint with meta data (mandatory)
        -n, --nodeid    Method Node to get meta data for (mandatory)
        -F, --format    Json format for result

     call        Call method node on endpoint
        with ...
        -i, --id        Id of endpoint to call method on (mandatory)
        -n, --nodeid    Method Node to call (mandatory)
        -o, --objectid  Object context for method

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
        private static void PrintWriterGroupsHelp() {
            Console.WriteLine(
                @"
Manage dataset writer groups

Commands and Options

     select      Select group as -i/--id argument in all calls.
        with ...
        -i, --id        Group id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     list        List writer groups
        with ...
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     query       Find writer groups
        with ...
        -n, --name      Name of the group
        -e, --encoding  Message encoding
        -P, --priority  Group priority
        -V, --version   Group version
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     create      Create new writer group
        with ...
        -n, --name      Name of the group
        -b, --batchsize Batch size
        -p, --publish   Publishing interval
        -h, --header    Network message header uri
        -k, --keepalive Keep alive interval
        -e, --encoding  Message encoding
        -P, --priority  Group priority
        -V, --version   Group version
        -S, --offset    Sampling offset
        -C, --content   Network message content mask
        -O, --order     Dataset order

        -F, --format    Json format for result

     get         Get writer group
        with ...
        -i, --id        Id of group for renewal (mandatory)

     update      Update writer group information
        with ...
        -i, --id        Id of the group to update (mandatory)
        -g, --genid     Generation of group to update (mandatory)
        -n, --name      Name of the group
        -b, --batchsize Batch size
        -p, --publish   Publishing interval
        -h, --header    Network message header uri
        -k, --keepalive Keep alive interval
        -e, --encoding  Message encoding
        -P, --priority  Group priority
        -V, --version   Group version
        -S, --offset    Sampling offset
        -C, --content   Network message content mask
        -O, --order     Dataset order

     delete      Delete writer group
        with ...
        -i, --id        Id of group to delete (mandatory)
        -g, --genid     Generation of group to delete (mandatory)

     monitor     Monitor writer groups

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintDataSetWritersHelp() {
            Console.WriteLine(
                @"
Manage dataset and dataset writers

Commands and Options

     select      Select writer as -i/--id argument in all calls.
        with ...
        -i, --id        Group id to select.
        -c, --clear     Clear current selection
        -s, --show      Show current selection

     list        List dataset writers
        with ...
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     query       Find dataset writers
        with ...
        -g, --group     Writer group to search in
        -n, --name      Dataset name to look for
        -e, --endpoint  Endpoint id of writer
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     create      Create new dataset
        with ...
        -e, --endpoint  Endpoint id for the writer (mandatory)
        -g, --group     Group the writer should be part of (or default)
        -n, --name      Dataset name
        -p, --publish   Publishing interval
        -R, --resolve   Resolve display name
        -N, --number    Network message number
        -s, --size      Configured size
        -d, --dataset   Dataset message content mask
        -o, --offset    Dataset offset
        -f, --interval  Keyframe interval
        -P, --priority  Dataset priority
        -K, --kacount   Keep alive counter
        -L, --lifetime  Lifetime count
        -M, --maxnotif  Max notifications per publish
        -F, --format    Json format for result

     get         Get dataset
        with ...
        -i, --id        Id of dataset writer (mandatory)

     update      Update dataset information
        with ...
        -i, --id        Id of the dataset writer to update (mandatory)
        -g, --genid     Generation of writer to update (mandatory)
        -g, --group     Group writer should be part of
        -n, --name      Dataset name
        -p, --publish   Publishing interval
        -R, --resolve   Resolve display name
        -N, --number    Network message number
        -s, --size      Configured size
        -d, --dataset   Dataset message content mask
        -o, --offset    Dataset offset
        -f, --interval  Keyframe interval
        -P, --priority  Dataset priority
        -K, --kacount   Keep alive counter
        -L, --lifetime  Lifetime count
        -M, --maxnotif  Max notifications per publish

     delete      Delete dataset
        with ...
        -i, --id        Id of dataset writer to delete (mandatory)
        -g, --genid     Generation of group to delete (mandatory)

     monitor     Monitor dataset writers

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintEventDataSetHelp() {
            Console.WriteLine(
                @"
Manage dataset event definitions

Commands and Options

     add         Create new event definition in empty dataset writer
        with ...
        -i, --id        Dataset writer to define event for (mandatory)
        -n, --notifier  Event Notifier
        -p, --path      Browse path
        -D, --discard   Discard new
        -q, --queue     Queue size
        -m, --mode      Monitoring mode
        -t, --triggerid Trigger id
        -F, --format    Json format for result

     update      Update dataset event definition
        with ...
        -i, --id        Id of the dataset writer to update (mandatory)
        -g, --genid     Generation of writer to update (mandatory)
        -D, --discard   Discard new
        -q, --queue     Queue size
        -m, --mode      Monitoring mode
        -t, --triggerid Trigger id

     get         Get dataset event definition
        with ...
        -i, --id        Dataset writer with event definition (mandatory)
        -F, --format    Json format for result

     remove      Remove dataset event definition
        with ...
        -i, --id        Dataset writer id (mandatory)
        -g, --genid     Generation of variable to remove (mandatory)

     data        Monitor event
        with ...
        -i, --id        Dataset writer id with event to monitor (mandatory)

     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintDataSetVariablesHelp() {
            Console.WriteLine(
                @"
Manage dataset variables

Commands and Options

     list        List dataset variables
        with ...
        -i, --id        Dataset writer (mandator)
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     query       Find dataset variables
        with ...
        -i, --id        Dataset writer (mandatory)
        -a, --attribute Node attribute to search for
        -d, --name      Display name to look for
        -n, --nodeId    Node Id to look for
        -C, --continuation
                        Continuation from previous result.
        -A, --all       Return all items (unpaged)
        -F, --format    Json format for result

     add         Create new variable in dataset writer
        with ...
        -i, --id        Dataset writer (mandatory)
        -a, --attribute Node attribute to search for
        -d, --name      Display name to look for
        -n, --nodeId    Node Id to look for
        -D, --discard   Discard new
        -f, --filter    Data change filter
        -q, --queue     Queue size
        -B, --dbtype    Deadband type
        -b, --deadband  Deadband value
        -h, --heartbeat Heartbeat value
        -s, --sampling  Sampling interval
        -o, --order     Order
        -m, --mode      Monitoring mode
        -t, --triggerid Trigger id
        -r, --range     Index range
        -F, --format    Json format for result

     update      Update dataset variable information
        with ...
        -i, --id        Id of the dataset writer to update (mandatory)
        -v, --variable  Variable id (mandatory)
        -g, --genid     Generation of writer to update (mandatory)
        -d, --name      Display name to look for
        -D, --discard   Discard new
        -f, --filter    Data change filter
        -q, --queue     Queue size
        -B, --dbtype    Deadband type
        -b, --deadband  Deadband value
        -h, --heartbeat Heartbeat value
        -s, --sampling  Sampling interval
        -m, --mode      Monitoring mode
        -t, --triggerid Trigger id

     remove      Remove variable
        with ...
        -i, --id        Dataset writer id (mandatory)
        -v, --variable  Variable id (mandatory)
        -g, --genid     Generation of variable to remove (mandatory)

     data        Monitor variable
        with ...
        -i, --id        Dataset writer id (mandatory)
        -v, --variable  Variable id to monitor (mandatory)

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
        private readonly ITwinServiceApi _twin;
        private readonly IRegistryServiceApi _directory;
        private readonly IDiscoveryServiceApi _registry;
        private readonly IPublisherServiceApi _publisher;
        private readonly IHistoryServiceApi _history;
        private readonly IVaultServiceApi _vault;
        private readonly IMetricServer _metrics;
        private readonly IJsonSerializer _serializer;
    }
}
