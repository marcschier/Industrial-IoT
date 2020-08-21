// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.Memory {
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Autofac;

    public class MemoryDatabaseQueryTests : IClassFixture<MemoryDatabaseFixture> {
        public MemoryDatabaseQueryTests(MemoryDatabaseFixture fixture) {
            _fixture = fixture;
        }

        /// <summary>
        /// Dump all documents using linq
        /// </summary>
        [Fact]
        public async Task QueryAllDocuments1Async() {
            var documents = await _fixture.GetDocumentsAsync();
            var query = documents.CreateQuery<dynamic>();
            var results = await RunAsync(documents, query);
            Assert.Equal(2, results.Count);
        }


        [Fact]
        public async Task QueryAllDocuments2Async() {
            var documents = await _fixture.GetDocumentsAsync();
            var families =
                from f in documents.CreateQuery<Family>()
                select f;
            var results = await RunAsync(documents, families);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task QueryWithAndFilterAndProjectionAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var query =
                from f in documents.CreateQuery<Family>()
                where f.Id == "AndersenFamily" || f.Address.City == "NY"
                select new { Name = f.LastName, f.Address.City };

            var results1 = await RunAsync(documents, query);
            Assert.Equal(2, results1.Count);

            var query2 = documents.CreateQuery<Family>(1)
                .Where(d => d.LastName == "Andersen")
                .Select(f => new { Name = f.LastName });

            var results2 = await RunAsync(documents, query2);
            Assert.Single(results2);

            query = documents.CreateQuery<Family>()
                       .Where(f => f.Id == "AndersenFamily" || f.Address.City == "NY")
                       .Select(f => new { Name = f.LastName, f.Address.City });

            results1 = await RunAsync(documents, query);
            Assert.Equal(2, results1.Count);
        }

        [Fact]
        public async Task QueryWithAndFilterAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var families = from f in documents.CreateQuery<Family>()
                           where f.Id == "AndersenFamily" && f.Address.City == "Seattle"
                           select f;

            var results = await RunAsync(documents, families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>()
                .Where(f => f.Id == "AndersenFamily" && f.Address.City == "Seattle");

            results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithEqualsOnIdAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var families =
                from f in documents.CreateQuery<Family>()
                where f.Id == "AndersenFamily"
                select f;

            var results = await RunAsync(documents, families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>().Where(f => f.Id == "AndersenFamily");
            results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithInequalityAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var query = from f in documents.CreateQuery<Family>()
                        where f.Id != "AndersenFamily"
                        select f;

            var results = await RunAsync(documents, query);
            Assert.Single(results);

            query = documents.CreateQuery<Family>()
                       .Where(f => f.Id != "AndersenFamily");

            results = await RunAsync(documents, query);
            Assert.Single(results);

            query =
                from f in documents.CreateQuery<Family>()
                where f.Id == "Wakefield" && f.Address.City != "NY"
                select f;

            results = await RunAsync(documents, query);
            Assert.Empty(results);
        }

        [Fact]
        public async Task QueryWithRangeOperatorsOnNumbersAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var families = from f in documents.CreateQuery<Family>()
                           where f.Children[0].Grade > 5
                           select f;

            var results = await RunAsync(documents, families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>()
                       .Where(f => f.Children[0].Grade > 5);

            results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithRangeOperatorsOnStringsAsync() {
            var documents = await _fixture.GetDocumentsAsync();


            var families = documents.CreateQuery<Family>()
                .Where(f => f.Address.State.CompareTo("NY") > 0);

            var results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithRangeOperatorsDateTimesAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var families = documents.CreateQuery<Family>()
                .Where(f => f.RegistrationDate >= DateTime.UtcNow.AddDays(-3));

            var results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithOrderByNumbersAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var families =
                from f in documents.CreateQuery<Family>()
                where f.LastName == "Andersen"
                orderby f.Children[0].Grade
                select f;

            var results = await RunAsync(documents, families);
            Assert.Single(results);

            // LINQ Lambda
            families = documents.CreateQuery<Family>()
                .Where(f => f.LastName == "Andersen")
                .OrderBy(f => f.Children[0].Grade);

            results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithOrderByStringsAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var families = from f in documents.CreateQuery<Family>()
                           where f.LastName == "Andersen"
                           orderby f.Address.State descending
                           select f;

            var results = await RunAsync(documents, families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>()
                       .Where(f => f.LastName == "Andersen")
                       .OrderByDescending(f => f.Address.State);

            results = await RunAsync(documents, families);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithAggregatesAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var count = documents.CreateQuery<Family>()
                .Where(f => f.LastName == "Andersen")
                .Count();

            Assert.Equal(1, count);

            count = documents.CreateQuery<Family>()
                .SelectMany(f => f.Children)
                .Count();

            Assert.Equal(3, count);

            var maxGrade = documents.CreateQuery<Family>()
                .SelectMany(f => f.Children)
                .Max(c => c.Grade);

            Assert.Equal(8, maxGrade);
        }

        [Fact]
        public async Task QueryWithSubdocumentsAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var children = documents.CreateQuery<Family>()
                     .SelectMany(family => family.Children
                     .Select(c => c));

            var results = await RunAsync(documents, children);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task QueryWithTwoJoinsAndFilterAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var query = documents.CreateQuery<Family>()
                    .SelectMany(family => family.Children
                    .SelectMany(child => child.Pets
                    .Where(pet => pet.GivenName == "Fluffy")
                    .Select(pet => new {
                        family = family.Id,
                        child = child.FirstName,
                        pet = pet.GivenName
                    }
                    )));

            var results = await RunAsync(documents, query);
            Assert.Single(results);
        }

        [Fact]
        public async Task QueryWithTwoJoinsAsync() {
            var documents = await _fixture.GetDocumentsAsync();

            var query = documents.CreateQuery<Family>()
                    .SelectMany(family => family.Children
                    .SelectMany(child => child.Pets
                    .Select(pet => new {
                        family = family.Id,
                        child = child.FirstName,
                        pet = pet.GivenName
                    }
                    )));

            var results = await RunAsync(documents, query);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task QueryWithSingleJoinAsync() {

            var documents = await _fixture.GetDocumentsAsync();

            var query = documents.CreateQuery<Family>()
                    .SelectMany(family => family.Children
                    .Select(c => family.Id));

            var results = await RunAsync(documents, query);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task QueryWithStringMathAndArrayOperatorsAsync() {
            var documents = await _fixture.GetDocumentsAsync();


            var query1 = documents.CreateQuery<Family>()
                .Where(family => family.LastName.StartsWith("An"));
            var results1 = await RunAsync(documents, query1);
            Assert.Single(results1);

            var query2 = documents.CreateQuery<Family>()
                .Select(family => (int)Math.Round((double)family.Children[0].Grade));

            var results2 = await RunAsync(documents, query2);
            Assert.Collection(results2, a => Assert.Equal(5, a.Value), a => Assert.Equal(8, a.Value));

            var query3 = documents.CreateQuery<Family>()
                .Select(family => family.Children.Count());
            var results3 = await RunAsync(documents, query3);
            Assert.Collection(results3, a => Assert.Equal(1, a.Value), a => Assert.Equal(2, a.Value));
        }

        private static async Task<List<IDocumentInfo<T>>> RunAsync<T>(IQuery documents,
            IQueryable<T> query) {
            var feed = documents.GetResults(query);
            var results = new List<IDocumentInfo<T>>();
            while (feed.HasMore()) {
                var result = await feed.ReadAsync();
                foreach (var item in result) {
                    results.Add(item);
                }
            }
            return results;
        }

        private readonly MemoryDatabaseFixture _fixture;
    }
}