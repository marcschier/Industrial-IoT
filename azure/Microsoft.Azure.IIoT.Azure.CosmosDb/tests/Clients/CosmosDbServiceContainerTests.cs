﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public class CosmosDbServiceContainerTests : IClassFixture<CosmosDbServiceClientFixture> {
        private readonly CosmosDbServiceClientFixture _fixture;

        public CosmosDbServiceContainerTests(CosmosDbServiceClientFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task FindItemTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = await documents.FindAsync<Family>(fr.Id);
                Assert.Equal(fr.Id, f2.Id);
                Assert.Equal(fr.Id, f2.Value.Id);
                Assert.Equal(fr.Value.LastName, f2.Value.LastName);
                Assert.NotNull(f2.Etag);
                Assert.Equal(f2.Etag, fr.Etag);
            }
        }

        [SkippableFact]
        public async Task NotFindItemTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = await documents.FindAsync<Family>("xyz");
                Assert.Null(f2);
            }
        }

        [SkippableFact]
        public async Task FindItemBadArgumentTestsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                await Assert.ThrowsAsync<ArgumentNullException>(
                () => documents.FindAsync<Family>(null, default, null));
            }
        }

        [SkippableFact]
        public async Task AddItemsTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(10).OrderBy(x => x.Id).ToArray();
                foreach (var f in families) {
                    await documents.AddAsync(f);
                }

                var results = await ListAsync<Family, string>(documents, x => x.Id);
                Assert.Equal(families.Select(f => f.Id), results.Select(f => f.Value.Id));
            }
        }

        [SkippableFact]
        public async Task AddItemTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f.Id, results.Single().Id);
                Assert.Equal(f.Id, results.Single().Value.Id);
                Assert.Equal(f.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(fr.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task AddItemTwiceThrowsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                f2.Id = f.Id;
                await Assert.ThrowsAsync<ResourceConflictException>(() => documents.AddAsync(f2));
            }
        }

        [SkippableFact]
        public async Task AddItemAfterUpsertThrowsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                f2.Id = f.Id;
                await Assert.ThrowsAsync<ResourceConflictException>(() => documents.AddAsync(f2));
            }
        }

        [SkippableFact]
        public async Task AddItemBadArgumentTestsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                await Assert.ThrowsAsync<ArgumentNullException>(
                () => documents.AddAsync<Family>(null, default, "good", null));
                await Assert.ThrowsAsync<NotSupportedException>(
                    () => documents.AddAsync(1, default, "badid", null));
            }
        }

        [SkippableFact]
        public async Task UpsertItemsTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(10).OrderBy(x => x.Id).ToArray();
                foreach (var f in families) {
                    await documents.UpsertAsync(f);
                }

                var results = await ListAsync<Family, string>(documents, x => x.Id);
                Assert.Equal(families.Select(f => f.Id), results.Select(f => f.Value.Id));
            }
        }

        [SkippableFact]
        public async Task UpsertItemTwiceAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f.Id, results.Single().Id);
                Assert.Equal(f.Id, results.Single().Value.Id);
                Assert.Equal(f.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(fr.Etag, results.Single().Etag);

                var f2 = new Fixture().Create<Family>();
                f2.Id = f.Id;
                var f3 = await documents.UpsertAsync(f2);
                Assert.Equal(f2.Id, f3.Id);
                Assert.Equal(f2.Id, f3.Value.Id);
                Assert.Equal(f2.LastName, f3.Value.LastName);
                Assert.NotNull(f3.Etag);
                Assert.NotEqual(f3.Etag, fr.Etag);

                results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f2.Id, results.Single().Id);
                Assert.Equal(f2.Id, results.Single().Value.Id);
                Assert.Equal(f2.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(f3.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task UpsertItemTwiceWithEtagAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                f2.Id = f.Id;
                var f3 = await documents.UpsertAsync(f2, etag: fr.Etag);
                Assert.Equal(f2.Id, f3.Id);
                Assert.Equal(f2.Id, f3.Value.Id);
                Assert.Equal(f2.LastName, f3.Value.LastName);
                Assert.NotNull(f3.Etag);
                Assert.NotEqual(f3.Etag, fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f2.Id, results.Single().Id);
                Assert.Equal(f2.Id, results.Single().Value.Id);
                Assert.Equal(f2.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(f3.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task UpsertItemFirstTimeWithEtagInsertsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f, etag: "OldEtag");
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(fr.Id, results.Single().Id);
                Assert.Equal(fr.Id, results.Single().Value.Id);
                Assert.Equal(fr.Value.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(fr.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task UpsertItemTwiceWithBadEtagThrowsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                f2.Id = f.Id;
                await Assert.ThrowsAsync<ResourceOutOfDateException>(
                    () => documents.UpsertAsync(f2, etag: "bad"));
            }
        }

        [SkippableFact]
        public async Task UpsertItemAfterAddAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                f2.Id = f.Id;
                var f3 = await documents.UpsertAsync(f2);
                Assert.Equal(f2.Id, f3.Id);
                Assert.Equal(f2.Id, f3.Value.Id);
                Assert.Equal(f2.LastName, f3.Value.LastName);
                Assert.NotNull(f3.Etag);
                Assert.NotEqual(f3.Etag, fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f2.Id, results.Single().Id);
                Assert.Equal(f2.Id, results.Single().Value.Id);
                Assert.Equal(f2.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(f3.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task UpsertItemBadArgumentTestsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => documents.UpsertAsync<Family>(null, default, "good", null));
                await Assert.ThrowsAsync<NotSupportedException>(
                    () => documents.UpsertAsync(1, default, "badid", null));
            }
        }

        [SkippableFact]
        public async Task ReplaceItemAfterAddAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                var f3 = await documents.ReplaceAsync(fr, f2);
                Assert.Equal(f.Id, f3.Id);
                Assert.Equal(f.Id, f3.Value.Id); // Id was overridden with f.id
                Assert.Equal(f2.LastName, f3.Value.LastName);
                Assert.NotNull(f3.Etag);
                Assert.NotEqual(f3.Etag, fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f.Id, results.Single().Id);
                Assert.Equal(f.Id, results.Single().Value.Id);
                Assert.Equal(f2.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(f3.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task ReplaceItemAfterUpsertAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                var f2 = new Fixture().Create<Family>();
                var f3 = await documents.ReplaceAsync(fr, f2);
                Assert.Equal(f.Id, f3.Id);
                Assert.Equal(f.Id, f3.Value.Id); // Id was overridden with f.id
                Assert.Equal(f2.LastName, f3.Value.LastName);
                Assert.NotNull(f3.Etag);
                Assert.NotEqual(f3.Etag, fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Single(results);
                Assert.Equal(f.Id, results.Single().Id);
                Assert.Equal(f.Id, results.Single().Value.Id);
                Assert.Equal(f2.LastName, results.Single().Value.LastName);
                Assert.NotNull(results.Single().Etag);
                Assert.Equal(f3.Etag, results.Single().Etag);
            }
        }

        [SkippableFact]
        public async Task ReplaceItemWithBadEtagThrowsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);

                var f2 = new Fixture().Create<Family>();
                var f3 = await documents.ReplaceAsync(fr, f2);
                var f4 = new Fixture().Create<Family>();
                await Assert.ThrowsAsync<ResourceOutOfDateException>(
                    () => documents.ReplaceAsync(fr, f4, default, null));

                await documents.DeleteAsync<Family>(f.Id);
                var results = await ListAsync<Family>(documents);
                Assert.Empty(results);

                var f5 = new Fixture().Create<Family>();
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => documents.ReplaceAsync(f3, f5, default, null));
            }
        }

        [SkippableFact]
        public async Task ReplaceItemBadArgumentTestsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => documents.ReplaceAsync(fr, null, default, null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => documents.ReplaceAsync(null, f, default, null));
            }
        }

        [SkippableFact]
        public async Task DeleteItemAfterAddAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                await documents.DeleteAsync(fr);

                var results = await ListAsync<Family>(documents);
                Assert.Empty(results);
            }
        }

        [SkippableFact]
        public async Task DeleteItemAfterUpsertAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.UpsertAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                await documents.DeleteAsync(fr);

                var results = await ListAsync<Family>(documents);
                Assert.Empty(results);
            }
        }

        [SkippableFact]
        public async Task DeleteItemWithGoodEtagTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                await documents.DeleteAsync<Family>(fr.Id, etag: fr.Etag);

                var results = await ListAsync<Family>(documents);
                Assert.Empty(results);
            }
        }

        [SkippableFact]
        public async Task DeleteItemWithNoEtagTestAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                await documents.DeleteAsync<Family>(fr.Id);

                var results = await ListAsync<Family>(documents);
                Assert.Empty(results);
            }
        }

        [SkippableFact]
        public async Task DeleteItemWithBadEtagThrowsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var f = new Fixture().Create<Family>();
                var fr = await documents.AddAsync(f);
                Assert.Equal(f.Id, fr.Id);
                Assert.Equal(f.Id, fr.Value.Id);
                Assert.Equal(f.LastName, fr.Value.LastName);
                Assert.NotNull(fr.Etag);

                await Assert.ThrowsAsync<ResourceOutOfDateException>(
                    () => documents.DeleteAsync<Family>(fr.Id, etag: "bad"));

            }
        }

        [SkippableFact]
        public async Task DeleteItemBadArgumentTestsAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => documents.DeleteAsync<Family>(null, default, null));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => documents.DeleteAsync<Family>(null, default, null, etag: "good"));
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => documents.DeleteAsync<Family>(string.Empty, default, null, etag: "good"));
                await Assert.ThrowsAsync<ResourceNotFoundException>(
                    () => documents.DeleteAsync<Family>("bbbbb", default, null, etag: "good"));
            }
        }

        private static Task<List<IDocumentInfo<T>>> ListAsync<T>(
            IItemContainer documents) {
            return ListAsync<T, object>(documents, null);
        }

        private static async Task<List<IDocumentInfo<T>>> ListAsync<T, K>(
            IItemContainer documents, Expression<Func<T, K>> order) {
            var query = documents.CreateQuery<T>();
            if (order != null) {
                query = query.OrderBy(order);
            }
            var feed = query.GetResults();
            var results = new List<IDocumentInfo<T>>();
            while (feed.HasMore()) {
                var result = await feed.ReadAsync();
                foreach (var item in result) {
                    results.Add(item);
                }
            }
            return results;
        }
    }
}