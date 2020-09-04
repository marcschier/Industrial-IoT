﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.MassTransit {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;
    using System.Threading;

    public class MassTransitEventBusTests : IClassFixture<MassTransitFixture> {
        private readonly MassTransitFixture _fixture;

        public MassTransitEventBusTests(MassTransitFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task PublishTestAsync() {
            var bus = _fixture.GetEventBus();
            Skip.If(bus == null);

            var family = new Fixture().Create<Family>();

            var tcs = new TaskCompletionSource<Family>();
            var token = await bus.RegisterAsync<Family>(f => {
                tcs.SetResult(f);
                return Task.CompletedTask;
            });

            await bus.PublishAsync(family);

            var f = await tcs.Task;
            Assert.Equal(family.Id, f.Id);
            Assert.Equal(family.LastName, f.LastName);
            Assert.Equal(family.RegistrationDate, f.RegistrationDate);

            await bus.UnregisterAsync(token);
        }

        [SkippableFact]
        public async Task BadArgumentTestsAsync() {
            var bus = _fixture.GetEventBus();
            Skip.If(bus == null);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => bus.PublishAsync<Family>(null));
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => bus.RegisterAsync<Family>(null));
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => bus.UnregisterAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => bus.UnregisterAsync(string.Empty));
            await Assert.ThrowsAsync<ResourceInvalidStateException>(
                () => bus.UnregisterAsync("bad"));
        }
    }
}
