// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Clients {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Threading.Tasks;
    using System.Runtime.Serialization;

    public class CouchDbClientFixture {

        /// <summary>
        /// Creates the documents used in this Sample
        /// </summary>
        /// <param name="collection">collection</param>
        /// <returns>None</returns>
        private async Task CreateDocumentsAsync(IItemContainer collection) {
            var AndersonFamily = new Family {
                Id = "AndersenFamily",
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new Parent { FirstName = "Thomas" },
                    new Parent { FirstName = "Mary Kay"}
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new []
                        {
                            new Pet { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = true,
                RegistrationDate = DateTime.UtcNow.AddDays(-1)
            };

            await collection.UpsertAsync(AndersonFamily);

            var WakefieldFamily = new Family {
                Id = "WakefieldFamily",
                LastName = "Wakefield",
                Parents = new[] {
                    new Parent { FamilyName= "Wakefield", FirstName= "Robin" },
                    new Parent { FamilyName= "Miller", FirstName= "Ben" }
                },
                Children = new Child[] {
                    new Child
                    {
                        FamilyName= "Merriam",
                        FirstName= "Jesse",
                        Gender= "female",
                        Grade= 8,
                        Pets= new Pet[] {
                            new Pet { GivenName= "Goofy" },
                            new Pet { GivenName= "Shadow" }
                        }
                    },
                    new Child
                    {
                        FirstName= "Lisa",
                        Gender= "female",
                        Grade= 1
                    }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = false,
                RegistrationDate = DateTime.UtcNow.AddDays(-30)
            };

            await collection.UpsertAsync(WakefieldFamily);
        }

        /// <summary>
        /// Get database
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<IDatabase> GetDatabaseAsync() {
            var logger = ConsoleLogger.Create();
            var server = new MemoryDatabase(logger);
            return await server.OpenAsync("test", null);
        }

        /// <summary>
        /// Get collection interface
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<IItemContainer> GetDocumentsAsync() {
            var docs = await GetContainerAsync();
            if (docs == null) {
                return null;
            }
            await CreateDocumentsAsync(docs);
            return docs;
        }

        /// <summary>
        /// Get collection interface
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<IItemContainer> GetContainerAsync() {
            var database = await Try.Async(() => GetDatabaseAsync());
            if (database == null) {
                return null;
            }
            var docs = await database.OpenContainerAsync("test");
            return docs;
        }
    }

    [DataContract]
    public sealed class Parent {
        [DataMember]
        public string FamilyName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
    }

    [DataContract]
    public sealed class Child {
        [DataMember]
        public string FamilyName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string Gender { get; set; }
        [DataMember]
        public int Grade { get; set; }
        [DataMember]
        public Pet[] Pets { get; set; }
    }

    [DataContract]
    public sealed class Pet {
        [DataMember]
        public string GivenName { get; set; }
    }

    [DataContract]
    public sealed class Address {
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string County { get; set; }
        [DataMember]
        public string City { get; set; }
    }

    [DataContract]
    public sealed class Family {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public Parent[] Parents { get; set; }
        [DataMember]
        public Child[] Children { get; set; }
        [DataMember]
        public Address Address { get; set; }
        [DataMember]
        public bool IsRegistered { get; set; }
        [DataMember]
        public DateTime RegistrationDate { get; set; }
    }
}