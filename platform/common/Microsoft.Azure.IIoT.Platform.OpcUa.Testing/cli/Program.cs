// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Cli {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Sample;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Services;
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Runtime;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Opc.Ua;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Test client for opc ua services
    /// </summary>
    public static class Program {
        private enum Op {
            None,
            RunSampleServer,
            TestOpcUaServerClient
        }

        /// <summary>
        /// Test client entry point
        /// </summary>
        public static void Main(string[] args) {
            if (args is null) {
                throw new ArgumentNullException(nameof(args));
            }
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Console.WriteLine("unhandled: " + e.ExceptionObject);
            var op = Op.None;
            var endpoint = new EndpointModel();
            var host = Utils.GetHostName();
            var ports = new List<int>();
            try {
                for (var i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "--sample":
                        case "-s":
                            op = Op.RunSampleServer;
                            break;
                        case "-p":
                        case "--port":
                            i++;
                            if (i < args.Length) {
                                ports.Add(ushort.Parse(args[i]));
                                break;
                            }
                            throw new ArgumentException(
                                "Missing arguments for port option");
                        case "--test-client":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.TestOpcUaServerClient;
                            i++;
                            if (i < args.Length) {
                                endpoint.Url = args[i];
                            }
                            break;
                        case "-?":
                        case "-h":
                        case "--help":
                            throw new ArgumentException("Help");
                        default:
                            throw new ArgumentException($"Unknown {args[i]}");
                    }
                }
                if (op == Op.None) {
                    if (ports.Count == 0) {
                        var envPort = Environment.GetEnvironmentVariable("SERVER_PORT");
                        if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out var port)) {
                            ports.Add(port);
                        }
                        else {
                            throw new ArgumentException(
                                "Missing port to run sample server or specify --sample option.");
                        }
                    }
                    op = Op.RunSampleServer;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Test server host
usage:       [options] operation [args]

Options:

    --port / -p             Port to listen on
    --help / -? / -h        Prints out this help.

Operations (Mutually exclusive):

    --sample / -s           Run sample server and wait for cancellation.
                            Default if port is specified.

    --test-client           Tests server stuff with passed endpoint url.
"
                    );
                return;
            }

            if (ports.Count == 0) {
                ports.Add(51210);
            }
            try {
                Console.WriteLine($"Running {op}...");
                switch (op) {
                    case Op.RunSampleServer:
                        RunServerAsync(ports).Wait();
                        return;
                    case Op.TestOpcUaServerClient:
                        TestOpcUaServerClientAsync(endpoint).Wait();
                        break;
                    default:
                        throw new ArgumentException("Unknown.");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return;
            }

            Console.WriteLine("Press key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Run server until exit
        /// </summary>
        private static async Task RunServerAsync(IEnumerable<int> ports) {
            using (var logger = StackLogger.Create(ConsoleLogger.CreateLogger())) {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                AssemblyLoadContext.Default.Unloading += _ => tcs.TrySetResult(true);
                using (var server = new ServerConsoleHost(new ServerFactory(logger.Logger),
                    logger.Logger) {
                    AutoAccept = true
                }) {
                    await server.StartAsync(ports).ConfigureAwait(false);
#if DEBUG
                    if (!Console.IsInputRedirected) {
                        Console.WriteLine("Press any key to exit...");
                        Console.TreatControlCAsInput = true;
                        await Task.WhenAny(tcs.Task, Task.Run(() => Console.ReadKey())).ConfigureAwait(false);
                        return;
                    }
#endif
                    await tcs.Task.ConfigureAwait(false);
                    logger.Logger.LogInformation("Exiting.");
                }
            }
        }


        /// <summary>
        /// Test client
        /// </summary>
        private static async Task TestOpcUaServerClientAsync(EndpointModel endpoint) {
            using (var logger = StackLogger.Create(ConsoleLogger.CreateLogger()))
            using (var config = new TestClientServicesConfig())
            using (var client = new ClientServices(logger.Logger, config))
            using (var server = new ServerWrapper(endpoint, logger)) {
                await client.ExecuteServiceAsync(endpoint, null, session => {
                    Console.WriteLine("Browse the OPC UA server namespace.");
                    var w = Stopwatch.StartNew();
                    var stack = new Stack<Tuple<string, ReferenceDescription>>();
                    session.Browse(null, null, ObjectIds.RootFolder,
                        0u, Opc.Ua.BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences,
                        true, 0, out var continuationPoint, out var references);
                    Console.WriteLine(" DisplayName, BrowseName, NodeClass");
                    references.Reverse();
                    foreach (var rd in references) {
                        stack.Push(Tuple.Create("", rd));
                    }
                    while (stack.Count > 0) {
                        var browsed = stack.Pop();
                        session.Browse(null, null,
                            ExpandedNodeId.ToNodeId(browsed.Item2.NodeId, session.NamespaceUris),
                            0u, Opc.Ua.BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences,
                            true, 0, out continuationPoint, out references);
                        references.Reverse();
                        foreach (var rd in references) {
                            stack.Push(Tuple.Create(browsed.Item1 + "   ", rd));
                        }
                        Console.WriteLine($"{browsed.Item1}{(references.Count == 0 ? "-" : "+")} " +
                            $"{browsed.Item2.DisplayName}, {browsed.Item2.BrowseName}, {browsed.Item2.NodeClass}");
                    }
                    Console.WriteLine($"   ....        took {w.ElapsedMilliseconds} ms...");
                    return Task.FromResult(true);
                }).ConfigureAwait(false);
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
