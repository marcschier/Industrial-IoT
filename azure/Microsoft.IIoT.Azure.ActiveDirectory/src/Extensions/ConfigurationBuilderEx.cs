// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Extensions.Configuration {
    using Microsoft.Extensions.Primitives;
    using Microsoft.IIoT.Azure.ActiveDirectory.Utils;
    using Microsoft.IIoT.Diagnostics;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using Autofac;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class ConfigurationBuilderEx {

        /// <summary>
        /// Add configuration from Azure KeyVault. Providers configured prior to
        /// this one will be used to get Azure KeyVault connection details.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="allowInteractiveLogon"></param>
        /// <param name="singleton"></param>
        /// <param name="keyVaultUrlVarName"></param>
        /// <param name="providerPriority"> Determines where in the configuration
        /// providers chain current provider should be added. </param>
        /// <returns></returns>
        public static IConfigurationBuilder AddFromKeyVault(this IConfigurationBuilder builder,
            ConfigSourcePriority providerPriority = ConfigSourcePriority.Normal,
            bool allowInteractiveLogon = false, bool singleton = true, string keyVaultUrlVarName = null) {
            if (builder is null) {
                throw new ArgumentNullException(nameof(builder));
            }

            var configuration = builder.Build();

            // Check if configuration should be loaded from KeyVault, default to true.
            var keyVaultConfigEnabled = configuration.GetValue(
                PcsVariable.PCS_KEYVAULT_CONFIG_ENABLED, true);
            if (!keyVaultConfigEnabled) {
                return builder;
            }

            var provider = KeyVaultConfigurationProvider.CreateInstanceAsync(
                allowInteractiveLogon, singleton, configuration, keyVaultUrlVarName).Result;
            if (provider != null) {
                switch (providerPriority) {
                    case ConfigSourcePriority.Low:
                        builder.Add(provider);
                        break;
                    case ConfigSourcePriority.Normal:
                        builder.Sources.Insert(0, provider);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unknown ConfigurationProviderPriority value: {providerPriority}");
                }
            }
            return builder;
        }

        /// <summary>
        /// Add keyvault authentication
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="allowInteractiveLogon"></param>
        /// <param name="singleton"></param>
        /// <param name="keyVaultUrlVarName"></param>
        /// <param name="providerPriority"> Determines where in the configuration
        /// providers chain current provider should be added. </param>
        /// <returns></returns>
        public static ContainerBuilder AddKeyVaultConfiguration(this ContainerBuilder builder,
            ConfigSourcePriority providerPriority = ConfigSourcePriority.Low,
            bool allowInteractiveLogon = false, bool singleton = true, string keyVaultUrlVarName = null) {

            builder.AddConfigurationSource(configuration => {
                // Check if configuration should be loaded from KeyVault, default to true.
                var keyVaultConfigEnabled = configuration.GetValue(
                    PcsVariable.PCS_KEYVAULT_CONFIG_ENABLED, true);
                if (!keyVaultConfigEnabled) {
                    return null;
                }
                var provider = KeyVaultConfigurationProvider.CreateInstanceAsync(
                    allowInteractiveLogon, singleton, configuration, keyVaultUrlVarName).Result;
                if (provider == null) {
                    return null;
                }
                return provider;
            }, providerPriority);
            return builder;
        }

        /// <summary>
        /// Keyvault configuration provider.
        /// </summary>
        internal sealed class KeyVaultConfigurationProvider : IConfigurationSource,
            IConfigurationProvider, IDisposable {

            /// <summary>
            /// Create keyvault provider
            /// </summary>
            /// <param name="configuration"></param>
            /// <param name="keyVaultUri"></param>
            /// <param name="allowInteractiveLogon"></param>
            private KeyVaultConfigurationProvider(IConfigurationRoot configuration,
                string keyVaultUri, bool allowInteractiveLogon) {
                _keyVault = new KeyVaultClientBootstrap(configuration, allowInteractiveLogon);
                _keyVaultUri = keyVaultUri;
                _cache = new ConcurrentDictionary<string, Task<SecretBundle>>();
                _reloadToken = new ConfigurationReloadToken();
            }

            /// <inheritdoc/>
            public IConfigurationProvider Build(IConfigurationBuilder builder) {
                return this;
            }

            /// <inheritdoc/>
            public bool TryGet(string key, out string value) {
                value = null;
                if (!key.StartsWith("PCS_", StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
                try {
                    if (_allSecretsLoaded && !_cache.ContainsKey(key)) {
                        // Prevents non existant keys to be looked up
                        return false;
                    }
                    var resultTask = _cache.GetOrAdd(key, k => {
                        return _keyVault.Client.GetSecretAsync(_keyVaultUri,
                            GetSecretNameForKey(k));
                    });
                    if (resultTask.IsFaulted || resultTask.IsCanceled) {
                        return false;
                    }
                    value = resultTask.Result.Value;
                    return true;
                }
                catch {
                    return false;
                }
            }

            /// <inheritdoc/>
            public void Set(string key, string value) {
                // No op
            }

            /// <inheritdoc/>
            public void Load() {
                // No op
            }

            /// <inheritdoc/>
            public IChangeToken GetReloadToken() {
                return _reloadToken;
            }

            /// <inheritdoc/>
            public void Dispose() {
                _keyVault.Dispose();
            }

            /// <inheritdoc/>
            public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys,
                string parentPath) {
                // Not supported
                return Enumerable.Empty<string>();
            }

            /// <summary>
            /// Create configuration provider
            /// </summary>
            /// <param name="allowInteractiveLogon"></param>
            /// <param name="singleton"></param>
            /// <param name="configuration"></param>
            /// <param name="keyVaultUrlVarName"></param>
            /// <returns></returns>
            public static async Task<KeyVaultConfigurationProvider> CreateInstanceAsync(
                bool allowInteractiveLogon, bool singleton, IConfigurationRoot configuration,
                string keyVaultUrlVarName) {
                if (string.IsNullOrEmpty(keyVaultUrlVarName)) {
                    keyVaultUrlVarName = PcsVariable.PCS_KEYVAULT_URL;
                }
                if (singleton && !allowInteractiveLogon) {
                    // Save singleton creation
                    if (_singleton == null) {
                        lock (kLock) {
                            if (_singleton == null) {
                                // Create instance
                                _singleton = CreateInstanceAsync(configuration, false,
                                    keyVaultUrlVarName, false);
                            }
                        }
                    }
                    return await _singleton.ConfigureAwait(false);
                }
                // Create new instance
                return await CreateInstanceAsync(configuration, allowInteractiveLogon,
                    keyVaultUrlVarName, true).ConfigureAwait(false);
            }

            /// <summary>
            /// Create new instance
            /// </summary>
            /// <param name="configuration"></param>
            /// <param name="allowInteractiveLogon"></param>
            /// <param name="keyVaultUrlVarName"></param>
            /// <param name="lazyLoad"></param>
            /// <returns></returns>
            private static async Task<KeyVaultConfigurationProvider> CreateInstanceAsync(
                IConfigurationRoot configuration, bool allowInteractiveLogon, string keyVaultUrlVarName,
                bool lazyLoad) {
                var vaultUri = configuration.GetValue<string>(keyVaultUrlVarName, null);
                var logger = Log.Console();
                if (string.IsNullOrEmpty(vaultUri)) {
                    logger.LogWarning("No keyvault uri found in configuration under {key}. ",
                        keyVaultUrlVarName);
                    vaultUri = Environment.GetEnvironmentVariable(keyVaultUrlVarName);
                    if (string.IsNullOrEmpty(vaultUri)) {
                        logger.LogWarning("No keyvault uri found in environment under {key}. " +
                            "Not reading configuration from keyvault without keyvault uri.",
                            keyVaultUrlVarName);
                        return null;
                    }
                }
                var provider = new KeyVaultConfigurationProvider(configuration, vaultUri,
                    allowInteractiveLogon);
                try {
                    await provider.ValidateReadSecretAsync(keyVaultUrlVarName).ConfigureAwait(false);
                }
                catch (Exception ex) {
// https://github.com/Azure/Industrial-IoT/tree/master/deploy/helm/azure-industrial-iot#load-configuration-from-azure-key-vault
                    throw new InvalidConfigurationException(
                        "Could not access the provided keyvault URI. " +
                        "If you want to read configuration from the keyvault, make sure " +
                        "it is reachable, the required permissions are configured " +
                        "and authentication provider information is available. " +
                        "Sign into Visual Studio or Azure CLI on this machine and try again.", ex);
                }
                if (!lazyLoad) {
                    while (true) {
                        try {
                            await provider.LoadAllSecretsAsync().ConfigureAwait(false);
                            break;
                        }
                        // try again...
                        catch (TaskCanceledException) {}
                        catch (SocketException) {}
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                        logger.LogDebug("Failed loading secrets due to timeout or network - try again ...");
                    }
                }
                return provider;
            }

            /// <summary>
            /// Read configuration secret
            /// </summary>
            /// <param name="secretName"></param>
            /// <returns></returns>
            private async Task ValidateReadSecretAsync(string secretName) {
                for (var retries = 0; ; retries++) {
                    try {
                        var secret = await _keyVault.Client.GetSecretAsync(_keyVaultUri,
                            GetSecretNameForKey(secretName)).ConfigureAwait(false);
                        // Worked - we have a working keyvault client.
                        return;
                    }
                    catch (TaskCanceledException) { }
                    catch (SocketException) { }
                    if (retries > 3) {
                        throw new TimeoutException(
                            $"Failed to access keyvault due to timeout or network {_keyVaultUri}.");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Preload cache
            /// </summary>
            /// <returns></returns>
            private async Task LoadAllSecretsAsync() {
                // Read all secrets
                var secretPage = await _keyVault.Client.GetSecretsAsync(_keyVaultUri)
                    .ConfigureAwait(false);
                var allSecrets = new List<SecretItem>(secretPage.ToList());
                while (true) {
                    if (secretPage.NextPageLink == null) {
                        break;
                    }
                    secretPage =  await _keyVault.Client.GetSecretsNextAsync(
                        secretPage.NextPageLink).ConfigureAwait(false);
                    allSecrets.AddRange(secretPage.ToList());
                }
                foreach (var secret in allSecrets) {
                    if (secret.Attributes?.Enabled != true) {
                        continue;
                    }
                    var key = GetKeyForSecretName(secret.Identifier.Name);
                    if (key == null) {
                        continue;
                    }
                    _cache.TryAdd(key, _keyVault.Client.GetSecretAsync(
                        secret.Identifier.Identifier));
                }
                _allSecretsLoaded = true;
                await Task.WhenAll(_cache.Values).ConfigureAwait(false);
            }

            /// <summary>
            /// Get secret key for key value. Replace any upper case
            /// letters with lower case and _ with -.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            private static string GetSecretNameForKey(string key) {
                return key.Replace("_", "-", StringComparison.Ordinal).ToLowerInvariant();
            }

            /// <summary>
            /// Get secret key for key value. Replace any upper case
            /// letters with lower case and _ with -.
            /// </summary>
            /// <param name="secretId"></param>
            /// <returns></returns>
            private static string GetKeyForSecretName(string secretId) {
                if (!secretId.StartsWith("pcs-", StringComparison.Ordinal)) {
                    return null;
                }
                return secretId.Replace("-", "_", StringComparison.Ordinal).ToUpperInvariant();
            }

            private static readonly object kLock = new object();
            private static Task<KeyVaultConfigurationProvider> _singleton;

            private readonly string _keyVaultUri;
            private readonly KeyVaultClientBootstrap _keyVault;
            private readonly ConcurrentDictionary<string, Task<SecretBundle>> _cache;
            private readonly ConfigurationReloadToken _reloadToken;
            private bool _allSecretsLoaded;
        }
    }
}
