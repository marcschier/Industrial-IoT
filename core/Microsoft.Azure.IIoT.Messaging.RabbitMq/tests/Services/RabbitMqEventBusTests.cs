// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.RabbitMq {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public class RabbitMqEventBusTests : IClassFixture<RabbitMqFixture> {
        private readonly RabbitMqFixture _fixture;

        public RabbitMqEventBusTests(RabbitMqFixture fixture) {
            _fixture = fixture;
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
            await Assert.ThrowsAsync<ArgumentException>(
                () => bus.UnregisterAsync(string.Empty));
            await Assert.ThrowsAsync<ArgumentException>(
                () => bus.UnregisterAsync("bad"));
        }
    }
}
