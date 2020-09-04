// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.RabbitMq.Services {
    using Serilog;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using System.Diagnostics;

    /// <summary>
    /// Represents a rabbit mq server instance
    /// </summary>
    public abstract class DockerContainer {

        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="logger"></param>
        public DockerContainer(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Start container
        /// </summary>
        /// <param name="containerParameters"></param>
        /// <param name="containerName"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        protected async Task<(string, bool)> StartContainerAsync(
            CreateContainerParameters containerParameters, string containerName, string imageName) {
            using (var dockerClient = GetDockerClient()) {
                containerParameters.Image = imageName;

                var containers = await dockerClient.Containers.ListContainersAsync(
                    new ContainersListParameters { All = true });
                var existingContainer = containers
                    .SingleOrDefault(c => c.Names.Contains("/" + containerName));

                string containerId = null;
                if (existingContainer != null) {
                    containerId = existingContainer.ID;
                    if (existingContainer.State == "running") {
                        return (containerId, false);
                    }
                }
                if (containerId == null) {
                    containerParameters.Name = containerName;
                    var container = await dockerClient.Containers.CreateContainerAsync(
                        containerParameters);
                    containerId = container.ID;
                }

                await dockerClient.Containers.StartContainerAsync(containerId,
                    new ContainerStartParameters());
                return (containerId, true);
            }
        }

        /// <summary>
        /// Wait to start
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        protected async Task WaitForContainerStartedAsync(int[] ports) {
            var attempts = 0;
            var sw = Stopwatch.StartNew();
            bool containerStarted;
            do {
                containerStarted = await IsContainerRunningAsync(ports);
                await Task.Delay(1000);
            } while (!containerStarted && attempts++ <= 60);
            sw.Stop();
            if (!containerStarted) {
                throw new TimeoutException($"Container failed to start after {sw.Elapsed}.)");
            }
        }

        /// <summary>
        /// Stop container
        /// </summary>
        /// <param name="containerId"></param>
        protected async Task StopContainerAsync(string containerId) {
            using (var dockerClient = GetDockerClient()) {
                var container = dockerClient.Containers.InspectContainerAsync(containerId).Result;
                if (container.State.Running) {
                    await dockerClient.Containers.KillContainerAsync(containerId,
                        new ContainerKillParameters());
                }
            }
        }

        /// <summary>
        /// Waits for container to have started
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        private async Task<bool> IsContainerRunningAsync(int[] ports) {
            try {
                var remoteEP = new IPEndPoint(IPAddress.Loopback, ports.First());
                var sender = new Socket(IPAddress.Loopback.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                await sender.ConnectAsync(remoteEP);
                sender.Dispose();
                _success++;
                await Task.Delay(1000);
                return await Task.FromResult(_success >= 5);
            }
            catch {
                _success = 0;
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// Create docker client
        /// </summary>
        /// <returns></returns>
        private DockerClient GetDockerClient() {
            return new DockerClientConfiguration(new Uri(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "npipe://./pipe/docker_engine"
                    : "unix:///var/run/docker.sock"
            )).CreateClient();
        }

        private int _success = 0;
        private readonly ILogger _logger;
    }
}
