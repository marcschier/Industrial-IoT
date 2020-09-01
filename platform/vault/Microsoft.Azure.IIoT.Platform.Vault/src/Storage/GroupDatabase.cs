// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Storage {
    using Microsoft.Azure.IIoT.Platform.Vault.Storage.Models;
    using Microsoft.Azure.IIoT.Platform.Vault.Models;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Trust group database
    /// </summary>
    public sealed class GroupDatabase : IGroupRepository {

        /// <summary>
        /// Create group database
        /// </summary>
        /// <param name="db"></param>
        public GroupDatabase(IItemContainerFactory db) {
            if (db == null) {
                throw new ArgumentNullException(nameof(db));
            }
            _groups = db.OpenAsync("groups").Result;
        }

        /// <inheritdoc/>
        public async Task<TrustGroupRegistrationModel> AddAsync(
            TrustGroupRegistrationModel group, CancellationToken ct) {
            if (group == null) {
                throw new ArgumentNullException(nameof(group));
            }
            while (true) {
                group.Id = "grp" + Guid.NewGuid();
                try {
                    var result = await _groups.AddAsync(group.ToDocumentModel(), ct);
                    return result.Value.ToServiceModel();
                }
                catch (ResourceConflictException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TrustGroupRegistrationModel> FindAsync(
            string groupId, CancellationToken ct) {
            if (string.IsNullOrEmpty(groupId)) {
                throw new ArgumentNullException(nameof(groupId));
            }
            var document = await _groups.FindAsync<GroupDocument>(groupId, ct);
            if (document == null) {
                throw new ResourceNotFoundException("No such group");
            }
            return document.Value.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<TrustGroupRegistrationModel> UpdateAsync(string groupId,
            Func<TrustGroupRegistrationModel, bool> predicate,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(groupId)) {
                throw new ArgumentNullException(nameof(groupId));
            }
            while (true) {
                var document = await _groups.FindAsync<GroupDocument>(
                    groupId, ct);
                if (document == null) {
                    throw new ResourceNotFoundException("Group does not exist");
                }
                var group = document.Value.Clone().ToServiceModel();
                if (!predicate(group)) {
                    return group;
                }
                try {
                    var result = await _groups.ReplaceAsync(document,
                        group.ToDocumentModel(), ct);
                    return result.Value.ToServiceModel();
                }
                catch (ResourceOutOfDateException) {
                    // Try again
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TrustGroupRegistrationModel> DeleteAsync(string groupId,
            Func<TrustGroupRegistrationModel, bool> predicate,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(groupId)) {
                throw new ArgumentNullException(nameof(groupId),
                    "The application id must be provided");
            }
            while (true) {
                var document = await _groups.FindAsync<GroupDocument>(
                    groupId, ct);
                if (document == null) {
                    return null;
                }
                var group = document.Value.ToServiceModel();
                if (!predicate(group)) {
                    return group;
                }
                try {
                    // Try delete
                    await _groups.DeleteAsync(document, ct);
                    return group;
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TrustGroupRegistrationListModel> QueryAsync(
            TrustGroupRegistrationQueryModel filter, string nextPageLink, int? pageSize,
            CancellationToken ct) {

            var query = nextPageLink != null ?
                _groups.ContinueQuery<GroupDocument>(nextPageLink, pageSize) :
                CreateQuery(_groups.CreateQuery<GroupDocument>(pageSize), filter);

            // Read results
            var results = await query.ReadAsync(ct);
            return new TrustGroupRegistrationListModel {
                Registrations = results.Select(r => r.Value.ToServiceModel()).ToList(),
                NextPageLink = query.ContinuationToken
            };
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<GroupDocument>> CreateQuery(
            IQuery<GroupDocument> query, TrustGroupRegistrationQueryModel filter) {

            if (filter?.IssuedKeySize != null) {
                query = query.Where(x => x.IssuedKeySize == filter.IssuedKeySize.Value);
            }
            if (filter?.IssuedLifetime != null) {
                query = query.Where(x => x.IssuedLifetime == filter.IssuedLifetime.Value);
            }
            if (filter?.IssuedSignatureAlgorithm != null) {
                query = query.Where(x =>
                    x.IssuedSignatureAlgorithm == filter.IssuedSignatureAlgorithm.Value);
            }
            if (filter?.Type != null) {
                query = query.Where(x => x.Type == filter.Type.Value.ToString());
            }
            if (filter?.Name != null) {
                query = query.Where(x => x.Name == filter.Name);
            }
            if (filter?.ParentId != null) {
                query = query.Where(x => x.ParentId == filter.ParentId);
            }
            if (filter?.SubjectName != null) {
                query = query.Where(x => x.SubjectName == filter.SubjectName);
            }
            if (filter?.Lifetime != null) {
                query = query.Where(x => x.Lifetime == filter.Lifetime.Value);
            }
            if (filter?.KeySize != null) {
                query = query.Where(x => x.KeySize == filter.KeySize.Value);
            }
            if (filter?.SignatureAlgorithm != null) {
                query = query.Where(x => x.SignatureAlgorithm == filter.SignatureAlgorithm);
            }
            query = query.Where(x => x.ClassType == GroupDocument.ClassTypeName);
            return query.GetResults();
        }

        private readonly IItemContainer _groups;
    }
}
