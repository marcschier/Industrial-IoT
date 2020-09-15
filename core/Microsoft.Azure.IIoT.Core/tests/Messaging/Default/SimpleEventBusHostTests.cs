// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Default {
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;
    using System.Collections.Generic;
    using Autofac;
    using System.Linq;
    using Microsoft.Azure.IIoT.Utils;

    public class SimpleEventBusHostTests : IClassFixture<SimpleEventBusFixture> {
        private readonly SimpleEventBusFixture _fixture;

        public SimpleEventBusHostTests(SimpleEventBusFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task PublishTest1Async() {
            var families = new FamilyHandler(2);
            var children1 = new ChildHandler(4);
            var children2 = new ChildHandler(4);
            var pets = new PetHandler(3);
            using (var harness = _fixture.GetHarness(builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(children1).AsImplementedInterfaces();
                builder.RegisterInstance(children2).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family1 = new Fixture().Create<Family>();
                var family2 = new Fixture().Create<Family>();

                var child1 = new Fixture().Create<Child>();
                var child2 = new Fixture().Create<Child>();
                var child3 = new Fixture().Create<Child>();
                var child4 = new Fixture().Create<Child>();

                var pet1 = new Fixture().Create<Pet>();
                var pet2 = new Fixture().Create<Pet>();
                var pet3 = new Fixture().Create<Pet>();

                await bus.PublishAsync(family1);
                await bus.PublishAsync(child1);
                await bus.PublishAsync(pet2);
                await bus.PublishAsync(child2);
                await bus.PublishAsync(family2);
                await bus.PublishAsync(child3);
                await bus.PublishAsync(pet1);
                await bus.PublishAsync(child4);
                await bus.PublishAsync(child1);
                await bus.PublishAsync(family1);
                await bus.PublishAsync(family2);
                await bus.PublishAsync(pet3);
                await bus.PublishAsync(pet3);

                var f = await families.Complete.Task;
                var c1 = await children1.Complete.Task;
                var c2 = await children2.Complete.Task;
                var p = await pets.Complete.Task;

                Assert.True(f.SetEqualsSafe(family1.YieldReturn().Append(family2)));
                Assert.True(p.SetEqualsSafe(pet1.YieldReturn().Append(pet2).Append(pet3)));
                Assert.True(c1.SetEqualsSafe(child1.YieldReturn().Append(child2).Append(child3).Append(child4)));
                Assert.True(c2.SetEqualsSafe(child1.YieldReturn().Append(child2).Append(child3).Append(child4)));
            }
        }

        [SkippableFact]
        public async Task PublishTest2Async() {
            var families = new FamilyHandler(100);
            var pets = new PetHandler(100);
            using (var harness = _fixture.GetHarness(builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var senders = Enumerable.Range(0, 100).Select(async i => {
                    await bus.PublishAsync(new Fixture().Create<Family>());
                    await bus.PublishAsync(new Fixture().Create<Pet>());
                });

                await Task.WhenAll(senders);

                var f = await families.Complete.Task;
                var p = await pets.Complete.Task;

                Assert.Equal(100, f.Count);
                Assert.Equal(100, p.Count);
            }
        }

        [SkippableFact]
        public async Task BadArgumentsAndInvalidStateTests1Async() {
            var families = new FamilyHandler(100);
            var pets = new PetHandler(100);
            using (var harness = _fixture.GetHarness(builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var host = harness.GetEventBusHost();

                await host.StopAsync();
                await host.StartAsync();

                // Already running - should throw
                await Assert.ThrowsAsync<ResourceInvalidStateException>(host.StartAsync);

                await host.StopAsync();
                await host.StopAsync();
                await host.StartAsync();

                // Should throw
                await Assert.ThrowsAsync<ResourceInvalidStateException>(host.StartAsync);
            }
        }

        [SkippableFact]
        public async Task BadArgumentsAndInvalidStateTests2Async() {
            var harness = _fixture.GetHarness();
            try {
                var host = harness.GetEventBusHost();
                await host.StopAsync();
                await host.StartAsync();
                await host.StartAsync();
                await host.StopAsync();
                await host.StopAsync();
                await host.StartAsync();
                await host.StartAsync();

                host.Dispose();
                Assert.True(true);
            }
            catch (Exception ex) {
                Assert.Null(ex);
            }
            finally {
                Try.Op(() => harness.Dispose()); // purposefully crashes due to double dispose
            }
        }

        public class FamilyHandler : IEventHandler<Family> {
            private readonly int _count;
            public TaskCompletionSource<HashSet<Family>> Complete { get; } =
                new TaskCompletionSource<HashSet<Family>>();

            public FamilyHandler(int count) {
                _count = count;
            }
            public Task HandleAsync(Family eventData) {
                Families.Add(eventData);
                if (Families.Count == _count) {
                    Complete.TrySetResult(Families);
                }
                return Task.CompletedTask;
            }
            public HashSet<Family> Families { get; } = new HashSet<Family>(
                Compare.Using<Family>((x, y) => x.Id == y.Id));
        }

        public class ChildHandler : IEventHandler<Child> {
            private readonly int _count;
            public TaskCompletionSource<HashSet<Child>> Complete { get; } =
                new TaskCompletionSource<HashSet<Child>>();
            public ChildHandler(int count) {
                _count = count;
            }
            public Task HandleAsync(Child eventData) {
                Children.Add(eventData);
                if (Children.Count == _count) {
                    Complete.TrySetResult(Children);
                }
                return Task.CompletedTask;
            }
            public HashSet<Child> Children { get; } = new HashSet<Child>(
                Compare.Using<Child>((x, y) => x.FirstName == y.FirstName));
        }

        public class PetHandler : IEventHandler<Pet> {
            private readonly int _count;
            public TaskCompletionSource<HashSet<Pet>> Complete { get; } =
                new TaskCompletionSource<HashSet<Pet>>();
            public PetHandler(int count) {
                _count = count;
            }
            public Task HandleAsync(Pet eventData) {
                Pets.Add(eventData);
                if (Pets.Count == _count) {
                    Complete.TrySetResult(Pets);
                }
                return Task.CompletedTask;
            }
            public HashSet<Pet> Pets { get; } = new HashSet<Pet>(
                Compare.Using<Pet>((x, y) => x.GivenName == y.GivenName));
        }

    }
}
