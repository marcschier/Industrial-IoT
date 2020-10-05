// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Test.Scenarios.Cli {
    using Microsoft.Azure.IIoT.Api.Runtime;
    using Microsoft.Azure.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Api;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Clients;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Vault.Api;
    using Microsoft.Azure.IIoT.Platform.Vault.Api.Clients;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Http.SignalR;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Authentication.Runtime;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
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

            var config = new ApiConfig(configuration);

            // Register configuration interfaces and logger
            builder.RegisterInstance(config)
                .AsImplementedInterfaces();
            builder.RegisterInstance(config.Configuration)
                .AsImplementedInterfaces();

            // Configure aad
            builder.RegisterType<AadApiClientConfig>()
                .AsImplementedInterfaces();

            // Register logger
            builder.AddDebugDiagnostics(addConsole: false);
            builder.RegisterModule<NewtonSoftJsonModule>();
            if (useMsgPack) {
                builder.RegisterModule<MessagePackModule>();
            }

            // Register http client module ...
            builder.RegisterModule<HttpClientModule>();
            // ... as well as signalR client (needed for api)
            builder.RegisterType<SignalRHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            // Use default token sources
            builder.RegisterModule<NativeClientAuthentication>();

            // Register twin, vault, and registry services clients
            builder.RegisterType<TwinServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<RegistryServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<VaultServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherServiceClient>()
                .AsImplementedInterfaces();

            // ... with client event callbacks
            builder.RegisterType<RegistryServiceEvents>()
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
                .AddFromKeyVault(ConfigurationProviderPriority.Lowest, true)
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
            _registry = _scope.Resolve<IRegistryServiceApi>();
            _publisher = _scope.Resolve<IPublisherServiceApi>();
            _vault = _scope.Resolve<IVaultServiceApi>();
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
        /// Run client
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
  ____                            _           _____         _
 / ___|  ___ ___ _ __   __ _ _ __(_) ___     |_   _|__  ___| |_ ___
 \___ \ / __/ _ \ '_ \ / _` | '__| |/ _ \ _____| |/ _ \/ __| __/ __|
  ___) | (_|  __/ | | | (_| | |  | | (_) |_____| |  __/\__ \ |_\__ \
 |____/ \___\___|_| |_|\__,_|_|  |_|\___/      |_|\___||___/\__|___/
");
                            interactive = true;
                            break;
                        case "activation":
                            options = new CliOptions(args, 1);
                            await TestActivationAsync(options).ConfigureAwait(false);
                            break;
                        case "browse":
                            options = new CliOptions(args, 1);
                            await TestBrowseAsync(options).ConfigureAwait(false);
                            break;
                        case "publish":
                            options = new CliOptions(args, 1);
                            await TestPublishAsync(options).ConfigureAwait(false);
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

        /// <summary>
        /// Test activation and deactivation
        /// </summary>
        private async Task TestActivationAsync(CliOptions options) {
            IEnumerable<EndpointInfoApiModel> endpoints;
            if (!options.IsSet("-a", "--all")) {
                if (options.IsSet("-e", "--endpoint")) {
                    var id = await SelectEndpointAsync().ConfigureAwait(false);
                    var ep = await _registry.GetEndpointAsync(id).ConfigureAwait(false);
                    endpoints = ep.YieldReturn();
                }
                else {
                    var id = options.GetValueOrDefault<string>("-i", "--id", null);
                    if (id == null) {
                        id = await SelectApplicationAsync().ConfigureAwait(false);
                        if (id == null) {
                            throw new ArgumentException("Needs an id");
                        }
                    }

                    var app = await _registry.GetApplicationAsync(id).ConfigureAwait(false);
                    if (app.Endpoints.Count == 0) {
                        return;
                    }
                    endpoints = app.Endpoints;
                }
            }
            else {
                endpoints = await _registry.ListAllEndpointsAsync().ConfigureAwait(false);
            }
            await Task.WhenAll(endpoints.Select(e => TestActivationAsync(e, options))).ConfigureAwait(false);
            Console.WriteLine("Success!");
        }

        /// <summary>
        /// Test browsing
        /// </summary>
        private async Task TestBrowseAsync(CliOptions options) {
            IEnumerable<EndpointInfoApiModel> endpoints;
            if (!options.IsSet("-a", "--all")) {
                if (options.IsSet("-e", "--endpoint")) {
                    var id = await SelectEndpointAsync().ConfigureAwait(false);
                    var ep = await _registry.GetEndpointAsync(id).ConfigureAwait(false);
                    endpoints = ep.YieldReturn();
                }
                else {
                    var id = options.GetValueOrDefault<string>("-i", "--id", null);
                    if (id == null) {
                        id = await SelectApplicationAsync().ConfigureAwait(false);
                        if (id == null) {
                            throw new ArgumentException("Needs an id");
                        }
                    }

                    var app = await _registry.GetApplicationAsync(id).ConfigureAwait(false);
                    if (app.Endpoints.Count == 0) {
                        return;
                    }
                    endpoints = app.Endpoints;
                }
            }
            else {
                endpoints = await _registry.ListAllEndpointsAsync().ConfigureAwait(false);
            }
            await Task.WhenAll(endpoints.Select(e => TestBrowseAsync(e, options))).ConfigureAwait(false);
            Console.WriteLine("Success!");
        }

        /// <summary>
        /// Test publishing
        /// </summary>
        private async Task TestPublishAsync(CliOptions options) {
            IEnumerable<EndpointInfoApiModel> endpoints;
            if (!options.IsSet("-a", "--all")) {
                if (options.IsSet("-e", "--endpoint")) {
                    var id = await SelectEndpointAsync().ConfigureAwait(false);
                    var ep = await _registry.GetEndpointAsync(id).ConfigureAwait(false);
                    endpoints = ep.YieldReturn();
                }
                else {
                    var id = options.GetValueOrDefault<string>("-i", "--id", null);
                    if (id == null) {
                        id = await SelectApplicationAsync().ConfigureAwait(false);
                        if (id == null) {
                            throw new ArgumentException("Needs an id");
                        }
                    }

                    var app = await _registry.GetApplicationAsync(id).ConfigureAwait(false);
                    if (app.Endpoints.Count == 0) {
                        return;
                    }
                    endpoints = app.Endpoints;
                }
            }
            else {
                endpoints = await _registry.ListAllEndpointsAsync().ConfigureAwait(false);
            }
            await Task.WhenAll(endpoints.Select(e => TestPublishAsync(e, options))).ConfigureAwait(false);
            Console.WriteLine("Success!");
        }

        /// <summary>
        /// Test activation of endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private async Task TestActivationAsync(EndpointInfoApiModel endpoint,
            CliOptions options) {
            EndpointInfoApiModel ep;
            var repeats = options.GetValueOrDefault("-r", "--repeat", 10); // 10 times
            for (var i = 0; i < repeats; i++) {
                await _registry.ActivateEndpointAsync(endpoint.Id).ConfigureAwait(false);
                var sw = Stopwatch.StartNew();
                while (true) {
                    ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                    if (ep.ActivationState == EntityActivationState.ActivatedAndConnected) {
                        break;
                    }
                    if (sw.ElapsedMilliseconds > 60000) {
                        throw new Exception($"{endpoint.Id} failed to activate!");
                    }
                }

                Console.WriteLine($"{endpoint.Id} activated.");

                while (options.IsSet("-b", "--browse") || options.IsSet("-w", "--waitstate")) {
                    if (ep.EndpointState != EndpointConnectivityState.Connecting) {
                        Console.WriteLine($"{endpoint.Id} now in {ep.EndpointState} state.");
                        break;
                    }
                    if (sw.ElapsedMilliseconds > 60000) {
                        throw new Exception($"{endpoint.Id} failed to get endpoint state!");
                    }
                    ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                }
                if (ep.EndpointState == EndpointConnectivityState.Ready &&
                    options.IsSet("-b", "--browse")) {

                    var silent = !options.IsSet("-V", "--verbose");
                    var recursive = options.IsSet("-R", "--recursive");
                    var readDuringBrowse = options.IsProvidedOrNull("-v", "--readvalue");
                    var node = options.GetValueOrDefault<string>("-n", "--nodeid", null);
                    var targetNodesOnly = options.IsProvidedOrNull("-t", "--targets");
                    var maxReferencesToReturn = options.GetValueOrDefault<uint>("-x", "--maxrefs", null);
                    var direction = options.GetValueOrDefault<BrowseDirection>("-d", "--direction", null);

                    await BrowseAsync(0, endpoint.Id, silent, recursive, readDuringBrowse, node,
                        targetNodesOnly, maxReferencesToReturn, direction, options).ConfigureAwait(false);
                }
                else {
                    await Task.Delay(_rand.Next(
                        options.GetValueOrDefault("-l", "--min-wait", 1000), // 1 seconds
                        options.GetValueOrDefault("-h", "--max-wait", 20000))).ConfigureAwait(false);  // 20 seconds
                }

                await _registry.DeactivateEndpointAsync(endpoint.Id).ConfigureAwait(false);
                sw.Restart();
                while (true) {
                    ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                    if (ep.ActivationState == EntityActivationState.Deactivated) {
                        break;
                    }
                    if (sw.ElapsedMilliseconds > 60000) {
                        throw new Exception($"{endpoint.Id} failed to deactivate!");
                    }
                }
                Console.WriteLine($"{endpoint.Id} deactivated.");
            }
        }

        /// <summary>
        /// Test publish and unpublish
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private async Task TestPublishAsync(EndpointInfoApiModel endpoint,
            CliOptions options) {
            EndpointInfoApiModel ep;

            Console.WriteLine($"Activating {endpoint.Id} for publishing ...");
            await _registry.ActivateEndpointAsync(endpoint.Id, endpoint.GenerationId).ConfigureAwait(false);
            var sw = Stopwatch.StartNew();
            while (true) {
                ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                if (ep.ActivationState == EntityActivationState.ActivatedAndConnected &&
                    ep.EndpointState == EndpointConnectivityState.Ready) {
                    break;
                }
                if (sw.ElapsedMilliseconds > 60000) {
                    Console.WriteLine($"{endpoint.Id} could not be activated - skip!");
                    return;
                }
            }
            Console.WriteLine($"{endpoint.Id} activated - get all variables.");

            var nodes = new List<string>();
            await BrowseAsync(0, endpoint.Id, true, true, false, null,
                true, 1000, null, options, nodes).ConfigureAwait(false);

            Console.WriteLine($"{endpoint.Id} has {nodes.Count} variables.");
            sw.Restart();
            await _twin.NodePublishBulkAsync(endpoint.Id, new PublishBulkRequestApiModel {
                NodesToAdd = nodes.Select(n => new PublishedItemApiModel {
                    NodeId = n
                }).ToList()
            }).ConfigureAwait(false);
            Console.WriteLine($"{endpoint.Id} Publishing {nodes.Count} variables took {sw.Elapsed}.");

            sw.Restart();
            await _twin.NodePublishBulkAsync(endpoint.Id, new PublishBulkRequestApiModel {
                NodesToRemove = nodes.ToList()
            }).ConfigureAwait(false);
            Console.WriteLine($"{endpoint.Id} Unpublishing {nodes.Count} variables took {sw.Elapsed}.");

            await _registry.DeactivateEndpointAsync(endpoint.Id, endpoint.GenerationId).ConfigureAwait(false);
            sw.Restart();
            while (true) {
                ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                if (ep.ActivationState == EntityActivationState.Deactivated) {
                    break;
                }
                if (sw.ElapsedMilliseconds > 60000) {
                    throw new Exception($"{endpoint.Id} failed to deactivate!");
                }
            }
            Console.WriteLine($"{endpoint.Id} deactivated.");
        }

        /// <summary>
        /// Test activation of endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private async Task TestBrowseAsync(EndpointInfoApiModel endpoint,
            CliOptions options) {
            EndpointInfoApiModel ep;

            Console.WriteLine($"Activating {endpoint.Id} for recursive browse...");
            await _registry.ActivateEndpointAsync(endpoint.Id).ConfigureAwait(false);
            var sw = Stopwatch.StartNew();
            while (true) {
                ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                if (ep.ActivationState == EntityActivationState.ActivatedAndConnected &&
                    ep.EndpointState == EndpointConnectivityState.Ready) {
                    break;
                }
                if (sw.ElapsedMilliseconds > 60000) {
                    Console.WriteLine($"{endpoint.Id} could not be activated - skip!");
                    return;
                }
            }
            Console.WriteLine($"{endpoint.Id} activated - recursive browse.");

            var silent = !options.IsSet("-V", "--verbose");
            var readDuringBrowse = options.IsProvidedOrNull("-v", "--readvalue");
            var targetNodesOnly = options.IsProvidedOrNull("-t", "--targets");
            var maxReferencesToReturn = options.GetValueOrDefault<uint>("-x", "--maxrefs", null);

            var workers = options.GetValueOrDefault("-w", "--workers", 1);  // 1 worker per endpoint
            await Task.WhenAll(Enumerable.Range(0, workers).Select(i =>
                BrowseAsync(i, endpoint.Id, silent, true, readDuringBrowse, null,
                    targetNodesOnly, maxReferencesToReturn, null, options))).ConfigureAwait(false);

            await _registry.DeactivateEndpointAsync(endpoint.Id).ConfigureAwait(false);
            sw.Restart();
            while (true) {
                ep = await _registry.GetEndpointAsync(endpoint.Id).ConfigureAwait(false);
                if (ep.ActivationState == EntityActivationState.Deactivated) {
                    break;
                }
                if (sw.ElapsedMilliseconds > 60000) {
                    throw new Exception($"{endpoint.Id} failed to deactivate!");
                }
            }
            Console.WriteLine($"{endpoint.Id} deactivated.");
        }

        /// <summary>
        /// Browse nodes
        /// </summary>
        private async Task BrowseAsync(int index, string id, bool silent, bool recursive,
            bool? readDuringBrowse, string node, bool? targetNodesOnly,
            uint? maxReferencesToReturn, BrowseDirection? direction, CliOptions options,
            List<string> variables = null) {

            var request = new BrowseRequestApiModel {
                TargetNodesOnly = targetNodesOnly,
                ReadVariableValues = readDuringBrowse,
                MaxReferencesToReturn = maxReferencesToReturn,
                Direction = direction
            };
            var nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { node };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nodesRead = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var errors = 0;
            var sw = Stopwatch.StartNew();
            while (nodes.Count > 0) {
                request.NodeId = nodes.First();
                nodes.Remove(request.NodeId);
                try {
                    var result = await NodeBrowseAsync(_twin, id, request).ConfigureAwait(false);
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
                                if (variables != null &&
                                    r.Target.NodeClass == NodeClass.Variable) {
                                    variables.Add(r.Target.NodeId);
                                }
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
                                Console.WriteLine($"Browse {index} - reading {r.Target.NodeId} resulted in {ex}");
                                errors++;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Console.WriteLine($"Browse {index} {request.NodeId} resulted in {e}");
                    errors++;
                }
            }
            Console.WriteLine($"Browse {index} took {sw.Elapsed}. Visited " +
                $"{visited.Count} nodes and read {nodesRead.Count} of them with {errors} errors.");
        }

        /// <summary>
        /// Browse all references
        /// </summary>
        private static async Task<BrowseResponseApiModel> NodeBrowseAsync(
            ITwinServiceApi service, string endpoint, BrowseRequestApiModel request) {
            while (true) {
                var result = await service.NodeBrowseFirstAsync(endpoint, request).ConfigureAwait(false);
                while (result.ContinuationToken != null) {
                    try {
                        var next = await service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestApiModel {
                                ContinuationToken = result.ContinuationToken,
                                Header = request.Header,
                                ReadVariableValues = request.ReadVariableValues,
                                TargetNodesOnly = request.TargetNodesOnly
                            }).ConfigureAwait(false);
                        result.References.AddRange(next.References);
                        result.ContinuationToken = next.ContinuationToken;
                    }
                    catch (Exception) {
                        await Try.Async(() => service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestApiModel {
                                ContinuationToken = result.ContinuationToken,
                                Abort = true
                            })).ConfigureAwait(false);
                        throw;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Select application registration
        /// </summary>
        private async Task<string> SelectApplicationAsync() {
            var result = await _registry.ListAllApplicationsAsync().ConfigureAwait(false);
            var applicationId = ConsoleEx.Select(result.Select(r => r.ApplicationId));
            if (string.IsNullOrEmpty(applicationId)) {
                Console.WriteLine("Nothing selected - application selection cleared.");
            }
            else {
                Console.WriteLine($"Selected {applicationId}.");
            }
            return applicationId;
        }

        /// <summary>
        /// Select endpoint registration
        /// </summary>
        private async Task<string> SelectEndpointAsync() {
            var result = await _registry.ListAllEndpointsAsync().ConfigureAwait(false);
            var endpointId = ConsoleEx.Select(result.Select(r => r.Id));
            if (string.IsNullOrEmpty(endpointId)) {
                Console.WriteLine("Nothing selected - application selection cleared.");
            }
            else {
                Console.WriteLine($"Selected {endpointId}.");
            }
            return endpointId;
        }

        /// <summary>
        /// Print result
        /// </summary>
        private void PrintResult<T>(CliOptions options, T status) {
            Console.WriteLine("==================");
            Console.WriteLine(JsonConvert.SerializeObject(status,
                options.GetValueOrDefault("-F", "--format", Formatting.Indented)));
            Console.WriteLine("==================");
        }

        /// <summary>
        /// Print help
        /// </summary>
        private static void PrintHelp() {
            Console.WriteLine(
                @"
aziiottest  Allows to excercise integration scenarios.
usage:      aziiottest command [options]

Commands and Options

     activation  Tests activation and deactivation of endpoints.
        with ...
        -i, --id        Application id to scope endpoints.
        -e, --endpoint  Whether to select and test single endpoint
        -a, --all       Use all endpoints
        -r, --repeat    How many times to repeat.
        -w, --waitstate Wait for state changes before deactivating.
        -l, --min-wait  Minimum wait time in between act/deact.
        -h, --max-wait  Maximum wait time in between act/deact.
        -b, --browse    Browse after activation if endpoint is Ready.
            -n, --nodeid    Node to browse
            -x, --maxrefs   Max number of references
            -d, --direction Browse direction (Forward, Backward, Both)
            -R, --recursive Browse recursively and read node values
            -v, --readvalue Read node values in browse
            -t, --targets   Only return target nodes
            -V, --verbose   Print browse results to screen
            -F, --format    Json format for result

     browse      Tests recursive browsing of endpoints.
        with ...
        -i, --id        Application id to scope endpoints.
        -e, --endpoint  Whether to select and test single endpoint
        -a, --all       Use all endpoints
        -w, --workers   How many workers browsing.
        -x, --maxrefs   Max number of references
        -v, --readvalue Read node values in browse
        -t, --targets   Only return target nodes
        -V, --verbose   Print browse results to screen
        -F, --format    Json format for result

     publish     Tests publishing and unpublishing.
        with ...
        -i, --id        Application id to scope endpoints.
        -e, --endpoint  Whether to select and test single endpoint
        -a, --all       Use all endpoints
        -F, --format    Json format for result

     console
     exit        To run in console mode and exit console mode.
     help, -h, -? --help
                 Prints out this help.
"
                );
        }

        private readonly Random _rand = new Random();
        private readonly IMetricServer _metrics;
        private readonly ILifetimeScope _scope;
        private readonly ITwinServiceApi _twin;
        private readonly IRegistryServiceApi _registry;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IPublisherServiceApi _publisher;
        private readonly IVaultServiceApi _vault;
#pragma warning restore IDE0052 // Remove unread private members
    }
}
