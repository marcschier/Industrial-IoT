// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Api.Cli {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.IIoT.Extensions.Http.SignalR.Clients;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Authentication.Runtime;
    using Microsoft.IIoT.Extensions.Serializers;
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
            builder.RegisterType<AadApiClientConfig>()
                .AsImplementedInterfaces();
            // Use bearer authentication
            builder.RegisterModule<NativeClientAuthentication>();

            // Register logger
            builder.AddDiagnostics(builder => builder.AddDebug());
            if (useMsgPack) {
                builder.RegisterModule<MessagePackModule>();
            }

            // SignalR client (needed for api)
            builder.RegisterType<SignalRHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.AddOpcUa();
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
            _registry = _scope.Resolve<IDiscoveryServiceApi>();
            _history = _scope.Resolve<IHistoryServiceApi>();
            _publisher = _scope.Resolve<IPublisherServiceApi>();
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
            Console.WriteLine("Press any key to stop.");
            var apps = await revents.SubscribeApplicationEventsAsync(PrintEvent).ConfigureAwait(false);
            try {
                var endpoint = await revents.SubscribeEndpointEventsAsync(PrintEvent).ConfigureAwait(false);
                try {
                    Console.ReadKey();
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
                    .GetValueOrDefault<Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models.SecurityMode>("-m", "--mode", null),
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

        /// <summary>
        /// Get status
        /// </summary>
        private async Task GetStatusAsync() {
            Console.WriteLine("Twin:      " + await _twin.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("Registry:  " + await _registry.GetServiceStatusAsync().ConfigureAwait(false));
            Console.WriteLine("Publisher: " + await _publisher.GetServiceStatusAsync().ConfigureAwait(false));
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

        private readonly ILifetimeScope _scope;
        private readonly ITwinServiceApi _twin;
        private readonly IDiscoveryServiceApi _registry;
        private readonly IPublisherServiceApi _publisher;
        private readonly IHistoryServiceApi _history;
        private readonly IMetricServer _metrics;
        private readonly IJsonSerializer _serializer;
    }
}
