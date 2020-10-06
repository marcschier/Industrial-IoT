// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE1006 // Naming Styles
    public partial class _DrawerAssetContent {
        [Parameter]
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0079 // Remove unnecessary suppression
        public ApplicationInfoApiModel ApplicationData { get; set; }
    }
}
