// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;
    using System.Reflection.Metadata;

    public class LiteDbContainerTests : IClassFixture<LiteDbClientFixture> {
        private readonly LiteDbClientFixture _fixture;

        public LiteDbContainerTests(LiteDbClientFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task AddItemsTestAsync() {
            var documents = await _fixture.GetContainerAsync();
            Skip.If(documents == null);

            var families = new Fixture().CreateMany<Family>(10).OrderBy(x => x.Id).ToArray();
            foreach (var f in families) {
                await documents.AddAsync(f);
            }

            var results = await ListAsync<Family, string>(documents, x => x.Id);
            Assert.Equal(families.Select(f => f.Id), results.Select(f => f.Value.Id));
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