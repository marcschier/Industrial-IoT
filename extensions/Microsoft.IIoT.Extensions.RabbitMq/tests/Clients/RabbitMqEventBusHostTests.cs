// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq.Clients {
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Utils;
    using AutoFixture;
    using Autofac;
    using Xunit;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit.Categories;

    [SystemTest]
    [Collection(RabbitMqCollection.Name)]
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
                var bus = harness.GetEventBusPublisher();
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

                await bus.PublishAsync(family1).ConfigureAwait(false);
                await bus.PublishAsync(child1).ConfigureAwait(false);
                await bus.PublishAsync(pet2).ConfigureAwait(false);
                await bus.PublishAsync(child2).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(child3).ConfigureAwait(false);
                await bus.PublishAsync(pet1).ConfigureAwait(false);
                await bus.PublishAsync(child4).ConfigureAwait(false);
                await bus.PublishAsync(child1).ConfigureAwait(false);
                await bus.PublishAsync(family1).ConfigureAwait(false);
                await bus.PublishAsync(family2).ConfigureAwait(false);
                await bus.PublishAsync(pet3).ConfigureAwait(false);
                await bus.PublishAsync(pet3).ConfigureAwait(false);

                var f = await families.Complete.ConfigureAwait(false);
                var c1 = await children1.Complete.ConfigureAwait(false);
                var c2 = await children2.Complete.ConfigureAwait(false);
                var p = await pets.Complete.ConfigureAwait(false);

                Assert.True(f.SetEqualsSafe(family1.YieldReturn().Append(family2)));
                Assert.True(p.SetEqualsSafe(pet1.YieldReturn().Append(pet2).Append(pet3)));
                Assert.True(c1.SetEqualsSafe(child1.YieldReturn().Append(child2).Append(child3).Append(child4)));
                Assert.True(c2.SetEqualsSafe(child1.YieldReturn().Append(child2).Append(child3).Append(child4)));
            }
        }

        [SkippableTheory]
        [InlineData(11)]
        [InlineData(55)]
        [InlineData(100)]
        [InlineData(234)]
        public async Task PublishTest2Async(int count) {
            var families = new FamilyHandler(count);
            var pets = new PetHandler(count);
            var fix = new Fixture();
            var prefix = fix.Create<string>();
            using (var harness = _fixture.GetHarness(prefix, builder => {
                builder.RegisterInstance(families).AsImplementedInterfaces();
                builder.RegisterInstance(pets).AsImplementedInterfaces();
            })) {
                var bus = harness.GetEventBusPublisher();
                Skip.If(bus == null);

                var senders = Enumerable.Range(0, count).Select(async i => {
                    await bus.PublishAsync(fix.Create<Family>()).ConfigureAwait(false);
                    await bus.PublishAsync(fix.Create<Pet>()).ConfigureAwait(false);
                });

                await Task.WhenAll(senders).With2MinuteTimeout().ConfigureAwait(false);

                var f = await families.Complete.ConfigureAwait(false);
                var p = await pets.Complete.ConfigureAwait(false);

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

                await host.StopAsync().ConfigureAwait(false);
                await host.StartAsync().ConfigureAwait(false);

                // Already running - should throw
                await Assert.ThrowsAsync<ResourceInvalidStateException>(host.StartAsync).ConfigureAwait(false);

                await host.StopAsync().ConfigureAwait(false);
                await host.StopAsync().ConfigureAwait(false);
                await host.StartAsync().ConfigureAwait(false);

                // Should throw
                await Assert.ThrowsAsync<ResourceInvalidStateException>(host.StartAsync).ConfigureAwait(false);
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

                await host.StopAsync().ConfigureAwait(false);
                await host.StartAsync().ConfigureAwait(false);
                await host.StopAsync().ConfigureAwait(false);
                await host.StartAsync().ConfigureAwait(false);

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

        internal class FamilyHandler : IEventBusConsumer<Family> {
            private readonly int _count;
            private readonly TaskCompletionSource<HashSet<Family>> _complete =
                new TaskCompletionSource<HashSet<Family>>(TaskCreationOptions.RunContinuationsAsynchronously);
            public Task<HashSet<Family>> Complete => _complete.Task.ContinueAfter2Minutes(() => Families);

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

        internal class ChildHandler : IEventBusConsumer<Child> {
            private readonly int _count;
            private readonly TaskCompletionSource<HashSet<Child>> _complete =
                new TaskCompletionSource<HashSet<Child>>(TaskCreationOptions.RunContinuationsAsynchronously);
            public Task<HashSet<Child>> Complete => _complete.Task.ContinueAfter2Minutes(() => Children);
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

        internal class PetHandler : IEventBusConsumer<Pet> {
            private readonly int _count;
            private readonly TaskCompletionSource<HashSet<Pet>> _complete =
                new TaskCompletionSource<HashSet<Pet>>(TaskCreationOptions.RunContinuationsAsynchronously);
            public Task<HashSet<Pet>> Complete => _complete.Task.ContinueAfter2Minutes(() => Pets);
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
