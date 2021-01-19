// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Storage.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using AutoFixture;
    using AutoFixture.Kernel;
    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class EndpointDocumentTests {

        [Fact]
        public void TestEqualIsEqual() {
            var r1 = CreateRegistration();
            var r2 = r1;

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsNotEqual() {
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
            var m = r1.ToServiceModel("etag");
            var r2 = m.ToDocumentModel();

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsNotEqualWithServiceModelConversion() {
            var r1 = CreateRegistration();
            var m = r1.ToServiceModel("etag");
            m.Endpoint.SecurityPolicy = "";
            var r2 = m.ToDocumentModel();

            Assert.NotEqual(r1, r2);
            Assert.NotEqual(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 != r2);
            Assert.False(r1 == r2);
        }

        /// <summary>
        /// Helper to create registration
        /// </summary>
        /// <returns></returns>
        private static EndpointDocument CreateRegistration() {
            var fix = new Fixture();

            fix.Customizations.Add(new TypeRelay(typeof(VariantValue), typeof(VariantValue)));
            fix.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fix.Behaviors.Remove(b));
            fix.Behaviors.Add(new OmitOnRecursionBehavior());

            var cert = fix.CreateMany<byte>(1000).ToArray();
            var urls = fix.CreateMany<Uri>(4).ToList();
            var r1 = fix.Build<EndpointDocument>()
                .With(x => x.Thumbprint, cert.ToThumbprint())
                .With(x => x.AlternativeUrls,
                    fix.CreateMany<Uri>(4)
                        .Select(u => u.ToString())
                        .ToHashSet())
                .With(x => x.AuthenticationMethods,
                    fix.CreateMany<AuthenticationMethodModel>().ToList())
                .Without(x => x.NotSeenSince)
                .Without(x => x.ClassType)
                .Create();
            return r1;
        }
    }
}
