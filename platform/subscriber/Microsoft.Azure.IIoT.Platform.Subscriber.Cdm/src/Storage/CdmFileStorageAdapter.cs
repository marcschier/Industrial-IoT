﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Cdm.Storage {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.CommonDataModel.ObjectModel.Storage;
    using Newtonsoft.Json.Linq;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;
    using System.Collections.Concurrent;

    /// <summary>
    /// Writes data tables into files on file storage
    /// </summary>
    public class CdmFileStorageAdapter : NetworkAdapter, StorageAdapter,
        IDisposable {

        /// <inheritdoc/>
        public string LocationHint { get; set; }

        /// <inheritdoc/>
        public StorageAdapter Adapter => this;

        /// <summary>
        /// CDM Azure Data lake storage handler
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="storage"></param>
        /// <param name="config"></param>
        public CdmFileStorageAdapter(IFileStorage storage, ICdmFolderConfig config,
            ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (storage == null) {
                throw new ArgumentNullException(nameof(storage));
            }
            if (string.IsNullOrEmpty(config?.StorageDrive)) {
                throw new ArgumentException("Missing storage drive", nameof(config));
            }
            if (string.IsNullOrEmpty(config?.StorageFolder)) {
                throw new ArgumentException("Missing storage folder", nameof(config));
            }
            _hostName = storage.Endpoint;
            _fileSystem = config.StorageDrive;
            _folder = OpenRootFolderAsync(storage, config.StorageDrive,
                config.StorageFolder);
        }

        /// <inheritdoc/>
        public bool CanRead() {
            return true;
        }

        /// <inheritdoc/>
        public Task LockAsync(string corpusPath) {
            return _locks.GetOrAdd(FormatCorpusPath(corpusPath),
                p => GetLockedCorpusFileAsync(p));
        }

        /// <inheritdoc/>
        public async Task<string> ReadAsync(string corpusPath) {
            try {
                var file = await GetCorpusFileAsync(corpusPath).ConfigureAwait(false);
                using (var stream = new MemoryStream()) {
                    await file.DownloadAsync(stream).ConfigureAwait(false);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to read data from {corpus}", corpusPath);
                throw;
            }
        }

        /// <inheritdoc/>
        public bool CanWrite() {
            return true;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(string corpusPath, string data) {
            try {
                var file = await GetCorpusFileAsync(corpusPath).ConfigureAwait(false);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                    await file.UploadAsync(stream).ConfigureAwait(false);
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to write data to {corpus}", corpusPath);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UnlockAsync(string corpusPath) {
            if (_locks.TryRemove(FormatCorpusPath(corpusPath), out var locked)) {
                var file = await locked.ConfigureAwait(false);
                await file.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(string corpusPath, Func<bool, byte[]> encoder) {
            try {
                var file = await GetCorpusFileAsync(corpusPath).ConfigureAwait(false);
                var size = await file.GetSizeAsync().ConfigureAwait(false);
                var content = encoder(size == 0);
                await file.AppendAsync(content, content.Length).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to write data to {corpus}", corpusPath);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<string>> FetchAllFilesAsync(string folderCorpusPath) {
            try {
                var folder = await GetCorpusFolderAsync(folderCorpusPath).ConfigureAwait(false);
                var result = await folder.GetAllFilesAsync().ConfigureAwait(false);
                return result.ToList();
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to get files in {corpus}", folderCorpusPath);
                throw;
            }
        }

        /// <inheritdoc />
        public string CreateAdapterPath(string corpusPath) {
            if (string.IsNullOrEmpty(corpusPath)) {
                return null;
            }
            var formattedCorpusPath = FormatCorpusPath(corpusPath);
            return $"https://{_hostName}/{_fileSystem}/{_folder.Result.Name}/{formattedCorpusPath}";
        }

        /// <inheritdoc />
        public string CreateCorpusPath(string adapterPath) {
            if (string.IsNullOrEmpty(adapterPath)) {
                return null;
            }
            return adapterPath.Replace(
                $"https://{_hostName}/{_fileSystem}/{_folder.Result.Name}/", "");
        }

        /// <inheritdoc />
        public async Task<DateTimeOffset?> ComputeLastModifiedTimeAsync(string corpusPath) {
            try {
                var file = await GetCorpusFileAsync(corpusPath).ConfigureAwait(false);
                return await file.GetLastModifiedAsync().ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to get modified time from file.");
                return null;
            }
        }

        /// <inheritdoc/>
        public void UpdateConfig(string config) {
            // Do not honor this
            UpdateNetworkConfig(config);
        }

        /// <inheritdoc/>
        public string FetchConfig() {
            var configObject = new JObject
            {
                { "hostname", _hostName },
                { "root", _fileSystem }
            };
            // Try constructing network configs.
            configObject.Add(FetchNetworkConfig());
            if (LocationHint != null) {
                configObject.Add("locationHint", LocationHint);
            }
            var resultConfig = new JObject
            {
                { "type", Type },
                { "config", configObject }
            };
            return resultConfig.ToString();
        }

        /// <inheritdoc />
        public void ClearCache() {
            return;
        }

        /// <summary>
        /// Format corpus path.
        /// </summary>
        /// <param name="corpusPath">The corpusPath.</param>
        /// <returns></returns>
        private static string FormatCorpusPath(string corpusPath) {
            if (corpusPath.StartsWith("adls:")) {
                corpusPath = corpusPath[5..];
            }
            return corpusPath.TrimStart('/');
        }

        /// <summary>
        /// Open folder
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="fileSystem"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static async Task<IFolder> OpenRootFolderAsync(IFileStorage storage,
            string fileSystem, string folder) {
            var fs = await storage.CreateOrOpenDriveAsync(fileSystem).ConfigureAwait(false);
            return await fs.CreateOrOpenSubFolderAsync(folder).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a corpus file from the locked ones or a shared one
        /// if not locked.
        /// </summary>
        /// <param name="corpusPath"></param>
        /// <returns></returns>
        private async Task<IFile> GetCorpusFileAsync(string corpusPath) {
            if (_locks.TryGetValue(FormatCorpusPath(corpusPath), out var lockedFile)) {
                var locked = await lockedFile.ConfigureAwait(false);
                return locked.File;
            }
            return await GetSharedCorpusFileAsync(corpusPath).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the corpus fiile
        /// </summary>
        /// <param name="corpusPath"></param>
        /// <returns></returns>
        private async Task<IFile> GetSharedCorpusFileAsync(string corpusPath) {
            var pathElements = FormatCorpusPath(corpusPath).Split('/');
            if (pathElements.Length == 0) {
                throw new ArgumentException("Bad path", nameof(corpusPath));
            }
            var root = await _folder.ConfigureAwait(false);
            for (var i = 0; i < pathElements.Length - 1; i++) {
                root = await root.CreateOrOpenSubFolderAsync(pathElements[i]).ConfigureAwait(false);
            }
            return await root.CreateOrOpenFileAsync(pathElements.Last()).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a locked corpus file
        /// </summary>
        /// <param name="corpusPath"></param>
        /// <returns></returns>
        private async Task<IFileLock> GetLockedCorpusFileAsync(string corpusPath) {
            var pathElements = FormatCorpusPath(corpusPath).Split('/');
            if (pathElements.Length == 0) {
                throw new ArgumentException("Bad path", nameof(corpusPath));
            }
            var root = await _folder.ConfigureAwait(false);
            for (var i = 0; i < pathElements.Length - 1; i++) {
                root = await root.CreateOrOpenSubFolderAsync(pathElements[i]).ConfigureAwait(false);
            }
            return await root.CreateOrOpenFileLockAsync(pathElements.Last(),
                TimeSpan.FromSeconds(60)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the corpus folder
        /// </summary>
        /// <param name="corpusPath"></param>
        /// <returns></returns>
        private async Task<IFolder> GetCorpusFolderAsync(string corpusPath) {
            var pathElements = FormatCorpusPath(corpusPath).Split('/');
            if (pathElements.Length == 0) {
                throw new ArgumentException("Bad path", nameof(corpusPath));
            }
            var root = await _folder.ConfigureAwait(false);
            for (var i = 0; i < pathElements.Length - 1; i++) {
                root = await root.CreateOrOpenSubFolderAsync(pathElements[i]).ConfigureAwait(false);
            }
            return root;
        }

        internal const string Type = "adls";
        private readonly ILogger _logger;
        private readonly Uri _hostName;
        private readonly string _fileSystem;
        private readonly Task<IFolder> _folder;
        private readonly ConcurrentDictionary<string, Task<IFileLock>> _locks =
            new ConcurrentDictionary<string, Task<IFileLock>>();
    }
}
