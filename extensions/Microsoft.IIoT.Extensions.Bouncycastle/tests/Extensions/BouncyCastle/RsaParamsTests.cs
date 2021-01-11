﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.BouncyCastle {
    using Microsoft.IIoT.Extensions.Crypto.Models;
    using System.Security.Cryptography;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public static class RsaParamsTests {

        [Fact]
        public static void ConvertToAndFromTest() {
            using (var ecdsa = RSA.Create()) {
                var key = ecdsa.ToKey();

                var rsaparams1 = key.Parameters as RsaParams;
                var rsaparamsb = rsaparams1.ToRsaKeyParameters();
                var rsaparams2 = rsaparamsb.ToRsaParams();

                Assert.True(rsaparams1.SameAs(rsaparams2));
            }
        }
    }
}
