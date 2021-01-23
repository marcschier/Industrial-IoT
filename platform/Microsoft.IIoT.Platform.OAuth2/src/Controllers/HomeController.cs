// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Microsoft.IIoT.Platform.Identity.Provider.Controllers {
    using Microsoft.IIoT.Platform.Identity.Provider.Filters;
    using Microsoft.IIoT.Platform.Identity.Provider.Models;
    using Microsoft.IIoT.Extensions.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using IdentityServer4.Services;
    using System.Threading.Tasks;

    [SecurityHeaders]
    [ExceptionsFilter]
    [AllowAnonymous]
    public class HomeController : Controller {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;

        public HomeController(IIdentityServerInteractionService interaction,
            IWebHostEnvironment environment, ILogger<HomeController> logger) {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index() {
            if (_environment.IsDevelopment()) {
                // only show in development
                return View();
            }

            LoggerExtensions.LogInformation(_logger, "Homepage is disabled in production. Returning 404.");
            return NotFound();
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public async Task<IActionResult> Error(string errorId) {
#pragma warning restore IDE1006 // Naming Styles
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId).ConfigureAwait(false);
            if (message != null) {
                vm.Error = message;

                if (!_environment.IsDevelopment()) {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }
    }
}