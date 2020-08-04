// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.Azure.IIoT.Cryptography;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Security.Cryptography.X509Certificates;
    using System.Runtime.Serialization;

    /// <summary>
    /// Edgelet client providing discovery and in the future other services
    /// </summary>
    public sealed class EdgeletClient : ISecureElement {

        /// <inheritdoc/>
        public bool IsPresent => WorkloadUri != null;

        /// <summary>
        /// Workload uri
        /// </summary>
        internal string WorkloadUri =>
            Environment.GetEnvironmentVariable("IOTEDGE_WORKLOADURI")?.TrimEnd('/');

        /// <summary>
        /// Module generation
        /// </summary>
        internal string ModuleGenerationId =>
            Environment.GetEnvironmentVariable("IOTEDGE_MODULEGENERATIONID");

        /// <summary>
        /// Module identifier
        /// </summary>
        internal string ModuleId =>
            Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");

        /// <summary>
        /// Api to use
        /// </summary>
        internal string ApiVersion =>
            Environment.GetEnvironmentVariable("IOTEDGE_APIVERSION") ?? "2019-01-30";

        /// <summary>
        /// Uri of the security daemon.
        /// </summary>
        internal string ModuleWorkloadUri =>
            $"{WorkloadUri}/modules/{ModuleId}/genid/{ModuleGenerationId}";

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public EdgeletClient(IHttpClient client, IJsonSerializer serializer, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (IsPresent) {
                _logger.Information("Hsm present at {uri}", WorkloadUri);
            }
        }

        /// <inheritdoc/>
        public async Task<X509Certificate2Collection> CreateServerCertificateAsync(
            string commonName, DateTime expiration, CancellationToken ct) {
            EnsureHsmIsPresent();
            var request = _client.NewRequest(
                $"{ModuleWorkloadUri}/certificate/server?api-version={ApiVersion}", Resource.Local);
            _serializer.SerializeToRequest(request, new { commonName, expiration });
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                var response = await _client.PostAsync(request, ct);
                response.Validate();
                var result = _serializer.DeserializeResponse<EdgeletCertificateResponse>(response);
                // TODO add private key
                return new X509Certificate2Collection(
                    X509Certificate2Ex.ParsePemCerts(result.Certificate).ToArray());
            }, kMaxRetryCount);
        }

        /// <inheritdoc/>
        public async Task<byte[]> EncryptAsync(string initializationVector,
            byte[] plaintext, CancellationToken ct) {
            EnsureHsmIsPresent();
            var request = _client.NewRequest(
                $"{ModuleWorkloadUri}/encrypt?api-version={ApiVersion}", Resource.Local);
            _serializer.SerializeToRequest(request, new { initializationVector, plaintext });
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                var response = await _client.PostAsync(request, ct);
                response.Validate();
                return _serializer.DeserializeResponse<EncryptResponse>(response).CipherText;
            }, kMaxRetryCount);
        }

        /// <inheritdoc/>
        public async Task<byte[]> DecryptAsync(string initializationVector,
            byte[] ciphertext, CancellationToken ct) {
            EnsureHsmIsPresent();
            var request = _client.NewRequest(
                $"{ModuleWorkloadUri}/decrypt?api-version={ApiVersion}", Resource.Local);
            _serializer.SerializeToRequest(request, new { initializationVector, ciphertext });
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                var response = await _client.PostAsync(request, ct);
                response.Validate();
                return _serializer.DeserializeResponse<DecryptResponse>(response).Plaintext;
            }, kMaxRetryCount);
        }

        /// <inheritdoc/>
        public async Task<byte[]> SignAsync(byte[] data, string keyId, string algo,
            CancellationToken ct) {
            EnsureHsmIsPresent();
            if (algo == null) {
                algo = "HMACSHA256";
            }
            if (keyId == null) {
                keyId = "primary";
            }
            var request = _client.NewRequest(
                $"{ModuleWorkloadUri}/sign?api-version={ApiVersion}", Resource.Local);
            _serializer.SerializeToRequest(request, new { keyId, algo, data });
            return await Retry.WithExponentialBackoff(_logger, ct, async () => {
                var response = await _client.PostAsync(request, ct);
                response.Validate();
                return _serializer.DeserializeResponse<SignResponse>(response).Digest;
            }, kMaxRetryCount);
        }

        /// <summary>
        /// Test presence of hsm
        /// </summary>
        private void EnsureHsmIsPresent() {
            if (!IsPresent) {
                throw new InvalidOperationException("Hsm not present.");
            }
        }

        /// <summary>
        /// Sign response
        /// </summary>
        [DataContract]
        public class SignResponse {

            /// <summary>Signature of the data.</summary>
            [DataMember(Name = "digest")]
            public byte[] Digest { get; set; }
        }

        /// <summary>
        /// Encrypt response
        /// </summary>
        [DataContract]
        public class EncryptResponse {

            /// <summary>Cypher.</summary>
            [DataMember(Name = "cipherText")]
            public byte[] CipherText { get; set; }
        }

        /// <summary>
        /// Decrypt response
        /// </summary>
        [DataContract]
        public class DecryptResponse {

            /// <summary>Cypher.</summary>
            [DataMember(Name = "plaintext")]
            public byte[] Plaintext { get; set; }
        }

        /// <summary>
        /// Edgelet create certificate response
        /// </summary>
        [DataContract]
        public class EdgeletCertificateResponse {

            /// <summary>
            /// Base64 encoded PEM formatted byte array
            /// containing the certificate and its chain.
            /// </summary>
            [DataMember(Name = "certificate")]
            public string Certificate { get; set; }

            /// <summary>Private key.</summary>
            [DataMember(Name = "privateKey")]
            public EdgeletPrivateKey PrivateKey { get; set; }
        }

        /// <summary>
        /// Edgelet private key
        /// </summary>
        [DataContract]
        public class EdgeletPrivateKey {

            /// <summary>Type of private key.</summary>
            [DataMember(Name = "type")]
            public string Type { get; set; }

            /// <summary>Base64 encoded PEM formatted byte array</summary>
            [DataMember(Name = "bytes")]
            public string Bytes { get; set; }
        }

        private readonly IHttpClient _client;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private const int kMaxRetryCount = 3;
    }
}
