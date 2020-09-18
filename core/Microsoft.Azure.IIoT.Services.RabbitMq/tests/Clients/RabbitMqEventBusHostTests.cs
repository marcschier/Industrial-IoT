// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using AutoFixture;
    using Autofac;
    using Xunit;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    public class RabbitMqEventBusHostTests : IClassFixture<RabbitMqEventBusFixture> {
        private readonly RabbitMqEventBusFixture _fixture;

        public RabbitMqEventBusHostTests(RabbitMqEventBusFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task PublishTest1Async() {
            var families = new FamilyHandler(2);
            var children1 = new ChildHandler(4);
            var children2 = new ChildHandler(4);
            var pets = new PetHandler(3);
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix, builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(children1).AsImplementedInterfaces();
                builder.RegisterInstance(children2).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var family1 = fix.Create<Family>();
                var family2 = fix.Create<Family>();

                var child1 = fix.Create<Child>();
                var child2 = fix.Create<Child>();
                var child3 = fix.Create<Child>();
                var child4 = fix.Create<Child>();

                var pet1 = fix.Create<Pet>();
                var pet2 = fix.Create<Pet>();
                var pet3 = fix.Create<Pet>();

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

                var f = await families.Complete;
                var c1 = await children1.Complete;
                var c2 = await children2.Complete;
                var p = await pets.Complete;

                Assert.True(f.SetEqualsSafe(family1.YieldReturn().Append(family2)));
                Assert.True(p.SetEqualsSafe(pet1.YieldReturn().Append(pet2).Append(pet3)));
                Assert.True(c1.SetEqualsSafe(child1.YieldReturn().Append(child2).Append(child3).Append(child4)));
                Assert.True(c2.SetEqualsSafe(child1.YieldReturn().Append(child2).Append(child3).Append(child4)));
            }
        }

        [SkippableTheory]
        [InlineData(11)]
        //  [InlineData(55)]
        //  [InlineData(100)]
        //  [InlineData(234)]
        public async Task PublishTest2Async(int count) {
            var families = new FamilyHandler(count);
            var pets = new PetHandler(count);
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix, builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var bus = harness.GetEventBus();
                Skip.If(bus == null);

                var senders = Enumerable.Range(0, count).Select(async i => {
                    await bus.PublishAsync(fix.Create<Family>());
                    await bus.PublishAsync(fix.Create<Pet>());
                });

                await Task.WhenAll(senders).With1MinuteTimeout();

                var f = await families.Complete;
                var p = await pets.Complete;

                Assert.Equal(count, f.Count);
                Assert.Equal(count, p.Count);
            }
        }

        [SkippableFact]
        public async Task BadArgumentsAndInvalidStateTests1Async() {
            var families = new FamilyHandler(1);
            var pets = new PetHandler(1);
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix, builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var host = harness.GetEventBusHost();
                Skip.If(host == null);

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
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            var harness = _fixture.GetHarness(prefix);
            try {
                var host = harness.GetEventBusHost();
                Skip.If(host == null);

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
            catch (SkipException) { }
            catch (Exception ex) {
                Assert.Null(ex);
            }
            finally {
                Try.Op(() => harness.Dispose()); // purposefully crashes due to double dispose
            }
        }

        public class FamilyHandler : IEventHandler<Family> {
            private readonly int _count;
            private readonly TaskCompletionSource<HashSet<Family>> _complete =
                new TaskCompletionSource<HashSet<Family>>();
            public Task<HashSet<Family>> Complete => _complete.Task.With1MinuteTimeout();

            public FamilyHandler(int count) {
                _count = count;
            }
            public Task HandleAsync(Family eventData) {
                Families.Add(eventData);
                if (Families.Count == _count) {
                    _complete.TrySetResult(Families);
                }
                return Task.CompletedTask;
            }
            public HashSet<Family> Families { get; } = new HashSet<Family>(
                Compare.Using<Family>((x, y) => x.Id == y.Id));
        }

        public class ChildHandler : IEventHandler<Child> {
            private readonly int _count;
            private readonly TaskCompletionSource<HashSet<Child>> _complete =
                new TaskCompletionSource<HashSet<Child>>();
            public Task<HashSet<Child>> Complete => _complete.Task.With1MinuteTimeout();
            public ChildHandler(int count) {
                _count = count;
            }
            public Task HandleAsync(Child eventData) {
                Children.Add(eventData);
                if (Children.Count == _count) {
                    _complete.TrySetResult(Children);
                }
                return Task.CompletedTask;
            }
            public HashSet<Child> Children { get; } = new HashSet<Child>(
                Compare.Using<Child>((x, y) => x.FirstName == y.FirstName));
        }

        public class PetHandler : IEventHandler<Pet> {
            private readonly int _count;
            private readonly TaskCompletionSource<HashSet<Pet>> _complete =
                new TaskCompletionSource<HashSet<Pet>>();
            public Task<HashSet<Pet>> Complete => _complete.Task.With1MinuteTimeout();
            public PetHandler(int count) {
                _count = count;
            }
            public Task HandleAsync(Pet eventData) {
                Pets.Add(eventData);
                if (Pets.Count == _count) {
                    _complete.TrySetResult(Pets);
                }
                return Task.CompletedTask;
            }
            public HashSet<Pet> Pets { get; } = new HashSet<Pet>(
                Compare.Using<Pet>((x, y) => x.GivenName == y.GivenName));
        }

    }
}
