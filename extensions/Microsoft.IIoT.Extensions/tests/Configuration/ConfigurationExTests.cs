// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Autofac {
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Memory;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class ConfigurationExTests {

        [Fact]
        public void TestGetFromRootConfiguration() {
            var builder = new ContainerBuilder();
            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    ["TEST"] = "test"
                })
                .Build();
            builder.AddConfiguration(c);

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal("test", value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithOverride1() {
            var builder = new ContainerBuilder();
            Environment.SetEnvironmentVariable("TEST", "other");
            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    ["TEST"] = "test"
                })
                .Build();
            builder.AddConfiguration(c);
            builder.AddEnvironmentVariableConfiguration(); // Will be found here

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal("other", value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithOverride2() {
            var builder = new ContainerBuilder();
            Environment.SetEnvironmentVariable("TEST", "other");
            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    ["TEST"] = "test"
                })
                .Build();
            builder.AddConfiguration(c); // Will be found here
            builder.AddEnvironmentVariableConfiguration(ConfigSourcePriority.Low);

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal("test", value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithOverride3() {
            var builder = new ContainerBuilder();
            Environment.SetEnvironmentVariable("TEST", "other");

            builder.AddEnvironmentVariableConfiguration();
            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    ["TEST"] = "test"
                })
                .Build();
            builder.AddConfiguration(c); // Will be found here

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal("test", value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithCustomSource0() {
            var builder = new ContainerBuilder();
            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();
            builder.AddConfigurationSource<CustomSource<double>>();
            builder.AddConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    ["TEST"] = "test"
                })
                .Build()); // Found here

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal("test", value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithCustomSource1() {
            var builder = new ContainerBuilder();
            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();
            builder.AddConfigurationSource<CustomSource<double>>();

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(double).Name, value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithCustomSource2() {
            var builder = new ContainerBuilder();
            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>(); // This is the last
            builder.AddConfigurationSource<CustomSource<double>>(ConfigSourcePriority.Low);

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(string).Name, value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithCustomSource3() {
            var builder = new ContainerBuilder();
            builder.AddConfigurationSource<CustomSource<double>>(ConfigSourcePriority.Low);
            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>(); // This is the last

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(string).Name, value);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithPreBuiltConfiguration1() {
            var builder = new ContainerBuilder();
            var capturedValue = "None";

            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();
            builder.AddConfigurationSource<CustomSource<double>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue = configuration.GetValue<string>("TEST");
                return new CustomSource<bool>();
            }); // last item should now resolve

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(bool).Name, value);
            Assert.Equal(typeof(double).Name, capturedValue);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithPreBuiltConfiguration2() {
            var builder = new ContainerBuilder();
            var capturedValue = "None";

            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();
            builder.AddConfigurationSource<CustomSource<double>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue = configuration.GetValue<string>("TEST");
                return new CustomSource<bool>();
            }, ConfigSourcePriority.Low); // Will be overridden

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(double).Name, value);
            Assert.Equal(typeof(double).Name, capturedValue);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithPreBuiltConfiguration2b() {
            var builder = new ContainerBuilder();
            var capturedValue1 = "None";
            var capturedValue2 = "None";

            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();
            builder.AddConfigurationSource<CustomSource<double>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue1 = configuration.GetValue<string>("TEST");
                return new CustomSource<bool>();
            });

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue2 = configuration.GetValue<string>("TEST");
                return new CustomSource<short>();
            }, ConfigSourcePriority.Low);

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(bool).Name, value);
            Assert.Equal(typeof(double).Name, capturedValue1);
            Assert.Equal(typeof(bool).Name, capturedValue2);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithPreBuiltConfiguration2c() {
            var builder = new ContainerBuilder();
            var capturedValue1 = "None";
            var capturedValue2 = "None";

            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue1 = configuration.GetValue<string>("TEST");
                return new CustomSource<bool>();
            }, ConfigSourcePriority.Low);

            builder.AddConfigurationSource<CustomSource<double>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue2 = configuration.GetValue<string>("TEST");
                return new CustomSource<short>();
            }, ConfigSourcePriority.Low);

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(double).Name, value);
            Assert.Equal(typeof(double).Name, capturedValue1);
            Assert.Equal(typeof(double).Name, capturedValue2);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithPreBuiltConfiguration2d() {
            var builder = new ContainerBuilder();
            var capturedValue1 = "None";
            var capturedValue2 = "None";

            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue1 = configuration.GetValue<string>("TEST");
                return new CustomSource<bool>();
            }, ConfigSourcePriority.Low);

            builder.AddConfigurationSource<CustomSource<double>>();

            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue2 = configuration.GetValue<string>("TEST");
                return new CustomSource<short>();
            });

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(short).Name, value);
            Assert.Equal(typeof(double).Name, capturedValue1);
            Assert.Equal(typeof(double).Name, capturedValue2);
        }

        [Fact]
        public void TestGetFromRootConfigurationWithPreBuiltConfiguration3() {
            var builder = new ContainerBuilder();
            var capturedValue = "None";

            builder.AddConfigurationSource<CustomSource<int>>();
            builder.AddConfigurationSource<CustomSource<string>>();
            builder.AddConfigurationSource<CustomSource<double>>();
            builder.AddConfigurationSource(configuration => {
                // Capture value and then return bool source
                capturedValue = configuration.GetValue<string>("TEST");
                return null;
            });

            using var scope = builder.Build();
            var confguration = scope.Resolve<IConfiguration>();
            var root = scope.Resolve<IConfigurationRoot>();
            Assert.NotNull(root);
            Assert.NotNull(confguration);

            var value = confguration.GetValue<string>("TEST");
            Assert.Equal(typeof(double).Name, value);
            Assert.Equal(typeof(double).Name, capturedValue);
        }

        public class CustomSource<T> : MemoryConfigurationSource {
            public CustomSource() {
                InitialData = new Dictionary<string, string> {
                    ["TEST"] = typeof(T).Name
                };
            }
            public CustomSource(string overrider) {
                InitialData = new Dictionary<string, string> {
                    ["TEST"] = overrider
                };
            }
        }
    }
}
