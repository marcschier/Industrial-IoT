// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Microsoft.Azure.IIoT.Platform.Identity.Provider.Controllers {
    using Microsoft.Azure.IIoT.Platform.Identity.Provider.Filters;
    using Microsoft.Azure.IIoT.Platform.Identity.Provider.Models;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Threading.Tasks;

    [SecurityHeaders]
    [ExceptionsFilter]
    [Authorize]
    public class DiagnosticsController : Controller {
#pragma warning disable IDE1006 // Naming Styles
        public async Task<IActionResult> Index() {
#pragma warning restore IDE1006 // Naming Styles
            var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
            if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString())) {
                return NotFound();
            }

            var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());
            return View(model);
        }
    }
}