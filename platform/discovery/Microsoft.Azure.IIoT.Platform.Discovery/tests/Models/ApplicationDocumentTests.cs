// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Models {
    using AutoFixture;
    using System.Linq;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class ApplicationDocumentTests {

        [Fact]
        public void TestEqualIsEqual() {
            var r1 = CreateDocument();
            var r2 = r1;

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsNotEqual() {
            var r1 = CreateDocument();
            var r2 = CreateDocument();

            Assert.NotEqual(r1, r2);
            Assert.False(r1.Equals(null));
            Assert.NotEqual(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 != r2);
            Assert.False(r1 == r2);
        }

        [Fact]
        public void TestEqualIsEqualWithServiceModelConversion() {
            var r1 = CreateDocument();
            var m = r1.ToServiceModel("etag");
            var r2 = m.ToDocumentModel();

            Assert.Equal(r1, r2);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 == r2);
            Assert.False(r1 != r2);
        }

        [Fact]
        public void TestEqualIsNotEqualWithServiceModelConversion() {
            var r1 = CreateDocument();
            var m = r1.ToServiceModel("etag");
            m.DiscoveryProfileUri = "";
            var r2 = m.ToDocumentModel();

            Assert.NotEqual(r1, r2);
            Assert.NotEqual(r1.GetHashCode(), r2.GetHashCode());
            Assert.True(r1 != r2);
            Assert.False(r1 == r2);
        }

        /// <summary>
        /// Create registration
        /// </summary>
        /// <returns></returns>
        private static ApplicationDocument CreateDocument() {
            var fix = new Fixture();
            var r1 = fix.Build<ApplicationDocument>()
                .With(x => x.Capabilities, fix.CreateMany<string>().ToHashSet())
                .With(x => x.DiscoveryUrls, fix.CreateMany<string>().ToHashSet())
                .With(x => x.HostAddresses, fix.CreateMany<string>().ToHashSet())
                .Without(x => x.NotSeenSince)
                .Without(x => x.ClassType)
                .Create();
            return r1;
        }
    }
}
