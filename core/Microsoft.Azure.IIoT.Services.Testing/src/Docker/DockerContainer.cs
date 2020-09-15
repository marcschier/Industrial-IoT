// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Docker {
    using Serilog;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Threading;
    using global::Docker.DotNet;
    using global::Docker.DotNet.Models;

    /// <summary>
    /// Represents a rabbit mq server instance
    /// </summary>
    public abstract class DockerContainer {

        /// <summary>
        /// Network name
        /// </summary>
        public string NetworkName { get; set; }

        /// <summary>
        /// Container name
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="networkName"></param>
        public DockerContainer(ILogger logger, string networkName = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            NetworkName = networkName;
        }

        /// <summary>
        /// Start container
        /// </summary>
        /// <param name="containerParameters"></param>
        /// <param name="containerName"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        protected async Task<(string, bool)> StartContainerAsync(
            CreateContainerParameters containerParameters, string containerName,
            string imageName) {
            using (var dockerClient = GetDockerClient()) {
                ContainerName = containerName;
                containerParameters.Image = imageName;
                await CreateNetworkIfNotExistsAsync(dockerClient);

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
                    var images = await dockerClient.Images.ListImagesAsync(
                        new ImagesListParameters { MatchName = imageName });
                    if (!images.Any()) {
                        var tag = imageName.Split(':').Last();
                        var imagesCreateParameters = new ImagesCreateParameters {
                            FromImage = imageName,
                            Tag = tag,
                        };
                        await dockerClient.Images.CreateImageAsync(
                            imagesCreateParameters, new AuthConfig(), null, default);
                    }
                    containerParameters.Name = containerName;
                    if (!string.IsNullOrEmpty(NetworkName)) {
                        containerParameters.HostConfig ??= new HostConfig();
                        containerParameters.HostConfig.NetworkMode = NetworkName;
                    }
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
        /// <param name="port"></param>
        /// <returns></returns>
        protected async Task WaitForContainerStartedAsync(int port) {
            var attempts = 0;
            var sw = Stopwatch.StartNew();
            bool containerStarted;
            do {
                containerStarted = await IsContainerRunningAsync(port);
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
                var container = await dockerClient.Containers.InspectContainerAsync(containerId);
                if (container.State.Running) {
                    ContainerName = null;
                    await dockerClient.Containers.KillContainerAsync(containerId,
                        new ContainerKillParameters());
                }
            }
        }

        /// <summary>
        /// Waits for container to have started
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private async Task<bool> IsContainerRunningAsync(int port) {
            try {
                var remoteEP = new IPEndPoint(IPAddress.Loopback, port);
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
        /// Helper to create network if needed
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        private async Task CreateNetworkIfNotExistsAsync(DockerClient dockerClient) {
            if (string.IsNullOrEmpty(NetworkName)) {
                return;
            }
            await _lock.WaitAsync();
            try {
                var networks = await dockerClient.Networks.ListNetworksAsync(
                    new NetworksListParameters());
                if (!networks.Any(n => n.Name == NetworkName)) {
                    await dockerClient.Networks.CreateNetworkAsync(
                        new NetworksCreateParameters { Name = NetworkName });
                }
            }
            finally {
                _lock.Release();
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
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    }
}
