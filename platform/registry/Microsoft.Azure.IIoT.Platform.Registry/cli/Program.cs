// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Cli {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Newtonsoft.Json;
    using Opc.Ua;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Test client for opc ua services
    /// </summary>
    public class Program {
        private enum Op {
            None,
            MakeSupervisor,
            ClearSupervisors,
            ClearRegistry
        }

        /// <summary>
        /// Test client entry point
        /// </summary>
        public static void Main(string[] args) {
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Console.WriteLine("unhandled: " + e.ExceptionObject);
            var op = Op.None;
            string deviceId = null, moduleId = null;
            var host = Utils.GetHostName();
            try {
                for (var i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "--make-supervisor":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.MakeSupervisor;
                            i++;
                            if (i < args.Length) {
                                deviceId = args[i];
                                i++;
                                if (i < args.Length) {
                                    moduleId = args[i];
                                    break;
                                }
                            }
                            throw new ArgumentException("Missing arguments to make iotedge device");
                        case "--clear-registry":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.ClearRegistry;
                            break;
                        case "--clear-supervisors":
                            if (op != Op.None) {
                                throw new ArgumentException("Operations are mutually exclusive");
                            }
                            op = Op.ClearSupervisors;
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
                    throw new ArgumentException("Missing operation.");
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Test host
usage:       [options] operation [args]

Options:

    --stress                Run test as stress test (if supported)
    --help / -? / -h        Prints out this help.

Operations (Mutually exclusive):

    --make-supervisor       Make supervisor module.
    --clear-registry        Clear device registry content.
    --clear-supervisors     Clear supervisors in device registry.

"
                    );
                return;
            }

            try {
                Console.WriteLine($"Running {op}...");
                switch (op) {
                    case Op.MakeSupervisor:
                        MakeSupervisorAsync(deviceId, moduleId).Wait();
                        break;
                    case Op.ClearSupervisors:
                        ClearSupervisorsAsync().Wait();
                        break;
                    case Op.ClearRegistry:
                        ClearRegistryAsync().Wait();
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
        /// Create supervisor module identity in device registry
        /// </summary>
        private static async Task MakeSupervisorAsync(string deviceId, string moduleId) {
            var logger = ConsoleOutLogger.Create();
            var config = new IoTHubConfig(null);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);

            await registry.CreateOrUpdateAsync(new DeviceTwinModel {
                Id = deviceId,
                ModuleId = moduleId
            }, true, CancellationToken.None);

            var module = await registry.GetRegistrationAsync(deviceId, moduleId, CancellationToken.None);
            Console.WriteLine(JsonConvert.SerializeObject(module));
            var twin = await registry.GetAsync(deviceId, moduleId, CancellationToken.None);
            Console.WriteLine(JsonConvert.SerializeObject(twin));
            var cs = ConnectionString.Parse(config.IoTHubConnString);
            Console.WriteLine("Connection string:");
            Console.WriteLine($"HostName={cs.HostName};DeviceId={deviceId};" +
                $"ModuleId={moduleId};SharedAccessKey={module.Authentication.PrimaryKey}");
        }

        /// <summary>
        /// Clear registry
        /// </summary>
        private static async Task ClearSupervisorsAsync() {
            var logger = ConsoleOutLogger.Create();
            var config = new IoTHubConfig(null);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);

            var query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{TwinProperty.Type} = '{IdentityType.Supervisor}'";
            var supers = await registry.QueryAllDeviceTwinsAsync(query);
            foreach (var item in supers) {
                foreach (var tag in item.Tags.Keys.ToList()) {
                    item.Tags[tag] = null;
                }
                foreach (var property in item.Properties.Desired.Keys.ToList()) {
                    item.Properties.Desired[property] = null;
                }
                foreach (var property in item.Properties.Reported.Keys.ToList()) {
                    if (!item.Properties.Desired.ContainsKey(property)) {
                        item.Properties.Desired.Add(property, null);
                    }
                }
                await registry.CreateOrUpdateAsync(item, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// Clear registry
        /// </summary>
        private static async Task ClearRegistryAsync() {
            var logger = ConsoleOutLogger.Create();
            var config = new IoTHubConfig(null);
            var registry = new IoTHubServiceClient(
                config, new NewtonSoftJsonSerializer(), logger);

            var result = await registry.QueryAllDeviceTwinsAsync(
                "SELECT * from devices where IS_DEFINED(tags.DeviceType)");
            foreach (var item in result) {
                await registry.DeleteAsync(item.Id, item.ModuleId, null, CancellationToken.None);
            }
        }
    }
}
