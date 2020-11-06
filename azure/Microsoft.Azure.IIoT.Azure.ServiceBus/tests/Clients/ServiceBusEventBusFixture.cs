// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Clients {
    using Microsoft.Azure.IIoT.Azure.ServiceBus.Runtime;
    using Microsoft.Azure.IIoT.Messaging.Services;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Runtime.Serialization;
    using Autofac;
    using Microsoft.Extensions.Options;

    public sealed class ServiceBusEventBusFixture : IDisposable {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        internal ServiceBusEventBusHarness GetHarness(string bus,
            Action<ContainerBuilder> configure = null) {
            if (Skip) {
                return new ServiceBusEventBusHarness(null, null);
            }
            return new ServiceBusEventBusHarness(bus, configure);
        }

        /// <inheritdoc/>
        public void Dispose() {
        }
    }

    public class Pid : IProcessIdentity {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string ServiceId { get; } = Guid.NewGuid().ToString();
        public string Name { get; } = "test";
        public string Description { get; } = "the test";
    }

    internal sealed class ServiceBusEventBusHarness : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public ServiceBusEventBusHarness(string bus, Action<ContainerBuilder> configure = null) {
            try {
                var builder = new ContainerBuilder();

                // Read connections string from keyvault
                var config = new ConfigurationBuilder()
                    .AddFromDotEnvFile()
                    .AddFromKeyVault()
                    .Build();
                builder.AddConfiguration(config);
                builder.RegisterType<ServiceBusConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                builder.RegisterInstance(new ConfigureOptions<ServiceBusEventBusOptions>(options => options.Topic = bus))
                    .AsImplementedInterfaces();
                builder.RegisterType<Pid>().SingleInstance()
                    .AsImplementedInterfaces();

                builder.RegisterModule<ServiceBusEventBusSupport>();
                builder.RegisterModule<NewtonSoftJsonModule>();

                builder.RegisterType<ServiceBusConfig>()
                    .AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<HostAutoStart>()
                    .AutoActivate()
                    .AsImplementedInterfaces().SingleInstance();

                builder.AddDiagnostics();
                configure?.Invoke(builder);
                _container = builder.Build();
                _topic = bus;
            }
            catch {
                _container = null;
            }
        }

        /// <summary>
        /// Get Event Bus
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventBus GetEventBus() {
            return Try.Op(() => _container?.Resolve<IEventBus>());
        }

        /// <summary>
        /// Get Event Bus
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public EventBusHost GetEventBusHost() {
            return Try.Op(() => _container?.Resolve<EventBusHost>());
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (_container == null) {
                return;
            }
            var config = _container.Resolve<IOptions<ServiceBusOptions>>();
            var pid = _container.Resolve<IProcessIdentity>();
            var managementClient = new ManagementClient(config.Value.ServiceBusConnString);
            Try.Op(() => managementClient.DeleteSubscriptionAsync(_topic, pid.ServiceId).Wait());
            managementClient.DeleteTopicAsync(_topic).Wait();
            _container.Dispose();
        }

        private readonly IContainer _container;
        private readonly string _topic;
    }

    [DataContract]
    public sealed class Parent {
        [DataMember]
        public string FamilyName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public DateTimeOffset Dob { get; set; }
    }

    [DataContract]
    public sealed class Child {
        [DataMember]
        public string FamilyName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string Gender { get; set; }
        [DataMember]
        public int Grade { get; set; }
        [DataMember]
        public Pet[] Pets { get; set; }
        [DataMember]
        public DateTime? Dob { get; set; }
    }

    [DataContract]
    public sealed class Pet {
        [DataMember]
        public string GivenName { get; set; }
        [DataMember]
        public DateTimeOffset? Dob { get; set; }
    }

    [DataContract]
    public sealed class Address {
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string County { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public TimeSpan? LivedAt { get; set; }
    }

    [DataContract]
    public sealed class Family {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public Parent[] Parents { get; set; }
        [DataMember]
        public Child[] Children { get; set; }
        [DataMember]
        public Address Address { get; set; }
        [DataMember]
        public bool IsRegistered { get; set; }
        [DataMember]
        public DateTime RegistrationDate { get; set; }
        [DataMember]
        public TimeSpan ExistsFor { get; set; }
        [DataMember]
        public int? Count { get; set; }

        public int Ignored { get; set; }
    }
}