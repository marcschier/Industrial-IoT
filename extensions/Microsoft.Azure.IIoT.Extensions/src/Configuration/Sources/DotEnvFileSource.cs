// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Configuration {
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Memory;
    using System.IO;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Env file configuration
    /// </summary>
    public class DotEnvFileSource : IConfigurationSource {

        /// <summary>
        /// Adds .env file environment variables from an .env file that 
        /// is in current folder or below up to root.
        /// </summary>
        public DotEnvFileSource() {
            _source = new MemoryConfigurationSource();
            try {
                // Find .env file
                var curDir = Path.GetFullPath(Environment.CurrentDirectory);
                while (!string.IsNullOrEmpty(curDir) &&
                    !File.Exists(Path.Combine(curDir, ".env"))) {
                    curDir = Path.GetDirectoryName(curDir);
                }
                if (!string.IsNullOrEmpty(curDir)) {
                    TryAddToSource(_source, Path.Combine(curDir, ".env"));
                }
            }
            catch (IOException) {
            }
        }

        /// <summary>
        /// Create configuration source from file
        /// </summary>
        /// <param name="filePath"></param>
        public DotEnvFileSource(string filePath) {
            _source = new MemoryConfigurationSource();
            try {
                TryAddToSource(_source, filePath);
            }
            catch (IOException) {
            }
        }

        /// <summary>
        /// Adds .env file environment variables
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static void TryAddToSource(MemoryConfigurationSource source, string filePath) {
            if (!string.IsNullOrEmpty(filePath)) {
                try {
                    var lines = File.ReadAllLines(filePath);
                    var values = new Dictionary<string, string>();
                    foreach (var line in lines) {
                        var offset = line.IndexOf('=');
                        if (offset == -1) {
                            continue;
                        }
                        var key = line.Substring(0, offset).Trim();
                        if (key.StartsWith("#", StringComparison.Ordinal)) {
                            continue;
                        }
                        key = key.Replace("__", ConfigurationPath.KeyDelimiter);
                        values.AddOrUpdate(key, line[(offset + 1)..]);
                    }
                    source.InitialData = values;
                }
                catch (IOException) { }
            }
        }

        /// <inheritdoc/>
        public IConfigurationProvider Build(IConfigurationBuilder builder) {
            return _source.Build(builder);
        }

        private readonly MemoryConfigurationSource _source;
    }
}
