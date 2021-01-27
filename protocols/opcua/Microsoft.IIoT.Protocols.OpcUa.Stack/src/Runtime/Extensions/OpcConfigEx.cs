﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa {
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Utils;
    using Opc.Ua;
    using Opc.Ua.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Configuration extensions
    /// </summary>
    public static class OpcConfigEx {

        /// <summary>
        /// Create application configuration
        /// </summary>
        /// <param name="opcConfig"></param>
        /// <param name="identity"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static async Task<ApplicationConfiguration> ToApplicationConfigurationAsync(
            this IClientServicesConfig opcConfig, IIdentity identity,
            CertificateValidationEventHandler handler) {
            if (opcConfig is null) {
                throw new ArgumentNullException(nameof(opcConfig));
            }
            if (string.IsNullOrWhiteSpace(opcConfig.ApplicationName)) {
                throw new ArgumentException("Missing application name", nameof(opcConfig));
            }

            // wait with the configuration until network is up
            for (var retry = 0; retry < 3; retry++) {
                if (NetworkInterface.GetIsNetworkAvailable()) {
                    break;
                }
                else {
                    await Task.Delay(3000).ConfigureAwait(false);
                }
            }

            var applicationConfiguration = new ApplicationConfiguration {
                ApplicationName = opcConfig.ApplicationName,
                ProductUri = opcConfig.ProductUri,
                ApplicationType = ApplicationType.Client,
                TransportQuotas = opcConfig.ToTransportQuotas(),
                CertificateValidator = new CertificateValidator(),
                ClientConfiguration = new ClientConfiguration(),
                ServerConfiguration = new ServerConfiguration()
            };
            try {
                await Retry.WithLinearBackoff(null, new CancellationToken(),
                    async () => {
                        //  try to resolve the hostname
                        var hostname = !string.IsNullOrWhiteSpace(identity?.Gateway) ?
                            identity.Gateway : !string.IsNullOrWhiteSpace(identity?.DeviceId) ?
                                identity.DeviceId : Utils.GetHostName();
                        var alternateBaseAddresses = new List<string>();
                        try {
                            alternateBaseAddresses.Add($"urn://{hostname}");
                            var hostEntry = Dns.GetHostEntry(hostname);
                            if (hostEntry != null) {
                                alternateBaseAddresses.Add($"urn://{hostEntry.HostName}");
                                foreach (var alias in hostEntry.Aliases) {
                                    alternateBaseAddresses.Add($"urn://{alias}");
                                }
                                foreach (var ip in hostEntry.AddressList) {
                                    // only ad IPV4 addresses
                                    switch (ip.AddressFamily) {
                                        case AddressFamily.InterNetwork:
                                            alternateBaseAddresses.Add($"urn://{ip}");
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        catch { }

                        applicationConfiguration.ApplicationUri =
                            opcConfig.ApplicationUri.Replace("urn:localhost", $"urn:{hostname}");
                        applicationConfiguration.SecurityConfiguration =
                            opcConfig.ToSecurityConfiguration(hostname);
                        applicationConfiguration.ServerConfiguration.AlternateBaseAddresses =
                            alternateBaseAddresses.ToArray();
                        await applicationConfiguration.Validate(applicationConfiguration.ApplicationType).ConfigureAwait(false);
                        var application = new ApplicationInstance(applicationConfiguration);
                        var hasAppCertificate = await application.CheckApplicationInstanceCertificate(true,
                            CertificateFactory.DefaultKeySize).ConfigureAwait(false);
                        if (!hasAppCertificate) {
                            throw new InvalidConfigurationException("OPC UA application certificate invalid");
                        }

                        applicationConfiguration.CertificateValidator.CertificateValidation += handler;
                        await applicationConfiguration.CertificateValidator
                            .Update(applicationConfiguration.SecurityConfiguration).ConfigureAwait(false);
                    },
                    e => true, 5).ConfigureAwait(false);
            }
            catch (Exception e) {
                throw new InvalidConfigurationException("OPC UA configuration not valid", e);
            }
            return applicationConfiguration;
        }
    }
}