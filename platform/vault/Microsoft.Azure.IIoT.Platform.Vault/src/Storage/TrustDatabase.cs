// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Storage {
    using Microsoft.Azure.IIoT.Platform.Vault.Storage.Models;
    using Microsoft.Azure.IIoT.Platform.Vault.Models;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Trust relationship database
    /// </summary>
    public sealed class TrustDatabase : ITrustRepository {

        /// <summary>
        /// Create relationship database
        /// </summary>
        /// <param name="db"></param>
        public TrustDatabase(ICollectionFactory db) {
            if (db == null) {
                throw new ArgumentNullException(nameof(db));
            }
            _relationships = db.OpenAsync("trust").Result;
        }

        /// <inheritdoc/>
        public async Task<TrustRelationshipModel> AddAsync(TrustRelationshipModel relationship,
            CancellationToken ct) {
            if (relationship == null) {
                throw new ArgumentNullException(nameof(relationship));
            }
            var result = await _relationships.AddAsync(relationship.ToDocumentModel(), ct: ct).ConfigureAwait(false);
            return result.Value.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string subjectId, TrustDirectionType? direction,
            string objectId, CancellationToken ct) {
            if (string.IsNullOrEmpty(subjectId)) {
                throw new ArgumentNullException(nameof(subjectId));
            }
            var query = CreateQuery(_relationships.CreateQuery<TrustDocument>(), 
                subjectId, direction, objectId);
            await query.ForEachAsync(
                d => _relationships.DeleteAsync<TrustDocument>(d.Id, ct: ct), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TrustRelationshipListModel> ListAsync(
            string entityId, TrustDirectionType? direction, string nextPageLink,
            int? pageSize, CancellationToken ct) {

            if (string.IsNullOrEmpty(entityId)) {
                throw new ArgumentNullException(nameof(entityId));
            }

            var query = nextPageLink != null ?
                _relationships.ContinueQuery<TrustDocument>(nextPageLink, pageSize) :
                CreateQuery(_relationships.CreateQuery<TrustDocument>(pageSize),
                    entityId, direction, null);

            // Read results
            var results = await query.ReadAsync(ct).ConfigureAwait(false);
            return new TrustRelationshipListModel {
                Relationships = results.Select(r => r.Value.ToServiceModel()).ToList(),
                NextPageLink = query.ContinuationToken
            };
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="direction"></param>
        /// <param name="objectId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<TrustDocument>> CreateQuery(IQuery<TrustDocument> query,
            string entityId, TrustDirectionType? direction, string objectId) {

            var trusted = direction == null ||
                TrustDirectionType.Trusted == (direction.Value & TrustDirectionType.Trusted);
            var trusts = direction == null ||
                TrustDirectionType.Trusting == (direction.Value & TrustDirectionType.Trusting);

            if (trusted && trusts) {
                if (objectId == null) {
                    query = query.Where(x => 
                        x.TrustedId == entityId || x.TrustingId == entityId);
                }
                else {
                    query = query.Where(x =>
                        (x.TrustedId == entityId && x.TrustingId == objectId) ||
                        (x.TrustedId == objectId && x.TrustingId == entityId));
                }
            }
            else if (trusted) {
                if (objectId == null) {
                    query = query.Where(x => x.TrustedId == entityId);
                }
                else {
                    query = query.Where(x =>
                        x.TrustedId == entityId && x.TrustingId == objectId);
                }
            }
            else if (trusts) {
                if (objectId == null) {
                    query = query.Where(x => x.TrustingId == entityId);
                }
                else {
                    query = query.Where(x =>
                        x.TrustedId == objectId && x.TrustingId == entityId);
                }
            }
            query = query.Where(x => x.ClassType == TrustDocument.ClassTypeName);
            return query.GetResults();
        }

        private readonly IDocumentCollection _relationships;
    }
}
