﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Edge client identity
    /// </summary>
    public sealed class IoTEdgeClientIdentity : IIdentity {

        /// <inheritdoc />
        public string Hub { get; }

        /// <inheritdoc />
        public string DeviceId { get; }

        /// <inheritdoc />
        public string ModuleId { get; }

        /// <inheritdoc />
        public string Gateway { get; }

        /// <summary>
        /// Create sdk factory
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public IoTEdgeClientIdentity(IOptions<IoTEdgeClientOptions> options, ILogger logger) {

            // The runtime injects this as an environment variable
            var deviceId = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            var moduleId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");
            var gateway = Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME");
            var hub = Environment.GetEnvironmentVariable("IOTEDGE_IOTHUBHOSTNAME");

            try {
                if (!string.IsNullOrEmpty(options.Value.EdgeHubConnectionString)) {
                    var cs = IotHubConnectionStringBuilder.Create(
                        options.Value.EdgeHubConnectionString);

                    if (string.IsNullOrEmpty(cs.SharedAccessKey)) {
                        throw new InvalidConfigurationException(
                            "Connection string is missing shared access key.");
                    }
                    if (string.IsNullOrEmpty(cs.DeviceId)) {
                        throw new InvalidConfigurationException(
                            "Connection string is missing device id.");
                    }

                    deviceId = cs.DeviceId;
                    moduleId = cs.ModuleId;
                    hub = cs.HostName;

                    // Use the environment variable if provided to override the gateway.
                    if (!string.IsNullOrEmpty(cs.GatewayHostName)) {
                        gateway = cs.GatewayHostName;
                    }
                }
            }
            catch (Exception e) {
                logger.LogError(e, "Bad configuration value in EdgeHubConnectionString config.");
            }

            if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(hub)) {
                throw new InvalidConfigurationException(
"If you are running outside of an IoT Edge context or in EdgeHubDev mode, then the " +
"host configuration is incomplete and missing the EdgeHubConnectionString setting." +
"You can run the module using the command line interface or in IoT Edge context, or " +
"manually set the 'EdgeHubConnectionString' environment variable.");
            }

            Hub = hub;
            ModuleId = moduleId;
            DeviceId = deviceId;
            Gateway = gateway;
        }
    }
}
