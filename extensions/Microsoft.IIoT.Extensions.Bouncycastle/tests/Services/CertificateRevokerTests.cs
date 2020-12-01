// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Crypto.Services {
    using Microsoft.IIoT.Crypto.Models;
    using Microsoft.IIoT.Crypto.Storage;
    using Microsoft.IIoT.Storage;
    using Microsoft.IIoT.Storage.Services;
    using Microsoft.IIoT.Serializers.NewtonSoft;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Utils;
    using Autofac;
    using Autofac.Extras.Moq;
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class CertificateRevokerTests {

        [Fact]
        public async Task RevokeRSAIssuerAndRSAIssuersTestAsync() {

            using (var mock = Setup()) {
                ICertificateIssuer service = mock.Create<CertificateIssuer>();
                var rootca = await service.NewRootCertificateAsync("rootca",
                    X500DistinguishedNameEx.Create("CN=rootca"), DateTime.UtcNow, TimeSpan.FromDays(5),
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(3) }).ConfigureAwait(false);
                var intca = await service.NewIssuerCertificateAsync("rootca", "intca",
                    X500DistinguishedNameEx.Create("CN=intca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(2) }).ConfigureAwait(false);
                var footca1 = await service.NewIssuerCertificateAsync("intca", "footca1",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);
                var footca2 = await service.NewIssuerCertificateAsync("intca", "footca2",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);

                // Run
                ICertificateRevoker revoker = mock.Create<CertificateRevoker>();
                await revoker.RevokeCertificateAsync(intca.SerialNumber).ConfigureAwait(false);

                ICertificateStore store = mock.Create<CertificateDatabase>();
                var foundi = await store.FindLatestCertificateAsync("intca").ConfigureAwait(false);
                var found1 = await store.FindLatestCertificateAsync("footca1").ConfigureAwait(false);
                var found2 = await store.FindLatestCertificateAsync("footca2").ConfigureAwait(false);

                ICrlEndpoint crls = mock.Create<CrlDatabase>();
                var chainr = await crls.GetCrlChainAsync(rootca.SerialNumber).ConfigureAwait(false);

                // Assert
                Assert.NotNull(foundi);
                Assert.NotNull(found1);
                Assert.NotNull(found2);
                Assert.NotNull(foundi.Revoked);
                Assert.NotNull(found1.Revoked);
                Assert.NotNull(found2.Revoked);
                Assert.NotNull(chainr);
                Assert.Single(chainr);
                Assert.True(chainr.Single().HasValidSignature(rootca));
            }
        }

        [Fact]
        public async Task RevokeECCIssuerAndECCIssuersTestAsync() {

            using (var mock = Setup()) {
                ICertificateIssuer service = mock.Create<CertificateIssuer>();
                var rootca = await service.NewRootCertificateAsync("rootca",
                    X500DistinguishedNameEx.Create("CN=rootca"), DateTime.UtcNow, TimeSpan.FromDays(5),
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(3) }).ConfigureAwait(false);
                var intca = await service.NewIssuerCertificateAsync("rootca", "intca",
                    X500DistinguishedNameEx.Create("CN=intca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(2) }).ConfigureAwait(false);
                var footca1 = await service.NewIssuerCertificateAsync("intca", "footca1",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);
                var footca2 = await service.NewIssuerCertificateAsync("intca", "footca2",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);

                // Run
                ICertificateRevoker revoker = mock.Create<CertificateRevoker>();
                await revoker.RevokeCertificateAsync(intca.SerialNumber).ConfigureAwait(false);

                ICertificateStore store = mock.Create<CertificateDatabase>();
                var foundi = await store.FindLatestCertificateAsync("intca").ConfigureAwait(false);
                var found1 = await store.FindLatestCertificateAsync("footca1").ConfigureAwait(false);
                var found2 = await store.FindLatestCertificateAsync("footca2").ConfigureAwait(false);

                ICrlEndpoint crls = mock.Create<CrlDatabase>();
                // Get crl for root
                var chainr = await crls.GetCrlChainAsync(rootca.SerialNumber).ConfigureAwait(false);

                // Assert
                Assert.NotNull(foundi);
                Assert.NotNull(found1);
                Assert.NotNull(found2);
                Assert.NotNull(foundi.Revoked);
                Assert.NotNull(found1.Revoked);
                Assert.NotNull(found2.Revoked);
                Assert.NotNull(chainr);
                Assert.Single(chainr);
                Assert.True(chainr.Single().HasValidSignature(rootca));
                Assert.True(chainr.Single().IsRevoked(intca));
            }
        }

        [Fact]
        public async Task RevokeRSAIssuersTestAsync() {

            using (var mock = Setup()) {
                // Setup
                ICertificateIssuer service = mock.Create<CertificateIssuer>();
                var rootca = await service.NewRootCertificateAsync("rootca",
                    X500DistinguishedNameEx.Create("CN=rootca"), DateTime.UtcNow, TimeSpan.FromDays(5),
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(3) }).ConfigureAwait(false);
                var intca = await service.NewIssuerCertificateAsync("rootca", "intca",
                    X500DistinguishedNameEx.Create("CN=intca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(2) }).ConfigureAwait(false);
                var footca1 = await service.NewIssuerCertificateAsync("intca", "footca1",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);
                var footca2 = await service.NewIssuerCertificateAsync("intca", "footca2",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.RSA },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);

                // Run
                ICertificateRevoker revoker = mock.Create<CertificateRevoker>();
                await revoker.RevokeCertificateAsync(footca1.SerialNumber).ConfigureAwait(false);
                await revoker.RevokeCertificateAsync(footca2.SerialNumber).ConfigureAwait(false);

                ICertificateStore store = mock.Create<CertificateDatabase>();
                var foundi = await store.FindLatestCertificateAsync("intca").ConfigureAwait(false);
                var found1 = await store.FindLatestCertificateAsync("footca1").ConfigureAwait(false);
                var found2 = await store.FindLatestCertificateAsync("footca2").ConfigureAwait(false);

                ICrlEndpoint crls = mock.Create<CrlDatabase>();
                // Get crl chain for intca and rootca
                var chainr = await crls.GetCrlChainAsync(intca.SerialNumber).ConfigureAwait(false);

                // Assert
                Assert.NotNull(foundi);
                Assert.NotNull(found1);
                Assert.NotNull(found2);
                Assert.Null(foundi.Revoked);
                Assert.NotNull(found1.Revoked);
                Assert.NotNull(found2.Revoked);
                Assert.NotNull(chainr);
                Assert.NotEmpty(chainr);
                Assert.Equal(2, chainr.Count());
                Assert.True(chainr.ToArray()[1].HasValidSignature(intca));
                Assert.True(chainr.ToArray()[0].HasValidSignature(rootca));
                Assert.True(chainr.Last().IsRevoked(footca1));
                Assert.True(chainr.Last().IsRevoked(footca2));
                Assert.False(chainr.First().IsRevoked(intca));
            }
        }

        [Fact]
        public async Task RevokeECCIssuersTestAsync() {

            using (var mock = Setup()) {
                ICertificateIssuer service = mock.Create<CertificateIssuer>();
                var rootca = await service.NewRootCertificateAsync("rootca",
                    X500DistinguishedNameEx.Create("CN=rootca"), DateTime.UtcNow, TimeSpan.FromDays(5),
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(3) }).ConfigureAwait(false);
                var intca = await service.NewIssuerCertificateAsync("rootca", "intca",
                    X500DistinguishedNameEx.Create("CN=intca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(2) }).ConfigureAwait(false);
                var footca1 = await service.NewIssuerCertificateAsync("intca", "footca1",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);
                var footca2 = await service.NewIssuerCertificateAsync("intca", "footca2",
                    X500DistinguishedNameEx.Create("CN=footca"), DateTime.UtcNow,
                    new CreateKeyParams { KeySize = 2048, Type = KeyType.ECC, Curve = CurveType.P384 },
                    new IssuerPolicies { IssuedLifetime = TimeSpan.FromHours(1) }).ConfigureAwait(false);

                // Run
                ICertificateRevoker revoker = mock.Create<CertificateRevoker>();
                await revoker.RevokeCertificateAsync(footca1.SerialNumber).ConfigureAwait(false);
                await revoker.RevokeCertificateAsync(footca2.SerialNumber).ConfigureAwait(false);

                ICertificateStore store = mock.Create<CertificateDatabase>();
                var foundi = await store.FindLatestCertificateAsync("intca").ConfigureAwait(false);
                var found1 = await store.FindLatestCertificateAsync("footca1").ConfigureAwait(false);
                var found2 = await store.FindLatestCertificateAsync("footca2").ConfigureAwait(false);

                ICrlEndpoint crls = mock.Create<CrlDatabase>();
                var chainr = await crls.GetCrlChainAsync(intca.SerialNumber).ConfigureAwait(false);

                // Assert
                Assert.NotNull(foundi);
                Assert.NotNull(found1);
                Assert.NotNull(found2);
                Assert.Null(foundi.Revoked);
                Assert.NotNull(found1.Revoked);
                Assert.NotNull(found2.Revoked);
                Assert.NotNull(chainr);
                Assert.NotEmpty(chainr);
                Assert.Equal(2, chainr.Count());
                Assert.True(chainr.ToArray()[1].HasValidSignature(intca));
                Assert.True(chainr.ToArray()[0].HasValidSignature(rootca));
            }
        }

        /// <summary>
        /// Setup mock
        /// </summary>
        /// <param name="mock"></param>
        /// <param name="provider"></param>
        private static AutoMock Setup() {
            var mock = AutoMock.GetLoose(builder => {
                builder.RegisterGeneric(typeof(OptionsMock<>)).AsImplementedInterfaces();
                builder.RegisterType<NewtonSoftJsonConverters>().As<IJsonSerializerConverterProvider>();
                builder.RegisterType<NewtonSoftJsonSerializer>().As<IJsonSerializer>();
                builder.RegisterType<MemoryDatabase>().SingleInstance().As<IDatabaseServer>();
                builder.RegisterType<CollectionFactory>().As<ICollectionFactory>();
                builder.RegisterType<KeyDatabase>().As<IKeyStore>().As<IDigestSigner>();
                builder.RegisterType<KeyHandleSerializer>().As<IKeyHandleSerializer>();
                builder.RegisterType<CertificateDatabase>().As<ICertificateStore>();
                builder.RegisterType<CertificateDatabase>().As<ICertificateRepository>();
                builder.RegisterType<CertificateFactory>().As<ICertificateFactory>();
                builder.RegisterType<CrlDatabase>().As<ICrlRepository>();
                builder.RegisterType<CertificateIssuer>().As<ICertificateIssuer>();
                builder.RegisterType<CrlFactory>().As<ICrlFactory>();
            });
            return mock;
        }
    }
}

