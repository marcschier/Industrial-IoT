// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Service.Api {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Xunit;

    [CollectionDefinition(Name)]
    public class ReadJsonCollection : ICollectionFixture<TestServerFixture> {
        public const string Name = "ReadJsonApi";
    }
}
