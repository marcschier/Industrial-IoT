// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.DependencyInjection {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT;
    using Microsoft.Azure.IIoT.Azure.AspNetCore.KeyVault;
    using Microsoft.Azure.IIoT.Azure.AspNetCore.KeyVault.Runtime;
    using Microsoft.Azure.IIoT.Azure.KeyVault;
    using Microsoft.Azure.IIoT.Azure.KeyVault.Runtime;
    using Microsoft.Azure.IIoT.Azure.ActiveDirectory.Utils;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.KeyVault.WebKey;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Add data protection using azure blob storage and keyvault
    /// </summary>
    public static class DataProtectionBuilderEx {

        /// <summary>
        /// Add azure data protection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddAzureDataProtection(
            this IServiceCollection services, IConfiguration configuration = null) {
            if (configuration == null) {
                configuration = services.BuildServiceProvider()
                    .GetRequiredService<IConfiguration>();
            }
            services.AddDataProtection()
                .AddAzureBlobKeyStorage(configuration)
                .AddAzureKeyVaultDataProtection(configuration);
        }

        /// <summary>
        /// Add Keyvault protection
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static IDataProtectionBuilder AddAzureKeyVaultDataProtection(
            this IDataProtectionBuilder builder, IConfiguration configuration) {
            var config = new DataProtectionConfig(configuration);
            if (string.IsNullOrEmpty(config.KeyVault.Value.KeyVaultBaseUrl)) {
                throw new InvalidConfigurationException(
                    "Keyvault base url is missing in your configuration " +
                    "for dataprotection to be able to store the root key.");
            }
            var keyName = config.KeyVaultKeyDataProtection;
            using var keyVault = new KeyVaultClientBootstrap(configuration);
            if (!TryInititalizeKeyAsync(keyVault.Client,
                config.KeyVault.Value.KeyVaultBaseUrl, keyName).Result) {
                throw new UnauthorizedAccessException("Cannot access keyvault");
            }
            var identifier = $"{config.KeyVault.Value.KeyVaultBaseUrl.TrimEnd('/')}/keys/{keyName}";
            return builder.ProtectKeysWithAzureKeyVault(keyVault.Client, identifier);
        }

        /// <summary>
        /// Add blob key storage
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static IDataProtectionBuilder AddAzureBlobKeyStorage(
            this IDataProtectionBuilder builder, IConfiguration configuration) {

            var config = new DataProtectionConfig(configuration);
            var containerName = config.BlobStorageContainerDataProtection;
            var connectionString = config.Storage.Value.GetStorageConnString();
            if (string.IsNullOrEmpty(connectionString)) {
               throw new InvalidConfigurationException(
                   "Storage configuration is missing in your configuration for " +
                   "dataprotection to store all keys across all instances.");
            }
            var storageAccount = CloudStorageAccount.Parse(config.Storage.Value.GetStorageConnString());
            var relativePath = $"{containerName}/keys.xml";
            var uriBuilder = new UriBuilder(storageAccount.BlobEndpoint);
            uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + "/" + relativePath.TrimStart('/');
            var block = new CloudBlockBlob(uriBuilder.Uri, storageAccount.Credentials);
            Try.Op(() => block.Container.Create());
            return builder.PersistKeysToAzureBlobStorage(block);
        }

        /// <summary>
        /// Read configuration secret
        /// </summary>
        /// <param name="keyVaultClient"></param>
        /// <param name="vaultUri"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private static async Task<bool> TryInititalizeKeyAsync(
            KeyVaultClient keyVaultClient, string vaultUri, string keyName) {
            var logger = Log.Console();
            try {
                try {
                    var key = await keyVaultClient.GetKeyAsync(vaultUri, keyName).ConfigureAwait(false);
                }
                catch {
                    // Try create key
                    await keyVaultClient.CreateKeyAsync(vaultUri, keyName, new NewKeyParameters {
                        KeySize = 2048,
                        Kty = JsonWebKeyType.Rsa,
                        KeyOps = new List<string> {
                            JsonWebKeyOperation.Wrap, JsonWebKeyOperation.Unwrap
                        }
                    }).ConfigureAwait(false);
                }
                // Worked - we have a working keyvault client.
                return true;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Failed to authenticate to keyvault {url}.", vaultUri);
                return false;
            }
        }

        /// <summary>
        /// Data protection default configuration
        /// </summary>
        internal sealed class DataProtectionConfig : ConfigBase {

            private const string kKeyVaultKeyDataProtectionDefault = "dataprotection";
            private const string kBlobStorageContainerDataProtectionDefault = "dataprotection";

            /// <inheritdoc/>
            public IOptions<StorageOptions> Storage { get; }
            /// <inheritdoc/>
            public IOptions<KeyVaultOptions> KeyVault { get; }

            /// <summary>Key (in KeyVault) to be used for encription of keys</summary>
            public string KeyVaultKeyDataProtection =>
                GetStringOrDefault(PcsVariable.PCS_KEYVAULT_KEY_DATAPROTECTION,
                    () => Environment.GetEnvironmentVariable(
                        PcsVariable.PCS_KEYVAULT_KEY_DATAPROTECTION) ??
                        kKeyVaultKeyDataProtectionDefault).Trim();

            /// <summary>Blob Storage Container that holds encrypted keys</summary>
            public string BlobStorageContainerDataProtection =>
                GetStringOrDefault(PcsVariable.PCS_STORAGE_CONTAINER_DATAPROTECTION,
                    () => Environment.GetEnvironmentVariable(
                        PcsVariable.PCS_STORAGE_CONTAINER_DATAPROTECTION) ??
                        kBlobStorageContainerDataProtectionDefault).Trim();

            /// <summary>
            /// Configuration constructor
            /// </summary>
            /// <param name="configuration"></param>
            public DataProtectionConfig(IConfiguration configuration) :
                base(configuration) {
                Storage = new StorageConfig(configuration).ToOptions();
                KeyVault = new KeyVaultConfig(configuration).ToOptions();
            }
        }
    }
}
