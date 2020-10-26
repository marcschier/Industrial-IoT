// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using AutoFixture;
    using System.Linq;
    using Xunit;

    public class DiscovererRegistrationTests {

        [Fact]
        public void TestEqualIsEqual() {
            var fix = new Fixture();
            _ = fix.CreateMany<byte>(1000).ToArray();
            var r1 = CreateRegistration();
            var r2 = r1;

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsNotEqual() {
            var fix = new Fixture();
            _ = fix.CreateMany<byte>(1000).ToArray();
            var r1 = CreateRegistration();
            var r2 = CreateRegistration();

            Assert.NotEqual(r1, r2);
            Assert.False(r1.Equals(null));
            Assert.NotEqual(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 != r2);
            Assert.False(r1 == r2);
        }

        [Fact]
        public void TestEqualIsEqualWithServiceModelConversion() {

            var r1 = CreateRegistration();
            var m = r1.ToServiceModel();
            var r2 = m.ToDiscovererRegistration();

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsNotEqualWithServiceModelConversionWhenDisabled() {
            _ = new Fixture();

            var r1 = CreateRegistration();
            var m = r1.ToServiceModel();
            var r2 = m.ToDiscovererRegistration(true);

            Assert.NotEqual(r1, r2);
            Assert.NotEqual(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 != r2);
            Assert.False(r1 == r2);
        }

        [Fact]
        public void TestEqualIsEqualWithDeviceModel() {
            var r1 = CreateRegistration();
            var m = r1.ToDeviceTwin(_serializer);
            var r2 = m.ToEntityRegistration();

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsEqualWithDeviceModelWhenDisabled() {
            _ = new Fixture();

            var r1 = CreateRegistration();
            var r2 = r1.ToServiceModel().ToDiscovererRegistration(true);
            var m1 = r1.Patch(r2, _serializer);
            var r3 = r2.ToServiceModel().ToDiscovererRegistration(false);
            var m2 = r2.Patch(r3, _serializer);

            Assert.True((bool)m1.Tags[nameof(EntityRegistration.IsDisabled)]);
            Assert.Null((bool?)m2.Tags[nameof(EntityRegistration.IsDisabled)]);
        }

        /// <summary>
        /// Create registration
        /// </summary>
        /// <returns></returns>
        private static DiscovererRegistration CreateRegistration() {
            var fix = new Fixture();
            var cert = fix.CreateMany<byte>(1000).ToArray();
            var r = fix.Build<DiscovererRegistration>()
                .FromFactory(() => new DiscovererRegistration(
                    fix.Create<string>(), fix.Create<string>()))
                .Without(x => x.IsDisabled)
                .Create();
            r._desired = r;
            return r;
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
