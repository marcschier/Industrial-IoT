// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Cli {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Serilog.Events;
    using System;

    /// <summary>
    /// Publisher module host process
    /// </summary>
    public class Program {

        public enum Target {
            None,
            Sdk,
            SdkBatch,
            Raw,
            EventHub,
            EventHubBatch,
            File
        }

#pragma warning disable CS0219 // Variable is assigned but its value is never used
        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(string[] args) {
            var target = Target.None;
            var inputRate = 100000000; // x values total per second
            var notifications = 1000000; // over x messages
            var outputRate = 0; // x network messages per second (0 == max)
            var mode = MessageSchema.Samples;
            var encoding = MessageEncoding.Json;
            var batchInterval = TimeSpan.FromMilliseconds(500);
            var batchSize = 50;
            var connectionString = string.Empty;

            Console.WriteLine("Publisher test command line interface.");
            try {
                for (var i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "-?":
                        case "-h":
                        case "--help":
                            throw new ArgumentException("Help");
                        case "-i":
                        case "--input-rate":
                            i++;
                            if (i < args.Length) {
                                if (!int.TryParse(args[i], out inputRate)) {
                                    i--;
                                }
                            }
                            break;
                        case "-o":
                        case "--output-rate":
                            i++;
                            if (i < args.Length) {
                                if (!int.TryParse(args[i], out outputRate)) {
                                    i--;
                                }
                            }
                            break;
                        case "-b":
                        case "--batch-size":
                            i++;
                            if (i < args.Length) {
                                if (!int.TryParse(args[i], out batchSize)) {
                                    i--;
                                }
                            }
                            break;
                        case "-t":
                        case "--batch-timeout":
                            i++;
                            if (i < args.Length) {
                                if (!TimeSpan.TryParse(args[i], out batchInterval)) {
                                    i--;
                                }
                            }
                            break;
                        case "-e":
                        case "--encoding":
                            i++;
                            if (i < args.Length) {
                                if (!Enum.TryParse(args[i], out encoding)) {
                                    i--;
                                }
                            }
                            break;
                        case "-m":
                        case "--mode":
                            i++;
                            if (i < args.Length) {
                                if (!Enum.TryParse(args[i], out mode)) {
                                    i--;
                                }
                            }
                            break;
                        case "-s":
                        case "--send":
                            i++;
                            if (i < args.Length) {
                                if (!Enum.TryParse(args[i], out target)) {
                                    i--;
                                }
                            }
                            break;
                        default:
                            throw new ArgumentException($"Unknown argument {args[i]}.");
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(
                    @"
Usage:       Microsoft.Azure.IIoT.Platform.Publisher.Edge.Cli [options]

Options:

    --batch-timeout
     -t      Batch timeout
    --batch-size
     -b      Batch size
    --input-rate
     -i      Input rate per second
    --output-rate
     -o      Output rate per second
    --encoding
     -e      Encoding (Json/Uadp)
    --mode
     -m      Messaging mode (PubSub/Samples)
    --connection-string
     -c      Device Connection string

    --help
     -?
     -h      Prints out this help.
"
                    );
                return;
            }

            if (target != Target.None && target != Target.File) {
                Console.WriteLine("Enter connection string:");
                connectionString = Console.ReadLine();
            }
            var logger = ConsoleLogger.Create(LogEventLevel.Error);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                logger.Fatal(e.ExceptionObject as Exception, "Exception");
                Console.WriteLine(e);
            };
            try {
          //      RunBenchmarkAsync(inputRate, notifications, batchSize, batchInterval,
          //          mode, encoding, outputRate, logger, connectionString, target).Wait();
            }
            catch (Exception e) {
                logger.Error(e, "Exception");
            }
        }

#pragma warning restore CS0219 // Variable is assigned but its value is never used
#if FALSE
        /// <summary>
        /// Run benchmark
        /// </summary>
        /// <returns></returns>
        private static async Task RunBenchmarkAsync(int rate, int notifications, int batch, TimeSpan interval,
            MessageSchema mode, MessageEncoding encoding, int delay, ILogger logger, string connectionString, Target target) {
            using (var io = new Mock(rate, notifications, batch, interval, mode, encoding, delay, logger,
                connectionString, target))
            using (var process = new DataFlowProcessingEngine(io, CreateEncoder(mode, encoding),
                io, io, logger, io)) {
                await process.RunAsync(Agent.Framework.Models.ProcessMode.Active, CancellationToken.None);
            }
        }

        /// <summary>
        /// Encoder creation
        /// </summary>
        /// <returns></returns>
        private static IMessageEncoder CreateEncoder(MessageSchema mode, MessageEncoding encoding) {
            return mode == MessageSchema.PubSub ? new NetworkMessageEncoder() :
                (IMessageEncoder)new MonitoredItemMessageEncoder();
        }

        /// <summary>
        /// Mock input and output
        /// </summary>
        public class Mock : IMessageTrigger, IMessageSink, IEngineConfiguration, IIdentity {

            public string Id { get; } = "Test";

            public int NumberOfConnectionRetries { get; }
            public long ValueChangesCount => _valueChangesCount;
            public long DataChangesCount => _dataChangesCount;


            public int? BatchSize => _batchSize;
            public TimeSpan? BatchTriggerInterval => _interval;

            public int? MaxMessageSize { get; }
            public TimeSpan? DiagnosticsInterval { get; }

            private readonly Target _target;
            private readonly DeviceClient _iotHubClient;

            public long SendErrorCount { get; }
            public long SentMessagesCount => _sentMessagesCount;
            public string DeviceId { get; } = "Device";
            public string ModuleId { get; } = "Module";
            public string SiteId { get; } = "Site";
            public string Gateway { get; } = "Gateway";

            public Mock(int inputRate, int notifications, int batchSize, TimeSpan interval,
                MessageSchema mode, MessageEncoding encoding, int outputRate, ILogger logger,
                string connectionString, Target target) {
                _inputRate = inputRate < 1 ? int.MaxValue : inputRate;
                _notifications = notifications == 0 || notifications > _inputRate ? 1 : notifications;
                _batchSize = batchSize;
                _interval = interval;
                _mode = mode;
                _encoding = encoding;
                _outputRate = outputRate;
                _logger = logger;
                DiagnosticsInterval = TimeSpan.FromSeconds(10);
                _target = target;
                if (!string.IsNullOrEmpty(connectionString)) {
                    if (target == Target.EventHub || target == Target.EventHubBatch) {
                        var cs = new EventHubsConnectionStringBuilder(connectionString) {

                        }.ToString();
                        _ehClient = EventHubClient.CreateFromConnectionString(cs);
                    }
                    else if (target == Target.Raw) {
                        _rawClient = new RawMqttClient(connectionString);
                    }
                    else if (target == Target.Sdk || _target == Target.SdkBatch) {
                        _iotHubClient = DeviceClient.CreateFromConnectionString(connectionString, new ITransportSettings[] {
                            new MqttTransportSettings(Devices.Client.TransportType.Mqtt_Tcp_Only) {
                               PublishToServerQoS = DotNetty.Codecs.Mqtt.Packets.QualityOfService.AtMostOnce,
                                RemoteCertificateValidationCallback = ValidateCertificate,
                                CertificateRevocationCheck = false
                            }

                      //  new AmqpTransportSettings(Devices.Client.TransportType.Amqp_Tcp_Only) {
                      //      RemoteCertificateValidationCallback = ValidateCertificate,
                      //      CertificateRevocationCheck = false
                      //  }
                    });
                    }
                }
            }

            public event EventHandler<DataSetMessageModel> OnMessage;

            public void Dispose() {
            }

            public Task RunAsync(CancellationToken ct) {
                var tcs = new TaskCompletionSource<bool>();
                var thread = new Thread(_ => {

                    _logger.Information("Starting producer");
                    var sw = new Stopwatch();
                    var cls = Guid.NewGuid();
                    var ctx = new ServiceMessageContext();
                    var w = new DataSetWriterModel {
                        DataSetWriterId = "WriterId",
                        DataSet = new PublishedDataSetModel {
                            Name = null,
                            DataSetMetaData = new DataSetMetaDataModel {
                                ConfigurationVersion = new ConfigurationVersionModel {
                                    MajorVersion = 1,
                                    MinorVersion = 0
                                },
                                DataSetClassId = cls,
                                Name = "Writer"
                            },
                            ExtensionFields = new Dictionary<string, string> {
                                ["EndpointId"] = "Endpoint",
                                ["PublisherId"] = "Publisher",
                                ["DataSetWriterId"] = "WriterId"
                            }
                        },
                        DataSetFieldContentMask =
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.StatusCode |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.SourceTimestamp |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.ServerTimestamp |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.NodeId |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.DisplayName |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.ApplicationUri |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.EndpointUrl |
                                    OpcUa.Publisher.Models.DataSetFieldContentMask.ExtensionFields,
                        MessageSettings = new DataSetWriterMessageSettingsModel() {
                            DataSetMessageContentMask =
                                        DataSetContentMask.Timestamp |
                                        DataSetContentMask.MetaDataVersion |
                                        DataSetContentMask.Status |
                                        DataSetContentMask.DataSetWriterId |
                                        DataSetContentMask.MajorVersion |
                                        DataSetContentMask.MinorVersion |
                                        DataSetContentMask.SequenceNumber
                        },
                    };
                    var g = new WriterGroupModel {
                        MessageType = _encoding,
                        Name = "WriterGroupName",
                        DataSetWriters = new List<DataSetWriterModel> { w},
                        MessageSettings = new WriterGroupMessageSettingsModel() {
                            NetworkMessageContentMask =
                                NetworkMessageContentMask.PublisherId |
                                NetworkMessageContentMask.WriterGroupId |
                                NetworkMessageContentMask.NetworkMessageNumber |
                                NetworkMessageContentMask.SequenceNumber |
                                NetworkMessageContentMask.PayloadHeader |
                                NetworkMessageContentMask.Timestamp |
                                NetworkMessageContentMask.DataSetClassId |
                                NetworkMessageContentMask.NetworkMessageHeader |
                                NetworkMessageContentMask.DataSetMessageHeader
                        },
                        WriterGroupId = "WriterGroup",
                    };

                    // Produce
                    for (var i = 0u; !ct.IsCancellationRequested; ) {
                        sw.Restart();

                        var messages = new List<DataSetMessageModel>();
                        var itemsPerNotification = _inputRate / _notifications;
                        var lastNotification = _inputRate % _notifications;
                        var notifications = lastNotification == 0 ? _notifications : _notifications + 1;
                        if (notifications == _notifications) {
                            lastNotification = itemsPerNotification;
                        }
                        var x = 0;
                        for (var j = 0; j < notifications; j++) {
                            var values = j == notifications - 1 ? lastNotification : itemsPerNotification;
                            var message = new DataSetMessageModel {
                                ApplicationUri = "Application",
                                EndpointUrl = "Endpoint",
                                PublisherId = "Publisher",
                                SequenceNumber = ++i,
                                ServiceMessageContext = ctx,
                                SubscriptionId = "Subscription",
                                Writer = w,
                                WriterGroup = g,
                                Notifications = LinqEx.Repeat(() => new MonitoredItemNotificationModel {
                                    DisplayName = "Value" + ++x,
                                    ClientHandle = (uint)x,
                                    Id = x.ToString(),
                                    NodeId = "i=" + x,
                                    SequenceNumber = i,
                                    Value = new DataValue(new Variant(i), StatusCodes.Good,
                                        DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow),
                                    PublishTime = DateTime.UtcNow,
                                }, values)
                            };
                            Interlocked.Increment(ref _dataChangesCount);
                            Interlocked.Add(ref _valueChangesCount, values);
                            OnMessage?.Invoke(this, message);
                        }
                      //  var elapsed = sw.ElapsedMilliseconds;
                      //  if (elapsed < 1000) {
                      //      Thread.Sleep(1000 - (int)elapsed);
                      //  }
                    }
                    _logger.Information("Ending producer");
                    tcs.SetResult(true);
                });
                thread.Start(); // Produce
                return tcs.Task;
            }

            public async Task SendAsync(IEnumerable<NetworkMessageModel> messages) {
                var numberOfMessages = 0;
                foreach (var message in messages) {
                    numberOfMessages++;
                    Interlocked.Increment(ref _sentMessagesCount);
                    if (_iotHubClient != null && _target == Target.Sdk) {
                        await _iotHubClient.SendEventAsync(new Message(message.Body));
                    }
                    else if (_ehClient != null && _target == Target.EventHub) {
                        await _ehClient.SendAsync(new EventData(message.Body), DateTime.UtcNow.ToString());
                    }
                    else if (_rawClient != null) {
                        await _rawClient.SendAsync(message.Body);
                    }
                    else if (_target == Target.File) {
                        await File.WriteAllBytesAsync("test.json", message.Body);
                    }
                }
                if (_iotHubClient != null && _target == Target.SdkBatch) {
                    await Task.WhenAll(messages.Select(message => _iotHubClient.SendEventAsync(new Message(message.Body))));
                }
                else if (_ehClient != null && _target == Target.EventHubBatch) {
                    await _ehClient.SendAsync(messages.Select(e => new EventData(e.Body)), DateTime.UtcNow.ToString());
                }
                else if (_target == Target.None) {
                    if (_outputRate > 0 && _outputRate < 1000) {
                        await Task.Delay(1000 / _outputRate * numberOfMessages); // Sending rate
                    }
                    else if (_outputRate == 0) {
                      //  await Task.Delay(3000);
                    }
                }
            }

            private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                return true;
            }

            private readonly int _inputRate;
            private readonly int _notifications;
            private readonly int _outputRate;
            private readonly ILogger _logger;
            private long _valueChangesCount;
            private long _dataChangesCount;
            private long _sentMessagesCount;

            private readonly int _batchSize;
            private readonly TimeSpan _interval;
            private readonly MessageSchema _mode;
            private readonly MessageEncoding _encoding;
            private readonly EventHubClient _ehClient;
            private readonly RawMqttClient _rawClient;
        }

        public class RawMqttClient {
            private readonly ConnectionString _cs;
            private MqttClient _client;
            private TaskCompletionSource<bool> _tcs;

            public RawMqttClient(string connectionString) {
                _cs = ConnectionString.Parse(connectionString);
                Connect();
            }

            private void Connect() {
                _client = new MqttClient(_cs.HostName, 8883, true, null, null, MqttSslProtocols.TLSv1_2, Validate);
                _client.ConnectionClosed += _client_ConnectionClosed;
                _client.MqttMsgPublished += _client_MqttMsgPublished;
                _client.MqttMsgPublishReceived += _client_MqttMsgPublishReceived;
                var target = $"{_cs.HostName}/{_cs.DeviceId}";
                var resource = $"{_cs.HostName}/devices/{_cs.DeviceId}";
                var sas = new SharedAccessSignatureBuilder {
                    Key = _cs.SharedAccessKey,
                  //  KeyName = _cs.DeviceId,
                    Target = resource,
                    TimeToLive = TimeSpan.FromHours(1)
                }.ToSignature();
                var result = _client.Connect(_cs.DeviceId, target, sas);
                if (result != 0) {
                    throw new ConnectionException("Refused");
                }
            }

            private void _client_ConnectionClosed(object sender, EventArgs e) {
                Connect();
            }

            private void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) {
                _tcs?.SetResult(true);
            }

            private void _client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e) {
                _tcs?.SetResult(true);
            }

            /// <summary>
            /// Sending
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public Task SendAsync(byte[] buffer) {
                _tcs = new TaskCompletionSource<bool>();
                _client.Publish($"devices/{_cs.DeviceId}/messages/events/", buffer, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                //return _tcs.Task;
                return Task.CompletedTask;
            }

            private bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                return true;
            }


        }
#endif
    }
}
