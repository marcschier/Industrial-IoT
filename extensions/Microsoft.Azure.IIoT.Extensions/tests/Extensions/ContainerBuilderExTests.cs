// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class ContainerBuilderExTests {

        [Fact]
        public void TestGetOptions() {
            var builder = new ContainerBuilder();
            builder.AddOptions();
            using var scope = builder.Build();
            var options = scope.Resolve<IOptions<TestOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Value);
            Assert.Null(options.Value.Test1);
            Assert.Equal(0, options.Value.Test2);
        }

        [Fact]
        public void TestGetOptionsConfigureWithCallbacks1() {
            var builder = new ContainerBuilder();
            builder.AddOptions();
            builder.Configure<TestOptions>(options => options.Test2 = 1);
            builder.Configure<TestOptions>(options => options.Test1 = "test");
            using var scope = builder.Build();
            var options = scope.Resolve<IOptions<TestOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Value);
            Assert.Equal("test", options.Value.Test1);
            Assert.Equal(1, options.Value.Test2);
        }

        [Fact]
        public void TestGetOptionsConfigureWithCallbacks2() {
            var builder = new ContainerBuilder();
            builder.AddOptions();
            builder.Configure<TestOptions>(options => options.Test2 = 1);
            builder.Configure<TestOptions>(options => options.Test1 = "test");
            builder.Configure<TestOptions>(options => options.Test2 = 0);
            builder.Configure<TestOptions>(options => options.Test1 = null);
            using var scope = builder.Build();
            var options = scope.Resolve<IOptions<TestOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Value);
            Assert.Null(options.Value.Test1);
            Assert.Equal(0, options.Value.Test2);
        }

        [Fact]
        public void TestGetOptionsConfigureWithClass() {
            var builder = new ContainerBuilder();
            builder.AddOptions();
            builder.RegisterType<TestConfigure>().AsImplementedInterfaces();
            using var scope = builder.Build();
            var options = scope.Resolve<IOptions<TestOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Value);
            Assert.Equal("test1000", options.Value.Test1);
            Assert.Equal(1000, options.Value.Test2);
        }
    }

    public class TestOptions {
        public string Test1 { get; set; }
        public int Test2 { get; set; }
    }

    public class TestConfigure : ConfigBase<TestOptions> {
        public TestConfigure(IConfiguration configuration = null) : base(configuration) {
        }

        public override void Configure(string name, TestOptions options) {
            options.Test1 = "test1000";
            options.Test2 = 1000;
        }
    }
}
