namespace Microsoft.IIoT.Platform.Identity.Provider.Controllers {
    using Microsoft.IIoT.Platform.Identity.Provider.Filters;
    using Microsoft.IIoT.Platform.Identity.Provider.Models;
    using Microsoft.IIoT.Platform.Identity.Models;
    using Microsoft.IIoT.Extensions.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using global::IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [SecurityHeaders]
    [ExceptionsFilter]
    [AllowAnonymous]
    public class ExternalController : Controller {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly ILogger<ExternalController> _logger;

        /// <summary>
        /// Create controller
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="interaction"></param>
        /// <param name="clientStore"></param>
        /// <param name="events"></param>
        /// <param name="logger"></param>
        public ExternalController(
            UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            ILogger<ExternalController> logger) {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
            _logger = logger;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl) {
            if (string.IsNullOrEmpty(returnUrl)) {
                returnUrl = "~/";
            }

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false) {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme
            var props = new AuthenticationProperties {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                }
            };

            return Challenge(props, scheme);

        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
#pragma warning disable IDE1006 // Naming Styles
        public async Task<IActionResult> Callback() {
#pragma warning restore IDE1006 // Naming Styles
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme).ConfigureAwait(false);
            if (result?.Succeeded != true) {
                throw new Exception("External authentication error");
            }

            if (_logger.IsEnabled(LogLevel.Debug)) {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                LoggerExtensions.LogDebug(_logger, "External claims: {@claims}", externalClaims);
            }

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result).ConfigureAwait(false);
            if (user == null) {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUserAsync(provider, providerUserId, claims).ConfigureAwait(false);
            }

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            // we must issue the cookie maually, and can't use the SignInManager because
            // it doesn't expose an API to issue additional claims from the login workflow
            var principal = await _signInManager.CreateUserPrincipalAsync(user).ConfigureAwait(false);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

            var isuser = new IdentityServerUser(user.Id) {
                DisplayName = name,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps).ConfigureAwait(false);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme).ConfigureAwait(false);

            // retrieve return URL
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl).ConfigureAwait(false);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name, true, context?.Client.ClientId)).ConfigureAwait(false);

            if (context != null) {
                if (context.IsNativeClient()) {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        private async Task<(UserModel user, string provider, string providerUserId, IEnumerable<Claim> claims)>
            FindUserFromExternalProviderAsync(AuthenticateResult result) {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId).ConfigureAwait(false);

            return (user, provider, providerUserId, claims);
        }

        private async Task<UserModel> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims) {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            // user's display name
            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            if (name != null) {
                filtered.Add(new Claim(JwtClaimTypes.Name, name));
            }
            else {
                var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                if (first != null && last != null) {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null) {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null) {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            // email
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
               claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email != null) {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }

            var user = new UserModel {
                UserName = Guid.NewGuid().ToString(),
            };
            var identityResult = await _userManager.CreateAsync(user).ConfigureAwait(false);
            if (!identityResult.Succeeded) {
                throw new Exception(identityResult.Errors.First().Description);
            }

            if (filtered.Any()) {
                identityResult = await _userManager.AddClaimsAsync(user, filtered).ConfigureAwait(false);
                if (!identityResult.Succeeded) {
                    throw new Exception(identityResult.Errors.First().Description);
                }
            }

            identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider)).ConfigureAwait(false);
            if (!identityResult.Succeeded) {
                throw new Exception(identityResult.Errors.First().Description);
            }

            return user;
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        private static void ProcessLoginCallback(AuthenticateResult externalResult,
            List<Claim> localClaims, AuthenticationProperties localSignInProps) {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null) {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null) {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
    }
}