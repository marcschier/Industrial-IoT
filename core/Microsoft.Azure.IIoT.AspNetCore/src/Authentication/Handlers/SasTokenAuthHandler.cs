// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore.Authentication.Clients {
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;

    /// <summary>
    /// Access token authentication handler
    /// </summary>
    public class SasTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions> {

        /// <summary>
        /// Create authentication handler
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="context"></param>
        /// <param name="validator"></param>
        public SasTokenAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IHttpContextAccessor context, ISasTokenValidator validator) :
            base(options, logger, encoder, clock) {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <inheritdoc/>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            var request = _context.HttpContext.Request;
            if (!request.Headers.ContainsKey("Authorization")) {
                return AuthenticateResult.Fail("Missing Authorization header");
            }
            try {
                var token = request.Headers["Authorization"][0].Trim();
                var identity = await _validator.ValidateToken(token);

                var principal = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, identity) }, Scheme.Name));
                return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
            }
            catch (Exception ex) {
                return AuthenticateResult.Fail(ex);
            }
        }

        private readonly IHttpContextAccessor _context;
        private readonly ISasTokenValidator _validator;
    }
}