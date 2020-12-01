// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Identity.Storage {
    using Microsoft.IIoT.Platform.Identity.Models;
    using Microsoft.IIoT.Storage;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Security.Claims;

    /// <summary>
    /// Represents a store for Identity users
    /// </summary>
    public sealed class UserDatabase : IUserStore<UserModel>,
        IUserClaimStore<UserModel>, IUserLoginStore<UserModel>,
        IUserRoleStore<UserModel>, IUserPasswordStore<UserModel>,
        IUserSecurityStampStore<UserModel>, IUserTwoFactorStore<UserModel>,
        IUserPhoneNumberStore<UserModel>, IUserEmailStore<UserModel>,
        IUserAuthenticatorKeyStore<UserModel>,
        IUserTwoFactorRecoveryCodeStore<UserModel>,
        IUserLockoutStore<UserModel> {

        /// <summary>
        /// Create client store
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="roleStore"></param>
        public UserDatabase(ICollectionFactory factory,
            IRoleStore<RoleModel> roleStore) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }
            _roleStore = roleStore;
            _documents = factory.OpenAsync("identity").Result;
        }


        /// <inheritdoc/>
        public async Task<IdentityResult> CreateAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            try {
                var document = user.ToDocumentModel();
                await _documents.AddAsync(document, ct: ct).ConfigureAwait(false);
                return IdentityResult.Success;
            }
            catch {
                return IdentityResult.Failed();
            }
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> UpdateAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            while (true) {
                var document = await _documents.FindAsync<UserDocumentModel>(
                    user.Id, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return IdentityResult.Failed();
                }
                try {
                    var newDocument = document.Value.UpdateFrom(user);
                    document = await _documents.ReplaceAsync(
                        document, newDocument, ct: ct).ConfigureAwait(false);
                    return IdentityResult.Success;
                }
                catch (ResourceOutOfDateException) {
                    continue; // Replace failed due to etag out of date - retry
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> DeleteAsync(UserModel user,
            CancellationToken ct) {
            try {
                await _documents.DeleteAsync<UserDocumentModel>(user.Id, null, user.ConcurrencyStamp,
                    ct).ConfigureAwait(false);
                return IdentityResult.Success;
            }
            catch {
                return IdentityResult.Failed();
            }
        }

        /// <inheritdoc/>
        public async Task<UserModel> FindByIdAsync(string userId,
            CancellationToken ct) {
            var user = await _documents.FindAsync<UserDocumentModel>(userId, ct: ct).ConfigureAwait(false);
            if (user?.Value == null) {
                return null;
            }
            return user.Value.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<UserModel> FindByNameAsync(string normalizedUserName,
            CancellationToken ct) {
            if (normalizedUserName == null) {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }
            normalizedUserName = normalizedUserName.ToLowerInvariant();
            var results = _documents.CreateQuery<UserDocumentModel>(1)
                .Where(x => x.NormalizedUserName == normalizedUserName)
                .GetResults();
            if (results.HasMore()) {
                var documents = await results.ReadAsync(ct).ConfigureAwait(false);
                return documents.FirstOrDefault().Value.ToServiceModel();
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<UserModel> FindByLoginAsync(string loginProvider,
            string providerKey, CancellationToken ct) {
            if (loginProvider == null) {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (providerKey == null) {
                throw new ArgumentNullException(nameof(providerKey));
            }
            var results = _documents.CreateQuery<UserDocumentModel>(1)
                //   .Where(x => x.Provider == loginProvider)
                //   .Where(x => x.ProviderKey == providerKey) // TODO
                .GetResults();
            if (results.HasMore()) {
                var documents = await results.ReadAsync(ct).ConfigureAwait(false);
                return documents.FirstOrDefault().Value.ToServiceModel();
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<UserModel> FindByEmailAsync(string normalizedEmail,
            CancellationToken ct) {
            if (normalizedEmail == null) {
                throw new ArgumentNullException(nameof(normalizedEmail));
            }
            normalizedEmail = normalizedEmail.ToLowerInvariant();
            var results = _documents.CreateQuery<UserDocumentModel>(1)
                .Where(x => x.NormalizedEmail == normalizedEmail)
                .GetResults();
            if (results.HasMore()) {
                var documents = await results.ReadAsync(ct).ConfigureAwait(false);
                return documents.FirstOrDefault().Value.ToServiceModel();
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<IList<UserModel>> GetUsersInRoleAsync(
            string normalizedRoleName, CancellationToken ct) {
            if (normalizedRoleName == null) {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }
            normalizedRoleName = normalizedRoleName.ToLowerInvariant();
            var results = _documents.CreateQuery<UserDocumentModel>()
                .Where(x => x.Roles != null)
                .Where(x => x.Roles.Contains(normalizedRoleName))
                .GetResults();
            var users = new List<UserModel>();
            if (results.HasMore()) {
                var documents = await results.ReadAsync(ct).ConfigureAwait(false);
                users.AddRange(documents.Select(d => d.Value.ToServiceModel()));
            }
            return users;
        }

        /// <inheritdoc/>
        public async Task AddToRoleAsync(UserModel user, string normalizedRoleName,
            CancellationToken ct) {
            if (normalizedRoleName == null) {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }
            // Check if the given role name exists
            var foundRole = await _roleStore.FindByNameAsync(
                normalizedRoleName, ct).ConfigureAwait(false);
            if (foundRole == null) {
                throw new ArgumentException(
                    $"The role {normalizedRoleName} does not exist",
                    nameof(normalizedRoleName));
            }
            await UpdateUserDocumentAsync(user, document => {
                document.Roles.Add(normalizedRoleName);
                return document;
            }, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task RemoveFromRoleAsync(UserModel user, string normalizedRoleName,
            CancellationToken ct) {
            if (normalizedRoleName == null) {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }
            return UpdateUserDocumentAsync(user, document => {
                document.Roles.Remove(normalizedRoleName);
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task<IList<string>> GetRolesAsync(UserModel user,
            CancellationToken ct) {
            return GetFromUserDocumentAsync<IList<string>>(user,
                document => document.Roles, ct);
        }

        /// <inheritdoc/>
        public Task<bool> IsInRoleAsync(UserModel user,
            string normalizedRoleName, CancellationToken ct) {
            if (normalizedRoleName == null) {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }
            return GetFromUserDocumentAsync(user,
                document => document.Roles.Contains(normalizedRoleName), ct);
        }

        /// <inheritdoc/>
        public Task SetAuthenticatorKeyAsync(UserModel user,
            string key, CancellationToken ct) {
            return UpdateUserDocumentAsync(user, document => {
                document.AuthenticatorKey = key;
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task<string> GetAuthenticatorKeyAsync(UserModel user,
            CancellationToken ct) {
            return GetFromUserDocumentAsync(user,
                document => document.AuthenticatorKey, ct);
        }

        /// <inheritdoc/>
        public Task ReplaceCodesAsync(UserModel user,
            IEnumerable<string> recoveryCodes, CancellationToken ct) {
            if (recoveryCodes == null) {
                throw new ArgumentNullException(nameof(recoveryCodes));
            }
            return UpdateUserDocumentAsync(user, document => {
                document.RecoveryCodes = recoveryCodes.ToList();
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task<bool> RedeemCodeAsync(UserModel user,
            string code, CancellationToken ct) {
            return UpdateUserDocumentAsync(user, document => {
                var redeemed = document.RecoveryCodes.Contains(code);
                document.RecoveryCodes.RemoveAll(k => k == code);
                return redeemed;
            }, ct);
        }

        /// <inheritdoc/>
        public Task<int> CountCodesAsync(UserModel user, CancellationToken ct) {
            return GetFromUserDocumentAsync(user,
                document => document.RecoveryCodes?.Count ?? 0, ct);
        }

        /// <inheritdoc/>
        public Task<IList<Claim>> GetClaimsAsync(UserModel user,
            CancellationToken ct) {
            return GetFromUserDocumentAsync<IList<Claim>>(user,
                doc => doc.Claims
                    .Select(c => c.ToClaim())
                    .ToList(),
                ct);
        }

        /// <inheritdoc/>
        public Task AddClaimsAsync(UserModel user, IEnumerable<Claim> claims,
            CancellationToken ct) {
            if (claims == null) {
                throw new ArgumentNullException(nameof(claims));
            }
            return UpdateUserDocumentAsync(user, document => {
                document.Claims.AddRange(claims.Select(c => c.ToDocumentModel()));
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task ReplaceClaimAsync(UserModel user, Claim claim, Claim newClaim,
            CancellationToken ct) {
            if (claim == null) {
                throw new ArgumentNullException(nameof(claim));
            }
            if (newClaim == null) {
                throw new ArgumentNullException(nameof(newClaim));
            }
            return UpdateUserDocumentAsync(user, document => {
                foreach (var item in document.Claims
                    .Where(c => c.Type == claim.Type && c.Value == claim.Value)
                    .ToList()) {
                    document.Claims.Remove(item);
                    document.Claims.Add(newClaim.ToDocumentModel());
                }
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task RemoveClaimsAsync(UserModel user, IEnumerable<Claim> claims,
            CancellationToken ct) {
            if (claims == null) {
                throw new ArgumentNullException(nameof(claims));
            }
            return UpdateUserDocumentAsync(user, document => {
                document.Claims.RemoveAll(c =>
                    claims.Any(o => c.Type == o.Type && c.Value == o.Value));
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<UserModel>> GetUsersForClaimAsync(Claim claim,
            CancellationToken ct) {
            if (claim == null) {
                throw new ArgumentNullException(nameof(claim));
            }
            var results = _documents.CreateQuery<UserDocumentModel>()
                .Where(x => x.Claims != null)
                .Where(x => x.Claims.Contains(claim.ToDocumentModel())) // TODO---
                .GetResults();
            var users = new List<UserModel>();
            if (results.HasMore()) {
                var documents = await results.ReadAsync(ct).ConfigureAwait(false);
                users.AddRange(documents.Select(d => d.Value.ToServiceModel()));
            }
            return users;
        }

        /// <inheritdoc/>
        public Task AddLoginAsync(UserModel user, UserLoginInfo login,
            CancellationToken ct) {
            if (login == null) {
                throw new ArgumentNullException(nameof(login));
            }
            return UpdateUserDocumentAsync(user, document => {
                document.Logins.Add(login.ToDocumentModel());
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task RemoveLoginAsync(UserModel user, string loginProvider,
            string providerKey, CancellationToken ct) {
            return UpdateUserDocumentAsync(user, document => {
                document.Logins.RemoveAll(login =>
                    login.ProviderKey == providerKey &&
                    login.LoginProvider == loginProvider);
                return document;
            }, ct);
        }

        /// <inheritdoc/>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(UserModel user,
            CancellationToken ct) {
            return GetFromUserDocumentAsync<IList<UserLoginInfo>>(user,
                doc => doc.Logins
                    .Select(login => login.ToServiceModel())
                    .ToList(),
                ct);
        }





        /// <inheritdoc/>
        public Task<string> GetNormalizedUserNameAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.NormalizedUserName);
        }

        /// <inheritdoc/>
        public Task<string> GetUserIdAsync(UserModel user, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Id);
        }

        /// <inheritdoc/>
        public Task<string> GetUserNameAsync(UserModel user, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.UserName);
        }

        /// <inheritdoc/>
        public Task SetNormalizedUserNameAsync(UserModel user,
            string normalizedName, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.NormalizedUserName = normalizedName ??
                throw new ArgumentNullException(nameof(normalizedName));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetUserNameAsync(UserModel user,
            string userName, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.UserName = userName ??
                throw new ArgumentNullException(nameof(userName));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetPasswordHashAsync(UserModel user,
            string passwordHash, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = passwordHash ??
                throw new ArgumentNullException(nameof(passwordHash));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<string> GetPasswordHashAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.PasswordHash);
        }

        /// <inheritdoc/>
        public Task<bool> HasPasswordAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.PasswordHash != null);
        }

        /// <inheritdoc/>
        public Task SetSecurityStampAsync(UserModel user,
            string stamp, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<string> GetSecurityStampAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.SecurityStamp);
        }

        /// <inheritdoc/>
        public Task SetTwoFactorEnabledAsync(UserModel user,
            bool enabled, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> GetTwoFactorEnabledAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.TwoFactorEnabled);
        }

        /// <inheritdoc/>
        public Task SetPhoneNumberAsync(UserModel user,
            string phoneNumber, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<string> GetPhoneNumberAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.PhoneNumber);
        }

        /// <inheritdoc/>
        public Task<bool> GetPhoneNumberConfirmedAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        /// <inheritdoc/>
        public Task SetPhoneNumberConfirmedAsync(UserModel user,
            bool confirmed, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetEmailAsync(UserModel user, string email,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.Email = email;
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc/>
        public Task<string> GetEmailAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc/>
        public Task<bool> GetEmailConfirmedAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.EmailConfirmed);
        }

        /// <inheritdoc/>
        public Task SetEmailConfirmedAsync(UserModel user,
            bool confirmed, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc/>
        public Task<string> GetNormalizedEmailAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.NormalizedEmail);
        }

        /// <inheritdoc/>
        public Task SetNormalizedEmailAsync(UserModel user,
            string normalizedEmail, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc/>
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.LockoutEnd);
        }

        /// <inheritdoc/>
        public Task SetLockoutEndDateAsync(UserModel user,
            DateTimeOffset? lockoutEnd, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<int> IncrementAccessFailedCountAsync(
            UserModel user, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc/>
        public Task ResetAccessFailedCountAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<int> GetAccessFailedCountAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc/>
        public Task<bool> GetLockoutEnabledAsync(UserModel user,
            CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <inheritdoc/>
        public Task SetLockoutEnabledAsync(UserModel user,
            bool enabled, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() {
        }

        /// <summary>
        /// Update user document
        /// </summary>
        /// <param name="user"></param>
        /// <param name="update"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<T> UpdateUserDocumentAsync<T>(UserModel user,
            Func<UserDocumentModel, T> update, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            if (update == null) {
                throw new ArgumentNullException(nameof(update));
            }
            while (true) {
                var document = await _documents.FindAsync<UserDocumentModel>(
                    user.Id, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("User was not found");
                }
                if (document.Etag != user.ConcurrencyStamp) {
                    throw new ResourceOutOfDateException("User was out of date");
                }
                try {
                    var newDocument = document.Value.Clone();
                    var result = update(newDocument);
                    document = await _documents.ReplaceAsync(document,
                        newDocument, ct: ct).ConfigureAwait(false);
                    return result;
                }
                catch (ResourceOutOfDateException) {
                    continue; // Replace failed due to etag out of date - retry
                }
            }
        }

        /// <summary>
        /// Get user document
        /// </summary>
        /// <param name="user"></param>
        /// <param name="extract"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<T> GetFromUserDocumentAsync<T>(UserModel user,
            Func<UserDocumentModel, T> extract, CancellationToken ct) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }
            if (extract == null) {
                throw new ArgumentNullException(nameof(extract));
            }
            var document = await _documents.GetAsync<UserDocumentModel>(
                user.Id, ct: ct).ConfigureAwait(false);
            if (document.Etag != user.ConcurrencyStamp) {
                throw new ResourceOutOfDateException("User was out of date");
            }
            return extract(document.Value);
        }

        private readonly IRoleStore<RoleModel> _roleStore;
        private readonly IDocumentCollection _documents;
    }
}
