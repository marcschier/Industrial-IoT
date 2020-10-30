﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Net.Models {
    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class PortRangeTests {

        [Fact]
        public void TestSimpleRange() {
            var range1 = new PortRange(0, 100);
            var range2 = new PortRange(100, 0);

            Assert.Equal(101, range1.Count);
            Assert.Equal(range1, range2);
            Assert.False(range1 != range2);
            Assert.True(range1 == range2);
            Assert.True(range1.Overlaps(range2));
            Assert.True(range1.Contains(0));
            Assert.True(range1.Contains(100));
            Assert.True(range1.Contains(6));
        }

        [Fact]
        public void TestNoneEquality1() {
            var range1 = new PortRange(0, 4);
            var range2 = new PortRange(1, 4);

            Assert.NotEqual(range1, range2);
            Assert.True(range1 != range2);
            Assert.False(range1 == range2);
            Assert.True(range1.Overlaps(range2));
        }

        [Fact]
        public void TestNoneEquality2() {
            var range1 = new PortRange(0, 4);
            var range2 = new PortRange(7, 9);

            Assert.NotEqual(range1, range2);
            Assert.True(range1 != range2);
            Assert.False(range1 == range2);
            Assert.False(range1.Overlaps(range2));
        }

        [Fact]
        public void TestParsing1() {
            _ = PortRange.TryParse("0-100", out var range1);
            var range2 = new PortRange(0, 100);

            _ = Assert.Single(range1);
            Assert.Equal(range2, range1.Single());
            Assert.Equal("0-100", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing2() {
            _ = PortRange.TryParse("0-100;144", out var range1);
            var range2 = new PortRange(0, 100);
            var range3 = new PortRange(144);

            Assert.Equal(range2, range1.First());
            Assert.Equal(range3, range1.Skip(1).First());
            Assert.Equal(2, range1.Count());
            Assert.Equal("0-100;144", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing2b() {
            _ = PortRange.TryParse("0-100;44", out var range1);
            var range2 = new PortRange(0, 100);

            Assert.Equal(range2, range1.First());
            _ = Assert.Single(range1);
            Assert.Equal("0-100", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing3() {
            _ = PortRange.TryParse("0,3,6,,,,", out var range1);

            Assert.Equal(new PortRange(0), range1.First());
            Assert.Equal(new PortRange(3), range1.Skip(1).First());
            Assert.Equal(new PortRange(6), range1.Skip(2).First());
            Assert.Equal(3, range1.Count());
            Assert.Equal("0;3;6", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing3b() {
            _ = PortRange.TryParse("0-1,3-4,6-9,,,,", out var range1);

            Assert.Equal(new PortRange(0, 1), range1.First());
            Assert.Equal(new PortRange(3, 4), range1.Skip(1).First());
            Assert.Equal(new PortRange(6, 9), range1.Skip(2).First());
            Assert.Equal(3, range1.Count());
            Assert.Equal("0-1;3-4;6-9", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing3c() {
            _ = PortRange.TryParse("0-1;3-4;6-*", out var range1);

            Assert.Equal(new PortRange(0, 1), range1.First());
            Assert.Equal(new PortRange(3, 4), range1.Skip(1).First());
            Assert.Equal(new PortRange(6, 65536), range1.Skip(2).First());
            Assert.Equal(3, range1.Count());
            Assert.Equal("0-1;3-4;6-*", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing3d() {
            _ = PortRange.TryParse("0-1,1-2,2-3,,,,", out var range1);

            _ = Assert.Single(range1);
            Assert.Equal(new PortRange(0, 3), range1.First());
            Assert.Equal("0-3", PortRange.Format(range1));
        }

        [Fact]
        public void TestParsing4() {
            _ = PortRange.TryParse(",,,,", out var range1);
            Assert.Empty(range1);
        }

        [Fact]
        public void TestParsing5() {
            _ = PortRange.TryParse("*", out var range1);
            _ = Assert.Single(range1);
            Assert.Equal(new PortRange(0, 65536), range1.Single());
            Assert.Equal("*", range1.Single().ToString());
        }

        [Fact]
        public void TestParsing6() {
            _ = PortRange.TryParse("100-*", out var range1);
            _ = Assert.Single(range1);
            Assert.Equal(new PortRange(100, 65536), range1.Single());
            Assert.Equal("100-*", range1.Single().ToString());
        }

        [Fact]
        public void TestParsing7() {
            _ = PortRange.TryParse("*-100", out var range1);
            _ = Assert.Single(range1);
            Assert.Equal(new PortRange(0, 100), range1.Single());
            Assert.Equal("0-100", range1.Single().ToString());
        }

        [Fact]
        public void TestParseFormatExceptions() {

            _ = Assert.Throws<FormatException>(() => PortRange.Parse("0.0.0.0/24;x/2;;"));
            _ = Assert.Throws<FormatException>(() => PortRange.Parse("abf,d"));
            _ = Assert.Throws<FormatException>(() => PortRange.Parse("0-1-2"));
            _ = Assert.Throws<FormatException>(() => PortRange.Parse("0,1-2,f,"));
        }
    }
}
