// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using AutoFixture;
    using Autofac;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class LiteDbQueryTests : IClassFixture<LiteDbClientFixture> {
        private readonly LiteDbClientFixture _fixture;

        public LiteDbQueryTests(LiteDbClientFixture fixture) {
            _fixture = fixture;
        }

        [SkippableFact]
        public async Task QueryAllDocuments1Async() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);
            var query = documents.CreateQuery<dynamic>();
            var results = await RunAsync(query);
            Assert.Equal(2, results.Count);
        }

        [SkippableFact]
        public async Task QueryAllDocuments2Async() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families =
                from f in documents.CreateQuery<Family>()
                select f;
            var results = await RunAsync(families);
            Assert.Equal(2, results.Count);
        }

        [SkippableFact]
        public async Task QueryAllDocuments3Async() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>();
            var results = await RunAsync(families);
            Assert.Equal(2, results.Count);
        }


        [SkippableFact]
        public async Task QueryAndersonAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>(1)
                .Where(d => d.LastName == "Andersen");

            var results = await RunAsync(families);
            Assert.Single(results);
            var family = results.Single().Value;
            Assert.Single(family.Children);
            Assert.Equal(1, family.Children.Select(c => c.Pets.Length).Sum());
        }

        [SkippableFact]
        public async Task QueryWithAndFilterAndProjectionAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var query =
                from f in documents.CreateQuery<Family>()
                where f.Id == "AndersenFamily" || f.Address.City == "NY"
                select new { Name = f.LastName, f.Address.City };

            var results1 = await RunAsync(query);
            Assert.Equal(2, results1.Count);

            var query2 = documents.CreateQuery<Family>(1)
                .Where(d => d.LastName == "Andersen")
                .Select(f => new { Name = f.LastName });

            var results2 = await RunAsync(query2);
            Assert.Single(results2);

            query = documents.CreateQuery<Family>()
                       .Where(f => f.Id == "AndersenFamily" || f.Address.City == "NY")
                       .Select(f => new { Name = f.LastName, f.Address.City });

            results1 = await RunAsync(query);
            Assert.Equal(2, results1.Count);
        }

        [SkippableFact]
        public async Task QueryWithAndFilterAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = from f in documents.CreateQuery<Family>()
                           where f.Id == "AndersenFamily" && f.Address.City == "Seattle"
                           select f;

            var results = await RunAsync(families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>()
                .Where(f => f.Id == "AndersenFamily" && f.Address.City == "Seattle");

            results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithEqualsOnIdAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families =
                from f in documents.CreateQuery<Family>()
                where f.Id == "AndersenFamily"
                select f;

            var results = await RunAsync(families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>().Where(f => f.Id == "AndersenFamily");
            results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithInequalityAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var query = from f in documents.CreateQuery<Family>()
                        where f.Id != "AndersenFamily"
                        select f;

            var results = await RunAsync(query);
            Assert.Single(results);

            query = documents.CreateQuery<Family>()
                       .Where(f => f.Id != "AndersenFamily");

            results = await RunAsync(query);
            Assert.Single(results);

            query =
                from f in documents.CreateQuery<Family>()
                where f.Id == "Wakefield" && f.Address.City != "NY"
                select f;

            results = await RunAsync(query);
            Assert.Empty(results);
        }

        [SkippableFact]
        public async Task QueryWithRangeOperatorsOnNumbersAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);
            var families = from f in documents.CreateQuery<Family>()
                           where f.Children[0].Grade > 5
                           select f;

            var results = await RunAsync(families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>()
                       .Where(f => f.Children[0].Grade > 5);

            results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithRangeOperatorsOnStringsAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>()
                .Where(f => f.Address.State.Equals("NY", StringComparison.OrdinalIgnoreCase));

            var results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithRangeOperatorsDateTimesAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var date = DateTime.UtcNow.AddDays(-3);
            var families = documents.CreateQuery<Family>()
                .Where(f => f.RegistrationDate >= date);

            var results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithOrderByDateTimesAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>()
                .OrderBy(f => f.RegistrationDate);

            var results = await RunAsync(families);
            Assert.Equal(2, results.Count);
            Assert.Equal("WakefieldFamily", results.First().Id);
        }

        [SkippableFact]
        public async Task QueryWithOrderByDescendingDateTimesAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>()
                .OrderByDescending(f => f.RegistrationDate);

            var results = await RunAsync(families);
            Assert.Equal(2, results.Count);
            Assert.Equal("AndersenFamily", results.First().Id);
        }

        [SkippableFact]
        public async Task QueryWithOrderByDateTimesLimitAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>()
                .OrderBy(f => f.RegistrationDate)
                .Take(1);

            var results = await RunAsync(families);
            Assert.Single(results);
            Assert.Equal("WakefieldFamily", results.Single().Id);
        }

        [SkippableFact]
        public async Task QueryWithOrderByDescendingDateTimesLimitAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var families = documents.CreateQuery<Family>()
                .OrderByDescending(f => f.RegistrationDate)
                .Take(1);

            var results = await RunAsync(families);
            Assert.Single(results);
            Assert.Equal("AndersenFamily", results.Single().Id);
        }

        [SkippableFact]
        public async Task QueryWithOrderByNumbersAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);
            var families =
                from f in documents.CreateQuery<Family>()
                where f.LastName == "Andersen"
                orderby f.Children[0].Grade
                select f;

            var results = await RunAsync(families);
            Assert.Single(results);

            // LINQ Lambda
            families = documents.CreateQuery<Family>()
                .Where(f => f.LastName == "Andersen")
                .OrderBy(f => f.Children[0].Grade);

            results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithOrderByStringsAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);
            var families = from f in documents.CreateQuery<Family>()
                           where f.LastName == "Andersen"
                           orderby f.Address.State descending
                           select f;

            var results = await RunAsync(families);
            Assert.Single(results);

            families = documents.CreateQuery<Family>()
                       .Where(f => f.LastName == "Andersen")
                       .OrderByDescending(f => f.Address.State);

            results = await RunAsync(families);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithCountAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var count = await documents.CreateQuery<Family>()
                .Where(f => f.LastName == "Andersen")
                .CountAsync();

            Assert.Equal(1, count);

            count = await documents.CreateQuery<Family>()
                .SelectMany(f => f.Children)
                .CountAsync();

            Assert.Equal(3, count);
        }

        [SkippableFact]
        public async Task QueryWithSubdocumentsAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var children = documents.CreateQuery<Family>()
                .SelectMany(family => family.Children.Select(c => c));

            var results = await RunAsync(children);
            Assert.Equal(3, results.Count);
        }

        [SkippableFact]
        public async Task QueryWithTwoJoinsAndFilterAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var query = documents.CreateQuery<Family>().Where(family => family.Children != null)
                    .SelectMany(family => family.Children.Where(child => child.Pets != null)
                    .SelectMany(child => child.Pets
                    .Where(pet => pet.GivenName == "Fluffy")
                    .Select(pet => new {
                        family = family.Id,
                        child = child.FirstName,
                        pet = pet.GivenName
                    }
                    )));

            var results = await RunAsync(query);
            Assert.Single(results);
        }

        [SkippableFact]
        public async Task QueryWithTwoJoinsAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var query = documents.CreateQuery<Family>().Where(family => family.Children != null)
                    .SelectMany(family => family.Children.Where(child => child.Pets != null)
                    .SelectMany(child => child.Pets
                    .Select(pet => new {
                        family = family.Id,
                        child = child.FirstName,
                        pet = pet.GivenName
                    }
                    )));

            var results = await RunAsync(query);
            Assert.Equal(3, results.Count);
        }

        [SkippableFact]
        public async Task QueryWithSingleJoinAsync() {

            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var query = documents.CreateQuery<Family>()
                    .SelectMany(family => family.Children
                    .Select(c => family.Id));

            var results = await RunAsync(query);
            Assert.Equal(3, results.Count);
        }

        [SkippableFact]
        public async Task QueryWithStringMathAndArrayOperatorsAsync() {
            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            var query1 = documents.CreateQuery<Family>()
                .Where(family => family.LastName.StartsWith("An"));
            var results1 = await RunAsync(query1);
            Assert.Single(results1);

            var query2 = documents.CreateQuery<Family>()
                .Select(family => (int)Math.Round((double)family.Children[0].Grade));

            var results2 = await RunAsync(query2);
            Assert.Collection(results2, a => Assert.Equal(5, a.Value), a => Assert.Equal(8, a.Value));

            var query3 = documents.CreateQuery<Family>()
                .Select(family => family.Children.Count());
            var results3 = await RunAsync(query3);
            Assert.Collection(results3, a => Assert.Equal(1, a.Value), a => Assert.Equal(2, a.Value));
        }

        [SkippableFact]
        public async Task QueryWithDistinct1Async() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var now = DateTime.UtcNow;
                var families = new Fixture().CreateMany<Family>(20);
                foreach (var f in families) {
                    f.LastName = "Same";
                    f.RegistrationDate = now;
                    f.IsRegistered = true;
                    f.Count = 6;
                    await documents.UpsertAsync(f);
                }

                var query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .Distinct();
                var results1 = await RunAsync(query1);
                Assert.Single(results1);
                Assert.Equal("Same", results1.Single().Value);

                var query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .Distinct();
                var results2 = await RunAsync(query2);
                Assert.Single(results2);
                Assert.Equal(6, results2.Single().Value);

                var query3 = documents.CreateQuery<Family>()
                    .Select(x => x.RegistrationDate)
                    .Distinct();
                var results3 = await RunAsync(query3);
                Assert.Single(results3);
                Assert.Equal(now, results3.Single().Value);

                var query4 = documents.CreateQuery<Family>()
                    .Select(x => x.IsRegistered)
                    .Distinct();
                var results4 = await RunAsync(query4);
                Assert.Single(results4);
                Assert.True(results4.Single().Value);
            }
        }


        [SkippableFact]
        public async Task QueryWithDistinct2Async() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(5);
                foreach (var f in families) {
                    f.LastName = "Same";
                    f.Count = 6;
                    await documents.UpsertAsync(f);
                }

                families = new Fixture().CreateMany<Family>(5);
                foreach (var f in families) {
                    f.LastName = "Other";
                    f.Count = null;
                    await documents.UpsertAsync(f);
                }

                var query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .Distinct();
                var results1 = await RunAsync(query1);
                Assert.Equal(2, results1.Count);

                query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .Distinct()
                    .OrderBy(x => x);
                results1 = await RunAsync(query1);
                Assert.Equal(2, results1.Count);
                Assert.Equal("Other", results1.First().Value);
                query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .Distinct()
                    .OrderByDescending(x => x);
                results1 = await RunAsync(query1);
                Assert.Equal(2, results1.Count);
                Assert.Equal("Same", results1.First().Value);

                var query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .Distinct();
                var results2 = await RunAsync(query2);
                Assert.Equal(2, results2.Count);

                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .Distinct()
                    .OrderBy(x => x);
                results2 = await RunAsync(query2);
                Assert.Equal(2, results2.Count);
                Assert.Null(results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .Distinct()
                    .OrderByDescending(x => x);
                results2 = await RunAsync(query2);
                Assert.Equal(2, results2.Count);
                Assert.Equal(6, results2.First().Value);

                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(1);
                results2 = await RunAsync(query2);
                Assert.Single(results2);
                Assert.Null(results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .Take(1);
                results2 = await RunAsync(query2);
                Assert.Single(results2);
                Assert.Equal(6, results2.First().Value);
            }
        }

        [SkippableFact]
        public async Task QueryWithSelectAndOrderByAsync() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var count = 0;
                var families = new Fixture().CreateMany<Family>(5);
                foreach (var f in families) {
                    f.LastName = "Same";
                    f.Count = ++count;
                    await documents.UpsertAsync(f);
                }

                families = new Fixture().CreateMany<Family>(5);
                foreach (var f in families) {
                    f.LastName = "Other";
                    f.Count = ++count;
                    await documents.UpsertAsync(f);
                }

                var query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .OrderBy(x => x);
                var results1 = await RunAsync(query1);
                Assert.Equal(10, results1.Count);
                Assert.Equal("Other", results1.First().Value);
                query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .OrderByDescending(x => x);
                results1 = await RunAsync(query1);
                Assert.Equal(10, results1.Count);
                Assert.Equal("Same", results1.First().Value);
                query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .OrderByDescending(x => x)
                    .Take(1);
                results1 = await RunAsync(query1);
                Assert.Single(results1);
                Assert.Equal("Same", results1.First().Value);
                query1 = documents.CreateQuery<Family>()
                    .Select(x => x.LastName)
                    .OrderBy(x => x)
                    .Take(1);
                results1 = await RunAsync(query1);
                Assert.Single(results1);
                Assert.Equal("Other", results1.First().Value);

                var query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .OrderBy(x => x);
                var results2 = await RunAsync(query2);
                Assert.Equal(10, results2.Count);
                Assert.Equal(1, results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .OrderByDescending(x => x);
                results2 = await RunAsync(query2);
                Assert.Equal(10, results2.Count);
                Assert.Equal(10, results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .OrderByDescending(x => x)
                    .Take(1);
                results2 = await RunAsync(query2);
                Assert.Single(results1);
                Assert.Equal(10, results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .OrderBy(x => x)
                    .Take(2);
                results2 = await RunAsync(query2);
                Assert.Equal(2, results2.Count);
                Assert.Equal(1, results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .OrderBy(x => x)
                    .Take(100);
                results2 = await RunAsync(query2);
                Assert.Equal(10, results2.Count);
                Assert.Equal(1, results2.First().Value);
                query2 = documents.CreateQuery<Family>()
                    .Select(x => x.Count)
                    .OrderBy(x => x)
                    .Where(x => x > 3);
                results2 = await RunAsync(query2);
                Assert.Equal(7, results2.Count);
                Assert.Equal(4, results2.First().Value);
            }
        }

        [SkippableFact]
        public async Task QueryContinueTest1Async() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(100);
                foreach (var f in families) {
                    await documents.UpsertAsync(f);
                }

                var query1 = documents.CreateQuery<Family>(10);
                var results1 = await RunAsync(query1);
                Assert.Equal(100, results1.Count);

                var query2 = documents.CreateQuery<Family>(10);
                var results2 = query2.GetResults();

                var loops = 0;
                for (var i = 0; i < 20; i++) {
                    loops++;
                    if (results2.HasMore()) {
                        var result = await results2.ReadAsync();
                        Assert.NotNull(result);
                        Assert.NotEmpty(result);
                        Assert.Equal(10, result.Count());
                    }
                    if (results2.ContinuationToken == null) {
                        break;
                    }
                    results2 = documents.ContinueQuery<Family>(results2.ContinuationToken, 10);
                }
                Assert.Equal(10, loops);
            }
        }

        [SkippableFact]
        public async Task QueryContinueTest2Async() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(100);
                foreach (var f in families) {
                    await documents.UpsertAsync(f);
                }

                var query1 = documents.CreateQuery<Family>(10);
                var results1 = await RunAsync(query1);
                Assert.Equal(100, results1.Count);

                var query2 = documents.CreateQuery<Family>(10);
                var results2 = query2.GetResults();

                var loops = 0;
                for (var i = 0; i < 20; i++) {
                    loops++;
                    var result = await results2.ReadAsync();
                    Assert.NotNull(result);
                    Assert.NotEmpty(result);
                    Assert.Equal(10, result.Count());
                    if (results2.ContinuationToken == null) {
                        result = await results2.ReadAsync();
                        Assert.NotNull(result);
                        Assert.Empty(result);
                        break;
                    }
                    results2 = documents.ContinueQuery<Family>(results2.ContinuationToken, 10);
                }
                Assert.Equal(10, loops);
            }
        }

        [SkippableFact]
        public async Task QueryContinueTest3Async() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(5);
                foreach (var f in families) {
                    await documents.UpsertAsync(f);
                }

                var query1 = documents.CreateQuery<Family>(10);
                var results1 = await RunAsync(query1);
                Assert.Equal(5, results1.Count);

                var query2 = documents.CreateQuery<Family>(10);
                var results2 = query2.GetResults();

                Assert.True(results2.HasMore());
                var result = await results2.ReadAsync();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Equal(5, result.Count());
                Assert.Null(results2.ContinuationToken);
            }
        }

        [SkippableFact]
        public async Task QueryContinueTest4Async() {
            using (var container = await _fixture.GetContainerAsync()) {
                Skip.If(container == null);
                var documents = container.Container;

                var families = new Fixture().CreateMany<Family>(100);
                foreach (var f in families) {
                    await documents.UpsertAsync(f);
                }

                var query2 = documents.CreateQuery<Family>(10);
                var results2 = query2.GetResults();

                Assert.True(results2.HasMore());
                var result = await results2.ReadAsync();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Equal(10, result.Count());
                Assert.NotNull(results2.ContinuationToken);

                results2 = documents.ContinueQuery<Family>(results2.ContinuationToken, 40);
                Assert.True(results2.HasMore());
                result = await results2.ReadAsync();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Equal(40, result.Count());
                Assert.NotNull(results2.ContinuationToken);

                results2 = documents.ContinueQuery<Family>(results2.ContinuationToken);
                Assert.True(results2.HasMore());
                result = await results2.ReadAsync();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Equal(50, result.Count());
                Assert.Null(results2.ContinuationToken);
            }
        }

        [SkippableFact]
        public async Task QueryContinueBadArgumentsThrowsAsync() {

            var documents = await _fixture.GetDocumentsAsync();
            Skip.If(documents == null);

            Assert.Throws<ArgumentNullException>(() => documents.ContinueQuery<Family>(null));

            await Assert.ThrowsAnyAsync<ArgumentException>(() => {
                var results = documents.ContinueQuery<Family>("badtoken");
                return results.ReadAsync();
            });
            await Assert.ThrowsAnyAsync<ArgumentException>(() => {
                var results = documents.ContinueQuery<Family>("{}");
                return results.ReadAsync();
            });
        }

        private static async Task<List<IDocumentInfo<T>>> RunAsync<T>(IQuery<T> query) {
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
