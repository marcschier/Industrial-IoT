// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Clients {
    using Microsoft.Azure.IIoT.Messaging.Services;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Runtime.Serialization;
    using Autofac;

    public sealed class OrleansEventBusFixture : IDisposable {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        public OrleansEventBusHarness GetHarness(string bus,
            Action<ContainerBuilder> configure = null) {
            if (Skip) {
                return null;
            }
            return new OrleansEventBusHarness(bus, configure);
        }

        /// <inheritdoc/>
        public void Dispose() {
        }
    }

    public sealed class OrleansEventBusHarness : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public OrleansEventBusHarness(string bus, Action<ContainerBuilder> configure = null) {
            if (bus is null) {
                throw new ArgumentNullException(nameof(bus));
            }
            try {
                var builder = new ContainerBuilder();
                builder.AddDiagnostics();
                builder.RegisterModule<NewtonSoftJsonModule>();

                //
                // Add test cluster client - orleans bus module will then
                // not register the generic client, however if run with
                // silo host it will.
                //
                if (OrleansClusterFixture.Client != null) {
                    builder.RegisterInstance(OrleansClusterFixture.Client)
                        .ExternallyOwned();
                }

                // Configure the bus
                builder.Configure<OrleansBusOptions>(options => options.Prefix = bus);

                // Now add module
                builder.RegisterModule<OrleansEventBusModule>();

                // Add additional services
                configure?.Invoke(builder);

                // Start it all
                builder.RegisterType<HostAutoStart>()
                    .AutoActivate()
                    .AsImplementedInterfaces().SingleInstance();
                _container = builder.Build();
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
            _container?.Dispose();
        }

        private readonly IContainer _container;
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