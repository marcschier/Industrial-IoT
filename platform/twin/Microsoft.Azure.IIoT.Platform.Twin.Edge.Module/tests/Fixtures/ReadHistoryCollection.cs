// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services.Module.Tests {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Testing.Fixtures;
    using Xunit;

    [CollectionDefinition(Name)]
    public class ReadHistoryCollection : ICollectionFixture<HistoryServerFixture> {

        public const string Name = "ReadHistory";
    }
}
