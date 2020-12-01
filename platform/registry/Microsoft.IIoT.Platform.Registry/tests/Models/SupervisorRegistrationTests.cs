// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Models {
    using Microsoft.IIoT.Serializers.NewtonSoft;
    using Microsoft.IIoT.Serializers;
    using AutoFixture;
    using System.Linq;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class SupervisorRegistrationTests {

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
            var r2 = m.ToSupervisorRegistration();

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
            var r2 = m.ToSupervisorRegistration(true);

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
            var r2 = r1.ToServiceModel().ToSupervisorRegistration(true);
            var m1 = r1.Patch(r2, _serializer);
            var r3 = r2.ToServiceModel().ToSupervisorRegistration(false);
            var m2 = r2.Patch(r3, _serializer);

            Assert.True((bool)m1.Tags[nameof(EntityRegistration.IsDisabled)]);
            Assert.Null((bool?)m2.Tags[nameof(EntityRegistration.IsDisabled)]);
        }

        /// <summary>
        /// Create registration
        /// </summary>
        /// <returns></returns>
        private static SupervisorRegistration CreateRegistration() {
            var fix = new Fixture();
            var cert = fix.CreateMany<byte>(1000).ToArray();
            var r = fix.Build<SupervisorRegistration>()
                .FromFactory(() => new SupervisorRegistration(
                    fix.Create<string>(), fix.Create<string>()))
                .Without(x => x.IsDisabled)
                .Create();
            return r;
        }

        private readonly IJsonSerializer _serializer = new NewtonSoftJsonSerializer();
    }
}
