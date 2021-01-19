﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Crypto.Services {
    using Microsoft.IIoT.Extensions.Crypto.Models;
    using Microsoft.IIoT.Extensions.Crypto.Storage;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.IIoT.Extensions.Storage.Services;
    using Microsoft.IIoT.Extensions.Utils;
    using Autofac.Extras.Moq;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class CrlFactoryTests {

        [Theory]
        [InlineData(SignatureType.PS512)]
        [InlineData(SignatureType.RS512)]
        [InlineData(SignatureType.PS256)]
        [InlineData(SignatureType.RS256)]
        [InlineData(SignatureType.PS384)]
        [InlineData(SignatureType.RS384)]
        public async Task RSASignedCrlCreateWith1TestAsync(SignatureType signature) {

            using (var mock = Setup()) {

                IKeyStore keys = mock.Create<KeyDatabase>();
                ICrlFactory factory = mock.Create<CrlFactory>();

                using (var root = SignatureType.PS512.Create("CN=root", true))
                using (var ca1 = root.Create(SignatureType.PS256, "CN=ca1", true))
                using (var ca2 = root.Create(SignatureType.PS256, "CN=ca2", true))
                using (var leaf1 = ca1.Create(SignatureType.RS256, "CN=leaf1"))
                using (var leaf2 = ca1.Create(SignatureType.RS256, "CN=leaf2"))
                using (var leaf3 = ca1.Create(SignatureType.RS256, "CN=leaf3")) {

                    var rootPrivateKey = root.ExportPrivateKey();
                    var rootPublicKey = rootPrivateKey.GetPublicKey();
                    var rootKeyHandle = await keys.ImportKeyAsync("ababa", rootPrivateKey,
                        new KeyStoreProperties { Exportable = true }).ConfigureAwait(false);

                    var next = DateTime.UtcNow + TimeSpan.FromDays(4);
                    next = next.Date;
                    var rootCert = root.ToCertificate(new IssuerPolicies(), rootKeyHandle);

                    var crl = await factory.CreateCrlAsync(rootCert, signature,
                        ca1.ToCertificate().YieldReturn(), next).ConfigureAwait(false);

                    var privateKey = await keys.ExportKeyAsync(rootKeyHandle).ConfigureAwait(false);

                    Assert.True(rootPrivateKey.SameAs(privateKey));
                    Assert.Equal(next, crl.NextUpdate);
                    Assert.Equal(root.Subject, crl.Issuer);
                    Assert.True(crl.IsRevoked(ca1.ToCertificate()));
                    Assert.False(crl.IsRevoked(ca2.ToCertificate()));
                    Assert.True(crl.HasValidSignature(rootCert));
                }
            }
        }

        [Theory]
        [InlineData(SignatureType.PS512)]
        [InlineData(SignatureType.RS512)]
        [InlineData(SignatureType.PS256)]
        [InlineData(SignatureType.RS256)]
        [InlineData(SignatureType.PS384)]
        [InlineData(SignatureType.RS384)]
        public async Task RSASignedCrlCreateWith2TestAsync(SignatureType signature) {

            using (var mock = Setup()) {

                IKeyStore keys = mock.Create<KeyDatabase>();
                ICrlFactory factory = mock.Create<CrlFactory>();

                using (var root = SignatureType.PS512.Create("CN=root", true))
                using (var ca1 = root.Create(SignatureType.PS256, "CN=ca1", true))
                using (var ca2 = root.Create(SignatureType.PS256, "CN=ca2", true))
                using (var leaf1 = ca1.Create(SignatureType.RS256, "CN=leaf1"))
                using (var leaf2 = ca1.Create(SignatureType.RS256, "CN=leaf2"))
                using (var leaf3 = ca1.Create(SignatureType.RS256, "CN=leaf3")) {

                    var rootPrivateKey = root.ExportPrivateKey();
                    var rootPublicKey = rootPrivateKey.GetPublicKey();
                    var rootKeyHandle = await keys.ImportKeyAsync("ababa", rootPrivateKey,
                        new KeyStoreProperties { Exportable = true }).ConfigureAwait(false);

                    var next = DateTime.UtcNow + TimeSpan.FromDays(4);
                    next = next.Date;
                    var rootCert = root.ToCertificate(new IssuerPolicies(), rootKeyHandle);

                    var crl = await factory.CreateCrlAsync(rootCert, signature,
                        new List<Certificate> {
                            ca2.ToCertificate(),
                            ca1.ToCertificate()
                        }, next).ConfigureAwait(false);

                    var privateKey = await keys.ExportKeyAsync(rootKeyHandle).ConfigureAwait(false);

                    Assert.True(rootPrivateKey.SameAs(privateKey));
                    Assert.Equal(next, crl.NextUpdate);
                    Assert.Equal(root.Subject, crl.Issuer);
                    Assert.True(crl.IsRevoked(ca1.ToCertificate()));
                    Assert.True(crl.IsRevoked(ca2.ToCertificate()));
                    Assert.True(crl.HasValidSignature(rootCert));
                }
            }
        }

        [Theory]
        [InlineData(SignatureType.ES256)]
        [InlineData(SignatureType.ES256K)]
        [InlineData(SignatureType.ES384)]
        [InlineData(SignatureType.ES512)]
        public async Task ECCSignedCrlCreateWith2TestAsync(SignatureType signature) {

            using (var mock = Setup()) {

                IKeyStore keys = mock.Create<KeyDatabase>();
                ICrlFactory factory = mock.Create<CrlFactory>();

                using (var root = SignatureType.ES512.Create("CN=root", true))
                using (var ca1 = root.Create(SignatureType.ES256, "CN=ca1", true))
                using (var ca2 = root.Create(SignatureType.ES256, "CN=ca2", true))
                using (var leaf1 = ca1.Create(SignatureType.ES256, "CN=leaf1"))
                using (var leaf2 = ca1.Create(SignatureType.ES256, "CN=leaf2"))
                using (var leaf3 = ca1.Create(SignatureType.ES256, "CN=leaf3")) {

                    var rootPrivateKey = root.ExportPrivateKey();
                    var rootPublicKey = rootPrivateKey.GetPublicKey();
                    var rootKeyHandle = await keys.ImportKeyAsync("ababa", rootPrivateKey,
                        new KeyStoreProperties { Exportable = true }).ConfigureAwait(false);

                    var next = DateTime.UtcNow + TimeSpan.FromDays(4);
                    next = next.Date;
                    var rootCert = root.ToCertificate(new IssuerPolicies(), rootKeyHandle);

                    var crl = await factory.CreateCrlAsync(rootCert, signature,
                        new List<Certificate> {
                            ca2.ToCertificate(),
                            ca1.ToCertificate()
                        }, next).ConfigureAwait(false);

                    var privateKey = await keys.ExportKeyAsync(rootKeyHandle).ConfigureAwait(false);

                    Assert.True(rootPrivateKey.SameAs(privateKey));
                    Assert.Equal(next, crl.NextUpdate);
                    Assert.Equal(root.Subject, crl.Issuer);
                    Assert.True(crl.IsRevoked(ca1.ToCertificate()));
                    Assert.True(crl.IsRevoked(ca2.ToCertificate()));
                    Assert.True(crl.HasValidSignature(rootCert));
                }
            }
        }

        /// <summary>
        /// Setup mock
        /// </summary>
        /// <param name="mock"></param>
        private static AutoMock Setup() {
            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterGeneric(typeof(OptionsMock<>)).AsImplementedInterfaces();
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().SingleInstance().As<IDatabaseServer>();
                builder.RegisterType<CollectionFactory>().As<ICollectionFactory>();
                builder.RegisterType<KeyDatabase>().As<IKeyStore>().As<IDigestSigner>();
                builder.RegisterType<KeyHandleSerializer>().As<IKeyHandleSerializer>();
                builder.RegisterType<CertificateIssuer>().As<ICertificateIssuer>();
                builder.RegisterType<CrlFactory>().As<ICrlFactory>();
            });
            return mock;
        }
    }
}
