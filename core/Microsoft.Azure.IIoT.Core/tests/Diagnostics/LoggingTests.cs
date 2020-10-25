// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Autofac;
    using Xunit;
    using System.Text;
    using Xunit.Sdk;
    using Microsoft.Extensions.Logging;

    public class LoggingTests {

        [Fact]
        public void ResolveLoggerInServiceTest() {

            var builder = new ContainerBuilder();
            builder.RegisterModule<Log>();
            builder.RegisterType<Test>().AsSelf();

            using var container = builder.Build();
            var test = container.Resolve<Test>();

            Assert.NotNull(test);
        }

        [Fact]
        public void ResolveLoggerDirect1Test() {

            var builder = new ContainerBuilder();
            builder.RegisterModule<Log>();
            builder.RegisterType<Test>().AsSelf();

            using var container = builder.Build();
            var logger = container.Resolve<ILogger<Test>>();

            Assert.NotNull(logger);
            Assert.True(logger is ILogger<Test>);
        }

        [Fact]
        public void ResolveLoggerDirect2Test() {

            var builder = new ContainerBuilder();
            builder.RegisterModule<Log>();
            builder.RegisterType<Test>().AsSelf();

            using var container = builder.Build();
            var logger = container.Resolve<ILogger>();

            Assert.NotNull(logger);
            Assert.True(logger is ILogger<Log>);
        }


        public class Test {
            public Test(ILogger logger) {
                Assert.NotNull(logger);
                Assert.True(logger is ILogger<Test>);
            }
        }

    }
}
