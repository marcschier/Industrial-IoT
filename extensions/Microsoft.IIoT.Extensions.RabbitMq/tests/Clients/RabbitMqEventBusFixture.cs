﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq.Clients {
    using Microsoft.IIoT.Extensions.Messaging.Services;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Runtime.Serialization;
    using Autofac;

    public sealed class RabbitMqEventBusFixture {

        public bool Skip { get; set; }

        /// <summary>
        /// Create test harness
        /// </summary>
        /// <returns></returns>
        public RabbitMqEventBusHarness GetHarness(string bus,
            Action<ContainerBuilder> configure = null) {

            if (Skip || !RabbitMqServerFixture.Up) {
                return new RabbitMqEventBusHarness();
            }
            return new RabbitMqEventBusHarness(bus, configure);
        }
    }

    public sealed class RabbitMqEventBusHarness : IDisposable {

        /// <summary>
        /// Create fixture
        /// </summary>
        public RabbitMqEventBusHarness(string bus, Action<ContainerBuilder> configure = null) {
            try {
                var builder = new ContainerBuilder();

                builder.RegisterModule<NewtonSoftJsonModule>();
                builder.RegisterModule<RabbitMqEventBusModule>();
                builder.Configure<RabbitMqOptions>(options => options.RoutingKey = bus);
                builder.RegisterType<HostAutoStart>()
                    .AutoActivate()
                    .AsImplementedInterfaces().SingleInstance();

                builder.AddDiagnostics();
                configure?.Invoke(builder);
                _container = builder.Build();
            }
            catch {
                _container = null;
            }
        }

        public RabbitMqEventBusHarness() {
            _container = null;
        }

        /// <summary>
        /// Get Event Bus Publisher
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventBusPublisher GetEventBusPublisher() {
            return Try.Op(() => _container?.Resolve<IEventBusPublisher>());
        }

        /// <summary>
        /// Get Event Bus Subscriber
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEventBusSubscriber GetEventBusSubscriber() {
            return Try.Op(() => _container?.Resolve<IEventBusSubscriber>());
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