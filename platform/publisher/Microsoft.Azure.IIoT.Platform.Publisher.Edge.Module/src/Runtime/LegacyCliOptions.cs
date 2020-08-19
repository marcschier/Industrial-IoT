﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Module.Runtime {
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Edge.Services;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Configuration;
    using Mono.Options;
    using Opc.Ua;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Class that represents a dictionary with all command line arguments from the legacy version of the OPC Publisher
    /// </summary>
    public class LegacyCliOptions : Dictionary<string, string>, ILegacyCliModelProvider {

        /// <summary>
        /// Creates a new instance of the the legacy CLI options based on existing configuration values.
        /// </summary>
        /// <param name="config"></param>
        public LegacyCliOptions(IConfiguration config) {
            foreach (var item in config.GetChildren()) {
                this[item.Key] = item.Value;
            }
            LegacyCliModel = ToLegacyCliModel();
        }

        // TODO: Figure out which are actually supported in the new publisher implementation

        /// <summary>
        /// Parse arguments and set values in the environment the way the new configuration expects it.
        /// </summary>
        /// <param name="args">The specified command line arguments.</param>
        public LegacyCliOptions(string[] args) {

            // command line options
            var options = new Mono.Options.OptionSet {
                    // Publisher configuration options
                    { "pf|publishfile=", "The filename to configure the nodes to publish.",
                        s => this[LegacyCliConfigKeys.PublisherNodeConfigurationFilename] = s },

                    { "di|diagnosticsinterval=", "Shows publisher diagnostic info at the specified interval " +
                        "in seconds (need log level info).\n-1 disables remote diagnostic log and diagnostic output",
                        (int i) => this[LegacyCliConfigKeys.DiagnosticsInterval] = TimeSpan.FromSeconds(i).ToString() },
                    { "lf|logfile=", "The filename of the logfile to use.",
                        s => this[LegacyCliConfigKeys.LogFileName] = s },
                    { "lt|logflushtimespan=", "The timespan in seconds when the logfile should be flushed.",
                        (int i) => this[LegacyCliConfigKeys.LogFileFlushTimeSpanSec] = TimeSpan.FromSeconds(i).ToString() },
                    { "ll|loglevel=", "The loglevel to use (allowed: fatal, error, warn, info, debug, verbose).",
                        (LogEventLevel l) => LogControl.Level.MinimumLevel = l },
                    { "ih|iothubprotocol=", "Protocol to use for communication with the hub. " +
                            $"(allowed values: {string.Join(", ", Enum.GetNames(typeof(TransportOption)))}).",
                        (TransportOption p) => this[LegacyCliConfigKeys.HubTransport] = p.ToString() },
                    { "dc|deviceconnectionstring=", "A device or edge module connection string to use.",
                        dc => this[LegacyCliConfigKeys.EdgeHubConnectionString] = dc },
                    { "ec|edgehubconnectionstring=", "An edge module connection string to use",
                        dc => this[LegacyCliConfigKeys.EdgeHubConnectionString] = dc },

                    { "hb|heartbeatinterval=", "The publisher is using this as default value in seconds " +
                        "for the heartbeat interval setting of nodes without a heartbeat interval setting.",
                        (int i) => this[LegacyCliConfigKeys.HeartbeatIntervalDefault] = TimeSpan.FromSeconds(i).ToString() },
                    { "sf|skipfirstevent=", "The publisher is using this as default value for the skip first " +
                        "event setting of nodes without a skip first event setting.",
                        (bool b) => this[LegacyCliConfigKeys.SkipFirstDefault] = b.ToString() },

                    { "fm|fullfeaturedmessage=", "The full featured mode for messages (all fields filled in)." +
                        "Default is 'true', for legacy compatibility use 'false'",
                        (bool b) => this[LegacyCliConfigKeys.FullFeaturedMessage] = b.ToString() },

                    // Client settings
                    { "ot|operationtimeout=", "The operation timeout of the publisher OPC UA client in ms.",
                        (uint i) => this[LegacyCliConfigKeys.OpcOperationTimeout] = TimeSpan.FromMilliseconds(i).ToString() },
                    { "oi|opcsamplinginterval=", "Default value in milliseconds to request the servers to " +
                        "sample values.",
                        (int i) => this[LegacyCliConfigKeys.OpcSamplingInterval] = TimeSpan.FromMilliseconds(i).ToString() },
                    { "op|opcpublishinginterval=", "Default value in milliseconds for the publishing interval " +
                            "setting of the subscriptions against the OPC UA server.",
                        (int i) => this[LegacyCliConfigKeys.OpcPublishingInterval] = TimeSpan.FromMilliseconds(i).ToString() },
                    { "ct|createsessiontimeout=", "The timeout in seconds used when creating a session to an endpoint.",
                        (uint u) => this[LegacyCliConfigKeys.OpcSessionCreationTimeout] = TimeSpan.FromSeconds(u).ToString() },
                    { "ki|keepaliveinterval=", "The interval in seconds the publisher is sending keep alive messages " +
                            "to the OPC servers on the endpoints it is connected to.",
                        (int i) => this[LegacyCliConfigKeys.OpcKeepAliveIntervalInSec] = TimeSpan.FromSeconds(i).ToString() },
                    { "kt|keepalivethreshold=", "Specify the number of keep alive packets a server can miss, " +
                        "before the session is disconneced.",
                        (uint u) => this[LegacyCliConfigKeys.OpcKeepAliveDisconnectThreshold] = u.ToString() },
                    { "fd|fetchdisplayname=", "Fetches the displayname for the monitored items subscribed.",
                        (bool b) => this[LegacyCliConfigKeys.FetchOpcNodeDisplayName] = b.ToString() },
                    { "sw|sessionconnectwait=", "Wait time in seconds publisher is trying to connect " +
                        "to disconnected endpoints and starts monitoring unmonitored items.",
                        (int s) => this[LegacyCliConfigKeys.SessionConnectWaitSec] = TimeSpan.FromSeconds(s).ToString() },
                    { "mq|monitoreditemqueuecapacity=", "Default queue size for monitored items.",
                        (uint u) => this[LegacyCliConfigKeys.DefaultQueueSize] = u.ToString() },

                    // cert store option
                    { "aa|autoaccept", "The publisher trusts all servers it is establishing a connection to.",
                          b => this[LegacyCliConfigKeys.AutoAcceptCerts] = (b != null).ToString() },
                    { "tm|trustmyself", "The publisher certificate is put into the trusted store automatically.",
                        t => this[LegacyCliConfigKeys.TrustMyself] = (t != null).ToString() },
                    { "at|appcertstoretype=", "The own application cert store type (allowed: Directory, X509Store).",
                        s => {
                            if (s.Equals(CertificateStoreType.X509Store, StringComparison.OrdinalIgnoreCase) ||
                                s.Equals(CertificateStoreType.Directory, StringComparison.OrdinalIgnoreCase)) {
                                this[LegacyCliConfigKeys.OpcOwnCertStoreType] = s;
                                return;
                            }
                            throw new OptionException("Bad store type", "at");
                        }
                    },
                    { "ap|appcertstorepath=", "The path where the own application cert should be stored.",
                        s => this[LegacyCliConfigKeys.OpcOwnCertStorePath] = s },
                    { "tp|trustedcertstorepath=", "The path of the trusted cert store.",
                        s => this[LegacyCliConfigKeys.OpcTrustedCertStorePath] = s },
                    { "tt|trustedcertstoretype=", "Legacy - do not use.", _ => {} },
                    { "rp|rejectedcertstorepath=", "The path of the rejected cert store.",
                        s => this[LegacyCliConfigKeys.OpcRejectedCertStorePath] = s },
                    { "rt|rejectedcertstoretype=", "Legacy - do not use.", _ => {} },
                    { "ip|issuercertstorepath=", "The path of the trusted issuer cert store.",
                        s => this[LegacyCliConfigKeys.OpcIssuerCertStorePath] = s },
                    { "it|issuercertstoretype=", "Legacy - do not use.", _ => {} },
                    { "bs|batchsize=", "The size of message batching buffer.",
                        (int i) => this[LegacyCliConfigKeys.BatchSize] = i.ToString() },
                    { "si|iothubsendinterval=", "The trigger batching interval in seconds.",
                        (int k) => this[LegacyCliConfigKeys.BatchTriggerInterval] = TimeSpan.FromSeconds(k).ToString() },
                    { "ms|iothubmessagesize=", "The maximum size of the (IoT D2C) message.",
                        (int i) => this[LegacyCliConfigKeys.MaxMessageSize] = i.ToString() },

                    // testing purposes
                    { "sc|scaletestcount=", "The number of monitored item clones in scale tests.",
                        (int i) => this[LegacyCliConfigKeys.ScaleTestCount] = i.ToString() },
                    { "mm|messagingmode=", "The messaging scheme " +
                        $"(allowed values: {string.Join(", ", Enum.GetNames(typeof(MessageSchema)))}).",
                        (MessageSchema m) => this[LegacyCliConfigKeys.Schema] = m.ToString() },
                    { "me|messageencoding=", "The message encoding for messages " +
                        $"(allowed values: {string.Join(", ", Enum.GetNames(typeof(MessageEncoding)))}).",
                        (MessageEncoding m) => this[LegacyCliConfigKeys.Encoding] = m.ToString() },

                    // Legacy unsupported
                    { "s|site=", "Legacy - do not use.", _ => {} },
                    { "ol|opcmaxstringlen=", "Legacy - do not use.", _ => {} },
                    { "tc|telemetryconfigfile=", "Legacy - do not use.", _ => {} },
                    { "ic|iotcentral=", "Legacy - do not use.", _ => {} },
                    { "ns|noshutdown=", "Legacy - do not use.", _ => {} },
                    { "rf|runforever", "Legacy - do not use.", _ => {} },
                    { "pn|portnum=", "Legacy - do not use.", _ => {} },
                    { "pa|path=", "Legacy - do not use.", _ => {} },
                    { "lr|ldsreginterval=", "Legacy - do not use.", _ => {} },
                    { "ss|suppressedopcstatuscodes=", "Legacy - do not use.", _ => {} },
                    { "csr", "Legacy - do not use.", _ => {} },
                    { "ab|applicationcertbase64=", "Legacy - do not use.", _ => {} },
                    { "af|applicationcertfile=", "Legacy - do not use.", _ => {} },
                    { "pk|privatekeyfile=", "Legacy - do not use.", _ => {} },
                    { "pb|privatekeybase64=", "Legacy - do not use.", _ => {} },
                    { "cp|certpassword=", "Legacy - do not use.", _ => {} },
                    { "tb|addtrustedcertbase64=", "Legacy - do not use.", _ => {} },
                    { "tf|addtrustedcertfile=", "Legacy - do not use.", _ => {} },
                    { "ib|addissuercertbase64=", "Legacy - do not use.", _ => {} },
                    { "if|addissuercertfile=", "Legacy - do not use.", _ => {} },
                    { "rb|updatecrlbase64=", "Legacy - do not use.", _ => {} },
                    { "uc|updatecrlfile=", "Legacy - do not use.", _ => {} },
                    { "rc|removecert=", "Legacy - do not use.", _ => {} },
                    { "dt|devicecertstoretype=", "Legacy - do not use.", _ => {} },
                    { "dp|devicecertstorepath=", "Legacy - do not use.", _ => {} },
                    { "i|install", "Legacy - do not use.", _ => {} },
                    { "st|opcstacktracemask=", "Legacy - do not use.", _ => {} },
                    { "sd|shopfloordomain=", "Legacy - do not use.", _ => {} },
                    { "vc|verboseconsole=", "Legacy - do not use.", _ => {} },
                    { "as|autotrustservercerts=", "Legacy - do not use.", _ => {} }
                };
            options.Parse(args);

            LegacyCliModel = ToLegacyCliModel();
        }

        /// <summary>
        /// check if we're running in standalone mode - default publishednodes.json file accessible
        /// </summary>
        public bool RunInLegacyMode => System.IO.File.Exists(
            GetValueOrDefault(LegacyCliConfigKeys.PublisherNodeConfigurationFilename,
                LegacyCliConfigKeys.DefaultPublishedNodesFilename));

        /// <summary>
        /// The model of the CLI arguments.
        /// </summary>
        public LegacyCliModel LegacyCliModel { get; }

        /// <summary>
        /// Gets the additiona loggerConfiguration that represents the command line arguments.
        /// </summary>
        /// <returns></returns>
        public LoggerConfiguration ToLoggerConfiguration() {
            LoggerConfiguration loggerConfiguration = null;
            if (!string.IsNullOrWhiteSpace(LegacyCliModel.LogFilename)) {
                loggerConfiguration ??= new LoggerConfiguration();
                loggerConfiguration = loggerConfiguration.WriteTo.File(
                    LegacyCliModel.LogFilename, flushToDiskInterval: LegacyCliModel.LogFileFlushTimeSpan);
            }
            return loggerConfiguration;
        }

        private LegacyCliModel ToLegacyCliModel() {
            return new LegacyCliModel {
                PublishedNodesFile = GetValueOrDefault(LegacyCliConfigKeys.PublisherNodeConfigurationFilename,
                    LegacyCliConfigKeys.DefaultPublishedNodesFilename),
                SessionConnectWait = GetValueOrDefault(LegacyCliConfigKeys.SessionConnectWaitSec, TimeSpan.FromSeconds(15)),
                DefaultHeartbeatInterval = GetValueOrDefault(LegacyCliConfigKeys.HeartbeatIntervalDefault, TimeSpan.Zero),
                DefaultSkipFirst = GetValueOrDefault(LegacyCliConfigKeys.SkipFirstDefault, false),
                DefaultSamplingInterval = GetValueOrDefault(LegacyCliConfigKeys.OpcSamplingInterval, TimeSpan.FromSeconds(1)),
                DefaultPublishingInterval = GetValueOrDefault(LegacyCliConfigKeys.OpcPublishingInterval, TimeSpan.FromSeconds(1)),
                FetchOpcNodeDisplayName = GetValueOrDefault(LegacyCliConfigKeys.FetchOpcNodeDisplayName, false),
                DefaultQueueSize = GetValueOrDefault<uint>(LegacyCliConfigKeys.DefaultQueueSize, 1),
                DiagnosticsInterval = GetValueOrDefault(LegacyCliConfigKeys.DiagnosticsInterval, TimeSpan.FromSeconds(60)),
                LogFileFlushTimeSpan = GetValueOrDefault(LegacyCliConfigKeys.LogFileFlushTimeSpanSec, TimeSpan.FromSeconds(30)),
                LogFilename = GetValueOrDefault<string>(LegacyCliConfigKeys.LogFileName, null),
                Transport = GetValueOrDefault(LegacyCliConfigKeys.HubTransport, TransportType.Mqtt.ToString()),
                Schema = GetValueOrDefault(LegacyCliConfigKeys.Schema, MessageSchema.Samples),
                Encoding = GetValueOrDefault(LegacyCliConfigKeys.Encoding, MessageEncoding.Json),
                FullFeaturedMessage = GetValueOrDefault(LegacyCliConfigKeys.FullFeaturedMessage, false),
                EdgeHubConnectionString = GetValueOrDefault<string>(LegacyCliConfigKeys.EdgeHubConnectionString, null),
                OperationTimeout = GetValueOrDefault(LegacyCliConfigKeys.OpcOperationTimeout, TimeSpan.FromSeconds(15)),
                SessionCreationTimeout = GetValueOrDefault(LegacyCliConfigKeys.OpcSessionCreationTimeout, TimeSpan.FromSeconds(1)),
                KeepAliveInterval = GetValueOrDefault(LegacyCliConfigKeys.OpcKeepAliveIntervalInSec, TimeSpan.FromSeconds(10)),
                MaxKeepAliveCount = GetValueOrDefault(LegacyCliConfigKeys.OpcKeepAliveDisconnectThreshold, 50),
                TrustSelf = GetValueOrDefault(LegacyCliConfigKeys.TrustMyself, true),
                AutoAcceptUntrustedCertificates = GetValueOrDefault(LegacyCliConfigKeys.AutoAcceptCerts, false),
                ApplicationCertificateStoreType = GetValueOrDefault(LegacyCliConfigKeys.OpcOwnCertStoreType, "Directory"),
                ApplicationCertificateStorePath = GetValueOrDefault(LegacyCliConfigKeys.OpcOwnCertStorePath, "pki/own"),
                TrustedPeerCertificatesPath = GetValueOrDefault(LegacyCliConfigKeys.OpcTrustedCertStorePath, "pki/trusted"),
                RejectedCertificateStorePath = GetValueOrDefault(LegacyCliConfigKeys.OpcRejectedCertStorePath, "pki/rejected"),
                TrustedIssuerCertificatesPath = GetValueOrDefault(LegacyCliConfigKeys.OpcIssuerCertStorePath, "pki/issuer"),
                BatchSize = GetValueOrDefault(LegacyCliConfigKeys.BatchSize, 50),
                BatchTriggerInterval = GetValueOrDefault<TimeSpan>(LegacyCliConfigKeys.BatchTriggerInterval, TimeSpan.FromSeconds(10)),
                MaxMessageSize = GetValueOrDefault(LegacyCliConfigKeys.MaxMessageSize, 0u),
                ScaleTestCount = GetValueOrDefault(LegacyCliConfigKeys.ScaleTestCount, 1)
            };
        }

        /// <summary>
        /// Helper to get default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private T GetValueOrDefault<T>(string key, T defaultValue) {
            if (!ContainsKey(key)) {
                return defaultValue;
            }
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFrom(this[key]);
        }
    }
}