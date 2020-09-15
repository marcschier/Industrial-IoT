// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hub {
    using AutoFixture;
    using System;
    using Xunit;

    /// <summary>
    /// Simple bitmap on top of ulong list
    /// </summary>
    public class HubResourceTests {

        [Fact]
        public void TestFormatParse1() {

            var fix = new Fixture();
            var hub = fix.Create<string>();
            var device = fix.Create<string>();
            var module = fix.Create<string>();

            var target = HubResource.Format(hub, device, module);
            var d = HubResource.Parse(target, out var h, out var m);

            Assert.Equal(hub, h);
            Assert.Equal(device, d);
            Assert.Equal(module, m);
        }

        [Fact]
        public void TestFormatParse2a() {

            var fix = new Fixture();
            var hub = fix.Create<string>();
            var device = fix.Create<string>();

            var target = HubResource.Format(hub, device, null);
            var d = HubResource.Parse(target, out var h, out var m);

            Assert.Equal(hub, h);
            Assert.Equal(device, d);
            Assert.Null(m);
        }

        [Fact]
        public void TestFormatParse2b() {

            var fix = new Fixture();
            var hub = fix.Create<string>();
            var device = fix.Create<string>();

            var target = HubResource.Format(hub, device, "");
            var d = HubResource.Parse(target, out var h, out var m);

            Assert.Equal(hub, h);
            Assert.Equal(device, d);
            Assert.Null(m);
        }

        [Fact]
        public void TestFormatParse3() {

            var fix = new Fixture();
            var device = fix.Create<string>();
            var module = fix.Create<string>();

            var target = HubResource.Format(null, device, module);
            var d = HubResource.Parse(target, out var h, out var m);

            Assert.Null(h);
            Assert.Equal(device, d);
            Assert.Equal(module, m);
        }

        [Fact]
        public void TestFormatParse4a() {

            var fix = new Fixture();
            var device = fix.Create<string>();

            var target = HubResource.Format(null, device, null);
            var d = HubResource.Parse(target, out var h, out var m);

            Assert.Null(h);
            Assert.Equal(device, d);
            Assert.Null(m);
        }

        [Fact]
        public void TestFormatParse4b() {

            var fix = new Fixture();
            var device = fix.Create<string>();

            var target = HubResource.Format("", device, null);
            var d = HubResource.Parse(target, out var h, out var m);

            Assert.Null(h);
            Assert.Equal(device, d);
            Assert.Null(m);
        }

        [Fact]
        public void TestFormatParseWithPrefix1() {

            var fix = new Fixture();
            var prefix = "test/test/";
            var hub = fix.Create<string>();
            var device = fix.Create<string>();
            var module = fix.Create<string>();

            var target = HubResource.Format(hub, device, module);
            var d = HubResource.Parse(prefix + target, out var h, out var m);

            Assert.Equal(hub, h);
            Assert.Equal(device, d);
            Assert.Equal(module, m);
        }

        [Fact]
        public void TestFormatParseWithPrefix2a() {

            var fix = new Fixture();
            var prefix = "test/////";
            var hub = fix.Create<string>();
            var device = fix.Create<string>();

            var target = HubResource.Format(hub, device, null);
            var d = HubResource.Parse(prefix + target, out var h, out var m);

            Assert.Equal(hub, h);
            Assert.Equal(device, d);
            Assert.Null(m);
        }

        [Fact]
        public void TestFormatParseWithPrefix2b() {

            var fix = new Fixture();
            var prefix = "/dd/test/";
            var hub = fix.Create<string>();
            var device = fix.Create<string>();

            var target = HubResource.Format(hub, device, "");
            var d = HubResource.Parse(prefix + target, out var h, out var m);

            Assert.Equal(hub, h);
            Assert.Equal(device, d);
            Assert.Null(m);
        }

        [Fact]
        public void TestBadArgumentThrown() {

            Assert.Throws<ArgumentNullException>(() => HubResource.Format("h", null, "m"));
            Assert.Throws<ArgumentNullException>(() => HubResource.Format("h", "", "m"));
            Assert.Throws<ArgumentNullException>(() => HubResource.Format(null, null, "m"));
            Assert.Throws<ArgumentNullException>(() => HubResource.Format("h", "", null));
            Assert.Throws<ArgumentNullException>(() => HubResource.Parse(null, out var h, out var m));
            Assert.Throws<ArgumentNullException>(() => HubResource.Parse("", out var h, out var m));

            Assert.Throws<FormatException>(() => HubResource.Parse("hub/x", out var h, out var m));
            Assert.Throws<FormatException>(() => HubResource.Parse("hub/devices", out var h, out var m));
            Assert.Throws<FormatException>(() => HubResource.Parse("hub/devices/did/fid", out var h, out var m));
            Assert.Throws<FormatException>(() => HubResource.Parse("hub/devices/did/modules/mid/bif", out var h, out var m));
        }
    }
}
