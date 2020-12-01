// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Services.Module.Cli {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.OpcUa.Sample;
    using Microsoft.IIoT.Platform.OpcUa.Services;
    using Microsoft.IIoT.Platform.OpcUa.Testing.Runtime;
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Azure.IoTHub.Clients;
    using Microsoft.IIoT.Azure.LogAnalytics;
    using Microsoft.IIoT.Azure.LogAnalytics.Runtime;
    using Microsoft.IIoT.Diagnostics;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Serializers.NewtonSoft;
    using Microsoft.IIoT.Storage.Services;
    using Microsoft.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Opc.Ua;
    using Opc.Ua.Design;
    using Opc.Ua.Design.Resolver;
    using Opc.Ua.Encoders;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// OPC Twin module cli
    /// </summary>
    public static class Program {

        private enum Op {
            None,
            Host,
            Add,
            Get,
            Reset,
            Delete,
            List,

            TestOpcUaModelBrowseEncoder,
            TestOpcUaModelBrowseFile,
            TestOpcUaModelArchiver,
            TestOpcUaModelWriter,
            TestOpcUaModelDesign,
            TestBrowseServer,

            ResetAll,
            Cleanup,
            CleanupAll
        }

        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(string[] args) {
            if (args is null) {
                throw new ArgumentNullException(nameof(args));
            }
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Console.WriteLine("unhandled: " + e.ExceptionObject);
            var op = Op.None;
            string deviceId = null, moduleId = null;
            Console.WriteLine("Twin module command line interface.");
            var configuration = new ConfigurationBuilder()
                .AddFromDotEnvFile()
                .AddEnvironmentVariables()
                .AddFromKeyVault()
                .Build();
            var cs = configuration.GetValue<string>(PcsVariable.PCS_IOTHUB_CONNSTRING, null);
            if (string.IsNullOrEmpty(cs)) {
                cs = configuration.GetValue<string>("_HUB_CS", null);
            }
            var diagnostics = new LogAnalyticsConfig(configuration).ToOptions().Value;
            IOptions<IoTHubOptions> config = null;
            var endpoint = new EndpointModel();
            string fileName = null;
            var host = Utils.GetHostName();
            var ports = new List<int>();
            try {
                for (var i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "-C":
                        case "--connection-string":
                            i++;
                            if (i < args.Length) {
                                cs = args[i];
                                break;
                            }
                            throw new ArgumentException(
                                "Missing arguments for connection string");
                        case "--test-browse":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestBrowseServer;
                            i++;
                            if (i < args.Length) {
                                endpoint.Url = args[i];
                            }
                            break;
                        case "--test-archive":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaModelArchiver;
                            i++;
                            if (i < args.Length) {
                                endpoint.Url = args[i];
                            }
                            break;
                        case "--test-export":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaModelBrowseEncoder;
                            i++;
                            if (i < args.Length) {
                                endpoint.Url = args[i];
                            }
                            break;
                        case "--test-file":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaModelBrowseFile;
                            i++;
                            if (i < args.Length) {
                                endpoint.Url = args[i];
                            }
                            break;
                        case "--test-writer":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaModelWriter;
                            i++;
                            if (i < args.Length) {
                                endpoint.Url = args[i];
                            }
                            break;
                        case "--test-design":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaModelDesign;
                            i++;
                            if (i < args.Length) {
                                fileName = args[i];
                            }
                            break;
                        case "-?":
                        case "-h":
                        case "--help":
                            throw new ArgumentException("Help");
                        default:
                            if (op != Op.None) {
                                throw new ArgumentException(
                                    "Operations are mutually exclusive");
                            }
                            switch (args[i]) {
                                case "--list":
                                    op = Op.List;
                                    break;
                                case "--add":
                                    op = Op.Add;
                                    break;
                                case "--get":
                                    op = Op.Get;
                                    break;
                                case "--reset":
                                    op = Op.Reset;
                                    break;
                                case "--delete":
                                    op = Op.Delete;
                                    break;
                                case "--host":
                                    op = Op.Host;
                                    break;
                                case "--delete-all":
                                    op = Op.CleanupAll;
                                    break;
                                case "--reset-all":
                                    op = Op.ResetAll;
                                    break;
                                case "--cleanup":
                                    op = Op.Cleanup;
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown {args[i]}");
                            }
                            // Try parse ids
                            switch (op) {
                                case Op.Add:
                                case Op.Host:
                                case Op.Get:
                                case Op.Reset:
                                case Op.Delete:
                                    i++;
                                    if (i < args.Length) {
                                        deviceId = args[i];
                                        i++;
                                        if (i < args.Length) {
                                            moduleId = args[i];
                                            break;
                                        }
                                    }
                                    break;
                            }
                            break;
                    }

                }
                if (op == Op.None) {
                    op = Op.Host;
                }
                if (string.IsNullOrEmpty(cs)) {
                    throw new ArgumentException("Missing connection string.");
                }
                if (!ConnectionString.TryParse(cs, out var connectionString)) {
                    throw new ArgumentException("Bad connection string.");
                }
                config = connectionString.ToIoTHubOptions();

                switch (op) {
                    case Op.Get:
                    case Op.Reset:
                    case Op.Delete:
                        if (deviceId == null || moduleId == null) {
                            throw new ArgumentException(
                                "Missing arguments for delete/reset/get command.");
                        }
                        break;
                    case Op.Add:
                    case Op.Host:
                        if (deviceId == null) {
                            deviceId = Utils.GetHostName();
                            Console.WriteLine($"Using <deviceId> '{deviceId}'");
                        }
                        if (moduleId == null) {
                            moduleId = "twin";
                            Console.WriteLine($"Using <moduleId> '{moduleId}'");
                        }
                        break;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Usage:       [options] operation [args]

Operations (Mutually exclusive):

    --list
             List all registered supervisor module identities.
    --add <deviceId> <moduleId>
             Add twin module with given device id and module id to device registry.
    --get <deviceId> <moduleId>
             Get twin module connection string from device registry.
    --host <deviceId> <moduleId>
             Host the twin module under the given device id and module id.
    --reset <deviceId> <moduleId>
             Reset registered module identity twin properties and tags.
    --delete <deviceId> <moduleId>
             Delete registered module identity.
    --reset-all
             Clear all registered supervisor module identities.
    --cleanup
             Clear entire Registry content.
    --delete-all
             Cleanup and delete all supervisor identities.

    --test-browse           Tests server browsing.
    --test_export           Tests server model export with passed endpoint url.
    --test-design           Test model design import
    --test-file             Tests server model export several files for perf.
    --test-writer           Tests server model import.
    --test-archive          Tests server model archiving to file.

Options:
     -C
    --connection-string
             IoT Hub owner connection string to use to connect to IoT hub for
             operations on the registry.  If not provided, read from environment.

    --help
     -?
     -h      Prints out this help.
"
                    );
                return;
            }

            if (ports.Count == 0) {
                ports.Add(51210);
            }
            try {
                switch (op) {
                    case Op.Host:
                        HostAsync(config, diagnostics, deviceId, moduleId).Wait();
                        break;
                    case Op.Add:
                        AddAsync(config, diagnostics, deviceId, moduleId).Wait();
                        break;
                    case Op.Get:
                        GetAsync(config, deviceId, moduleId).Wait();
                        break;
                    case Op.Reset:
                        ResetAsync(config, deviceId, moduleId).Wait();
                        break;
                    case Op.Delete:
                        DeleteAsync(config, deviceId, moduleId).Wait();
                        break;
                    case Op.ResetAll:
                        ResetAllAsync(config).Wait();
                        break;
                    case Op.List:
                        ListAsync(config).Wait();
                        break;
                    case Op.Cleanup:
                        CleanupAsync(config, false).Wait();
                        break;
                    case Op.CleanupAll:
                        CleanupAsync(config, true).Wait();
                        break;
                    case Op.TestOpcUaModelBrowseEncoder:
                        TestOpcUaModelExportServiceAsync(endpoint).Wait();
                        break;
                    case Op.TestOpcUaModelBrowseFile:
                        TestOpcUaModelExportToFileAsync(endpoint).Wait();
                        break;
                    case Op.TestOpcUaModelArchiver:
                        TestOpcUaModelArchiveAsync(endpoint).Wait();
                        break;
                    case Op.TestOpcUaModelWriter:
                        TestOpcUaModelWriterAsync(endpoint).Wait();
                        break;
                    case Op.TestOpcUaModelDesign:
                        TestOpcUaModelDesignAsync(fileName).Wait();
                        break;
                    case Op.TestBrowseServer:
                        TestBrowseServerAsync(endpoint).Wait();
                        break;
                    default:
                        throw new ArgumentException("Unknown.");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Host the module giving it its connection string.
        /// </summary>
        private static async Task HostAsync(IOptions<IoTHubOptions> config,
            LogAnalyticsOptions diagnostics, string deviceId, string moduleId) {
            Console.WriteLine("Create or retrieve connection string...");
            var logger = Log.Console(LogLevel.Error);
            var cs = await Retry.WithExponentialBackoff(logger,
                () => AddOrGetAsync(config, diagnostics, deviceId, moduleId)).ConfigureAwait(false);

            Console.WriteLine("Starting twin service...");
            var arguments = new List<string> {
                    $"EdgeHubConnectionString={cs}"
                };
            Service.Program.Main(arguments.ToArray());
            Console.WriteLine("Twin service exited.");
        }

        /// <summary>
        /// Add supervisor
        /// </summary>
        private static async Task AddAsync(IOptions<IoTHubOptions> config, LogAnalyticsOptions diagnostics,
            string deviceId, string moduleId) {
            var cs = await AddOrGetAsync(config, diagnostics, deviceId, moduleId).ConfigureAwait(false);
            Console.WriteLine(cs);
        }

        /// <summary>
        /// Get module connection string
        /// </summary>
        private static async Task GetAsync(
            IOptions<IoTHubOptions> config, string deviceId, string moduleId) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            var cs = await registry.GetConnectionStringAsync(deviceId, moduleId).ConfigureAwait(false);
            Console.WriteLine(cs);
        }

        /// <summary>
        /// Reset supervisor
        /// </summary>
        private static async Task ResetAsync(IOptions<IoTHubOptions> config,
            string deviceId, string moduleId) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            await ResetAsync(registry, await registry.GetAsync(deviceId, moduleId,
                CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete supervisor
        /// </summary>
        private static async Task DeleteAsync(IOptions<IoTHubOptions> config,
            string deviceId, string moduleId) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            await registry.DeleteAsync(deviceId, moduleId, null, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// List all twin module identities
        /// </summary>
        private static async Task ListAsync(IOptions<IoTHubOptions> config) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);

            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Supervisor}'";
            var supers = await registry.QueryAllDeviceTwinsAsync(query).ConfigureAwait(false);
            foreach (var item in supers) {
                Console.WriteLine($"{item.Id} {item.ModuleId}");
            }
        }

        /// <summary>
        /// Reset all supervisor tags and properties
        /// </summary>
        private static async Task ResetAllAsync(IOptions<IoTHubOptions> config) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);

            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Supervisor}'";
            var supers = await registry.QueryAllDeviceTwinsAsync(query).ConfigureAwait(false);
            foreach (var item in supers) {
                Console.WriteLine($"Resetting {item.Id} {item.ModuleId ?? ""}");
                await ResetAsync(registry, item).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Clear registry
        /// </summary>
        private static async Task CleanupAsync(IOptions<IoTHubOptions> config,
            bool includeSupervisors) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            var result = await registry.QueryAllDeviceTwinsAsync(
                "SELECT * from devices where IS_DEFINED(tags.DeviceType)").ConfigureAwait(false);
            foreach (var item in result) {
                Console.WriteLine($"Deleting {item.Id} {item.ModuleId ?? ""}");
                await registry.DeleteAsync(item.Id, item.ModuleId, null,
                    CancellationToken.None).ConfigureAwait(false);
            }
            if (!includeSupervisors) {
                return;
            }
            var query = "SELECT * FROM devices.modules WHERE " +
             $"properties.reported.{TwinProperty.Type} = '{IdentityType.Supervisor}'";
            var supers = await registry.QueryAllDeviceTwinsAsync(query).ConfigureAwait(false);
            foreach (var item in supers) {
                Console.WriteLine($"Deleting {item.Id} {item.ModuleId ?? ""}");
                await registry.DeleteAsync(item.Id, item.ModuleId, null,
                    CancellationToken.None).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Reset supervisor
        /// </summary>
        private static async Task ResetAsync(IoTHubServiceClient registry,
            DeviceTwinModel item) {
            var properties = new Dictionary<string, VariantValue>();
            var tags = new Dictionary<string, VariantValue>();
            if (item.Tags != null) {
                foreach (var tag in item.Tags.Keys.ToList()) {
                    tags.Add(tag, null);
                }
            }
            if (item.Properties?.Desired != null) {
                foreach (var property in item.Properties.Desired.Keys.ToList()) {
                    if (property.StartsWith('$')) {
                        continue;
                    }
                    properties.Add(property, null);
                }
            }
            if (item.Properties?.Reported != null) {
                foreach (var property in item.Properties.Reported.Keys.ToList()) {
                    if (property.StartsWith('$')) {
                        continue;
                    }
                    if (!item.Properties.Desired.ContainsKey(property)) {
                        properties.Add(property, null);
                    }
                }
            }
            item.Tags = tags;
            item.Properties = new TwinPropertiesModel {
                Desired = properties
            };
            await registry.PatchAsync(item, true, default).ConfigureAwait(false);
        }

        /// <summary>
        /// Add or get module identity
        /// </summary>
        private static async Task<ConnectionString> AddOrGetAsync(IOptions<IoTHubOptions> config,
            LogAnalyticsOptions diagnostics, string deviceId, string moduleId) {
            var logger = Log.Console(LogLevel.Error);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);
            try {
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = deviceId,
                    Tags = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = IdentityType.Gateway
                    },
                    Capabilities = new DeviceCapabilitiesModel {
                        IotEdge = true
                    }
                }, false, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Gateway {deviceId} exists.", deviceId);
            }
            try {
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = deviceId,
                    ModuleId = moduleId,
                    Properties = new TwinPropertiesModel {
                        Desired = new Dictionary<string, VariantValue> {
                            [nameof(diagnostics.LogWorkspaceId)] = diagnostics?.LogWorkspaceId,
                            [nameof(diagnostics.LogWorkspaceKey)] = diagnostics?.LogWorkspaceKey
                        }
                    }
                }, true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Module {moduleId} exists...", moduleId);
            }
            var cs = await registry.GetConnectionStringAsync(deviceId, moduleId).ConfigureAwait(false);
            return cs;
        }

        /// <summary>
        /// Test model browse encoder
        /// </summary>
        private static async Task TestOpcUaModelExportServiceAsync(EndpointModel endpoint) {
            using (var logger = StackLogger.Create(Log.Console()))
            using (var config = new TestClientServicesConfig())
            using (var client = new ClientServices(logger.Logger, config))
            using (var server = new ServerWrapper(endpoint, logger))
            using (var stream = Console.OpenStandardOutput())
            using (var writer = new StreamWriter(stream))
            using (var json = new JsonTextWriter(writer) {
                AutoCompleteOnClose = true,
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            })
            using (var encoder = new JsonEncoderEx(json, null,
                JsonEncoderEx.JsonEncoding.Array) {
                IgnoreDefaultValues = true,
                UseAdvancedEncoding = true
            })
            using (var browser = new BrowsedNodeStreamEncoder(client,
                endpoint.ToConnectionModel(), encoder, null, logger.Logger)) {
                await browser.EncodeAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Test model archiver
        /// </summary>
        private static async Task TestOpcUaModelArchiveAsync(EndpointModel endpoint) {
            using (var logger = StackLogger.Create(Log.Console())) {
                var storage = new ZipArchiveStorage();
                var fileName = "tmp.zip";
                using (var config = new TestClientServicesConfig())
                using (var client = new ClientServices(logger.Logger, config))
                using (var server = new ServerWrapper(endpoint, logger)) {
                    var sw = Stopwatch.StartNew();
                    using (var archive = await storage.OpenAsync(fileName, FileMode.Create,
                        FileAccess.Write).ConfigureAwait(false))
                    using (var archiver = new AddressSpaceArchiver(client,
                        endpoint.ToConnectionModel(), archive, logger.Logger)) {
                        await archiver.ArchiveAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                    var elapsed = sw.Elapsed;
                    using (var file = File.Open(fileName, FileMode.OpenOrCreate)) {
                        Console.WriteLine($"Encode as to {fileName} took " +
                            $"{elapsed}, and produced {file.Length} bytes.");
                    }
                }
            }
        }

        /// <summary>
        /// Test model browse encoder to file
        /// </summary>
        private static async Task TestOpcUaModelExportToFileAsync(EndpointModel endpoint) {
            using (var logger = StackLogger.Create(Log.Console())) {
                // Run both encodings twice to prime server and get realistic timings the
                // second time around
                var runs = new Dictionary<string, string> {
                    ["json1.zip"] = ContentMimeType.UaJson,
                    //  ["bin1.zip"] = ContentEncodings.MimeTypeUaBinary,
                    ["json2.zip"] = ContentMimeType.UaJson,
                    //  ["bin2.zip"] = ContentEncodings.MimeTypeUaBinary,
                    ["json1.gzip"] = ContentMimeType.UaJson,
                    //  ["bin1.gzip"] = ContentEncodings.MimeTypeUaBinary,
                    ["json2.gzip"] = ContentMimeType.UaJson,
                    // ["bin2.gzip"] = ContentEncodings.MimeTypeUaBinary
                };

                using (var config = new TestClientServicesConfig())
                using (var client = new ClientServices(logger.Logger, config))
                using (var server = new ServerWrapper(endpoint, logger)) {
                    foreach (var run in runs) {
                        var zip = Path.GetExtension(run.Key) == ".zip";
                        Console.WriteLine($"Writing {run.Key}...");
                        var sw = Stopwatch.StartNew();
                        using (var stream = new FileStream(run.Key, FileMode.Create)) {
                            using (var zipped = zip ?
                                new DeflateStream(stream, CompressionLevel.Optimal) :
                                (Stream)new GZipStream(stream, CompressionLevel.Optimal))
                            using (var browser = new BrowsedNodeStreamEncoder(client,
                                endpoint.ToConnectionModel(), zipped,
                                run.Value, null, logger.Logger)) {
                                await browser.EncodeAsync(CancellationToken.None).ConfigureAwait(false);
                            }
                        }
                        var elapsed = sw.Elapsed;
                        using (var file = File.Open(run.Key, FileMode.OpenOrCreate)) {
                            Console.WriteLine($"Encode as {run.Value} to {run.Key} took " +
                                $"{elapsed}, and produced {file.Length} bytes.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test model export and import
        /// </summary>
        private static async Task TestOpcUaModelWriterAsync(EndpointModel endpoint) {
            using (var logger = StackLogger.Create(Log.Console())) {
                var filename = "model.zip";
                using (var server = new ServerWrapper(endpoint, logger)) {
                    using (var client = new ClientServices(logger.Logger, new TestClientServicesConfig())) {
                        Console.WriteLine($"Reading into {filename}...");
                        using (var stream = new FileStream(filename, FileMode.Create)) {
                            using (var zipped = new DeflateStream(stream, CompressionLevel.Optimal))
                            using (var browser = new BrowsedNodeStreamEncoder(client,
                                endpoint.ToConnectionModel(), zipped,
                                ContentMimeType.UaJson, null, logger.Logger)) {
                                await browser.EncodeAsync(CancellationToken.None).ConfigureAwait(false);
                            }
                        }
                    }
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                var serializer = new NewtonSoftJsonSerializer();
                for (var i = 0; ; i++) {
                    Console.WriteLine($"{i}: Writing from {filename}...");
                    var sw = Stopwatch.StartNew();
                    using (var file = File.Open(filename, FileMode.OpenOrCreate)) {
                        using (var unzipped = new DeflateStream(file, CompressionMode.Decompress)) {
                            // TODO
                            // var writer = new SourceStreamImporter(new ItemContainerFactory(database),
                            //     new VariantEncoderFactory(), logger.Logger);
                            // await writer.ImportAsync(unzipped, Path.GetFullPath(filename + i),
                            //     ContentMimeType.UaJson, null, CancellationToken.None);
                        }
                    }
                    var elapsed = sw.Elapsed;
                    Console.WriteLine($"{i}: Writing took {elapsed}.");
                }
            }
        }

        /// <summary>
        /// Test model design import
        /// </summary>
        /// <param name="designFile"></param>
        /// <returns></returns>
        private static Task TestOpcUaModelDesignAsync(string designFile) {
            if (string.IsNullOrEmpty(designFile)) {
                throw new ArgumentException("Design name invalid", nameof(designFile));
            }
            var design = Model.Load(designFile, new CompositeModelResolver());
            design.Save(Path.GetDirectoryName(designFile));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Test address space control
        /// </summary>
        private static async Task TestBrowseServerAsync(EndpointModel endpoint, bool silent = false) {

            using (var logger = StackLogger.Create(Log.Console())) {

                var request = new BrowseRequestModel {
                    TargetNodesOnly = false
                };
                var nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                    ObjectIds.RootFolder.ToString()
                };

                using (var config = new TestClientServicesConfig())
                using (var client = new ClientServices(logger.Logger, config)) {
                    var service = new AddressSpaceServices(client, new VariantEncoderFactory(), logger.Logger);
                    using (var server = new ServerWrapper(endpoint, logger)) {
                        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var nodesRead = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var errors = 0;
                        var sw = Stopwatch.StartNew();
                        while (nodes.Count > 0) {
                            request.NodeId = nodes.First();
                            nodes.Remove(request.NodeId);
                            try {
                                if (!silent) {
                                    Console.WriteLine($"Browsing {request.NodeId}");
                                    Console.WriteLine($"====================");
                                }
                                var result = await service.NodeBrowseAsync(
                                    endpoint.ToConnectionModel(), request).ConfigureAwait(false);
                                visited.Add(request.NodeId);
                                if (!silent) {
                                    Console.WriteLine(JsonConvert.SerializeObject(result,
                                        Formatting.Indented));
                                }

                                // Do recursive browse
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
                                        r.Target.NodeClass.Value != Core.Models.NodeClass.Variable) {
                                        continue;
                                    }
                                    if (!silent) {
                                        Console.WriteLine($"Reading {r.Target.NodeId}");
                                        Console.WriteLine($"====================");
                                    }
                                    try {
                                        nodesRead.Add(r.Target.NodeId);
                                        var read = await service.NodeValueReadAsync(endpoint.ToConnectionModel(),
                                            new ValueReadRequestModel {
                                                NodeId = r.Target.NodeId
                                            }, default).ConfigureAwait(false);
                                        if (!silent) {
                                            Console.WriteLine(JsonConvert.SerializeObject(result,
                                                Formatting.Indented));
                                        }
                                    }
                                    catch (Exception ex) {
                                        Console.WriteLine($"Reading {r.Target.NodeId} resulted in {ex}");
                                        errors++;
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
                }
            }
        }

        /// <summary>
        /// Wraps server and disposes after use
        /// </summary>
        private class ServerWrapper : IDisposable {

            /// <summary>
            /// Create wrapper
            /// </summary>
            /// <param name="endpoint"></param>
            public ServerWrapper(EndpointModel endpoint, StackLogger logger) {
                _cts = new CancellationTokenSource();
                if (endpoint.Url == null) {
                    _server = RunSampleServerAsync(logger.Logger, _cts.Token);
                    endpoint.Url = "opc.tcp://" + Utils.GetHostName() +
                        ":51210/UA/SampleServer";
                }
                else {
                    _server = Task.CompletedTask;
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                _cts.Cancel();
                _server.Wait();
                _cts.Dispose();
            }

            /// <summary>
            /// Run server until cancelled
            /// </summary>
            /// <param name="ct"></param>
            /// <returns></returns>
            private static async Task RunSampleServerAsync(ILogger logger, CancellationToken ct) {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                ct.Register(() => tcs.TrySetResult(true));
                using (var server = new ServerConsoleHost(new ServerFactory(logger) {
                    LogStatus = false
                }, logger) {
                    AutoAccept = true
                }) {
                    await server.StartAsync(new List<int> { 51210 }).ConfigureAwait(false);
                    await tcs.Task.ConfigureAwait(false);
                }
            }

            private readonly CancellationTokenSource _cts;
            private readonly Task _server;
        }
    }
}
