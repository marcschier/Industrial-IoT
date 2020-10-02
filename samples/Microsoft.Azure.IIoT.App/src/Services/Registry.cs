// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Services {
    using Microsoft.Azure.IIoT.App.Data;
    using Microsoft.Azure.IIoT.App.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.App.Common;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using Microsoft.Azure.IIoT.Exceptions;

    public class Registry {

        /// <summary>
        /// Create registry
        /// </summary>
        /// <param name="registryService"></param>
        /// <param name="logger"></param>
        /// <param name="commonHelper"></param>
        public Registry(IRegistryServiceApi registryService, ILogger logger, UICommon commonHelper) {
            _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonHelper = commonHelper ?? throw new ArgumentNullException(nameof(commonHelper));
        }

        /// <summary>
        /// GetEndpointListAsync
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="previousPage"></param>
        /// <returns>EndpointInfoApiModel</returns>
        public async Task<PagedResult<EndpointInfo>> GetEndpointListAsync(
            string discovererId, string applicationId, string supervisorId, PagedResult<EndpointInfo> previousPage = null, bool getNextPage = false) {

            var pageResult = new PagedResult<EndpointInfo>();

            try {
                var endpoints = new EndpointInfoListApiModel();
                var query = new EndpointInfoQueryApiModel {
                    DiscovererId = discovererId == PathAll ? null : discovererId,
                    ApplicationId = applicationId == PathAll ? null : applicationId,
                    SupervisorId = supervisorId == PathAll ? null : supervisorId,
                    IncludeNotSeenSince = true
                };

                if (getNextPage && string.IsNullOrEmpty(previousPage?.ContinuationToken)) {
                    endpoints = await _registryService.QueryEndpointsAsync(query, _commonHelper.PageLength).ConfigureAwait(false);
                }
                else {
                    endpoints = await _registryService.ListEndpointsAsync(previousPage.ContinuationToken, _commonHelper.PageLength).ConfigureAwait(false);
                }

                foreach (var ep in endpoints.Items) {
                    // Get non cached version of endpoint
                    var endpoint = await _registryService.GetEndpointAsync(ep.Id).ConfigureAwait(false);
                    pageResult.Results.Add(new EndpointInfo {
                        EndpointModel = endpoint
                    });
                }
                if (previousPage != null) {
                    previousPage.Results.AddRange(pageResult.Results);
                    pageResult.Results = previousPage.Results;
                }

                pageResult.ContinuationToken = endpoints.ContinuationToken;
            }
            catch (UnauthorizedAccessException) {
                pageResult.Error = "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception e) {
                var message = "Cannot get endpoint list";
                _logger.Warning(e, message);
                pageResult.Error = message;
            }
            return pageResult;
        }

        /// <summary>
        /// GetDiscovererListAsync
        /// </summary>
        /// <param name="previousPage"></param>
        /// <returns>DiscovererInfo</returns>
        public async Task<PagedResult<DiscovererInfo>> GetDiscovererListAsync(PagedResult<DiscovererInfo> previousPage = null, bool getNextPage = false) {
            var pageResult = new PagedResult<DiscovererInfo>();

            try {
                var discovererModel = new DiscovererQueryApiModel();
                var applicationModel = new ApplicationRegistrationQueryApiModel();
                var discoverers = new DiscovererListApiModel();

                if (!getNextPage || string.IsNullOrEmpty(previousPage?.ContinuationToken)) {
                    discoverers = await _registryService.QueryDiscoverersAsync(discovererModel, _commonHelper.PageLengthSmall).ConfigureAwait(false);
                }
                else {
                    discoverers = await _registryService.ListDiscoverersAsync(previousPage.ContinuationToken, _commonHelper.PageLengthSmall).ConfigureAwait(false);
                }

                if (discoverers != null) {
                    if (discoverers.Items != null && discoverers.Items.Any()) {
                        foreach (var disc in discoverers.Items) {
                            var discoverer = await _registryService.GetDiscovererAsync(disc.Id).ConfigureAwait(false);
                            var info = new DiscovererInfo {
                                DiscovererModel = discoverer,
                                HasApplication = false,
                                ScanStatus = discoverer.Discovery != DiscoveryMode.Off && discoverer.Discovery != null
                            };
                            applicationModel.DiscovererId = discoverer.Id;
                            var applications = await _registryService.QueryApplicationsAsync(applicationModel, 1).ConfigureAwait(false);
                            if (applications != null) {
                                info.HasApplication = true;
                            }
                            pageResult.Results.Add(info);
                        }
                    }
                }

                if (previousPage != null && getNextPage) {
                    previousPage.Results.AddRange(pageResult.Results);
                    pageResult.Results = previousPage.Results;
                }

                pageResult.ContinuationToken = discoverers.ContinuationToken;
            }
            catch (UnauthorizedAccessException) {
                pageResult.Error = "Unauthorized access: Bad User Access Denied.";
            }
            catch (ResourceInvalidStateException) {
                pageResult.Error = "IotHubQuotaExceeded. Send and Receive operations are blocked for this hub until the next UTC day.";
            }
            catch (Exception e) {
                var message = "Cannot get discoverers as list";
                _logger.Warning(e, message);
                pageResult.Error = message;
            }
            return pageResult;
        }

        /// <summary>
        /// GetApplicationListAsync
        /// </summary>
        /// <param name="previousPage"></param>
        /// <returns>ApplicationInfoApiModel</returns>
        public async Task<PagedResult<ApplicationInfoApiModel>> GetApplicationListAsync(PagedResult<ApplicationInfoApiModel> previousPage = null, bool getNextPage = false) {
            var pageResult = new PagedResult<ApplicationInfoApiModel>();

            try {
                var query = new ApplicationRegistrationQueryApiModel {
                    IncludeNotSeenSince = true
                };
                var applications = new ApplicationInfoListApiModel();

                if (getNextPage && string.IsNullOrEmpty(previousPage?.ContinuationToken)) {
                    applications = await _registryService.QueryApplicationsAsync(query, _commonHelper.PageLength).ConfigureAwait(false);
                }
                else
                {
                    applications = await _registryService.ListApplicationsAsync(previousPage.ContinuationToken, _commonHelper.PageLength).ConfigureAwait(false);
                }

                if (applications != null) {
                    foreach (var app in applications.Items) {
                        var application = (await _registryService.GetApplicationAsync(app.ApplicationId).ConfigureAwait(false)).Application;
                        pageResult.Results.Add(application);
                    }
                }

                if (previousPage != null) {
                    previousPage.Results.AddRange(pageResult.Results);
                    pageResult.Results = previousPage.Results;
                    pageResult.ContinuationToken = applications.ContinuationToken;
                }
            }
            catch (UnauthorizedAccessException) {
                pageResult.Error = "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception e) {
                var message = "Can not get applications list";
                _logger.Warning(e, message);
                pageResult.Error = message;
            }
            return pageResult;
        }

        /// <summary>
        /// SetScanAsync
        /// </summary>
        /// <param name="discoverer"></param>
        /// <returns></returns>
        public async Task<string> SetDiscoveryAsync(DiscovererInfo discoverer) {
            try {
                var discoveryMode = DiscoveryMode.Off;
                if (discoverer.DiscovererModel.DiscoveryConfig?.DiscoveryUrls != null && discoverer.ScanStatus) {
                    discoveryMode = DiscoveryMode.Url;
                }
                else {
                    discoveryMode = discoverer.ScanStatus ? DiscoveryMode.Fast : DiscoveryMode.Off;
                }
                await _registryService.SetDiscoveryModeAsync(discoverer.DiscovererModel.Id, discoveryMode, discoverer.Patch).ConfigureAwait(false);
                discoverer.Patch = new DiscoveryConfigApiModel();
            }
            catch (UnauthorizedAccessException) {
                return "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to set discovery mode.");
                var errorMessageTrace = string.Concat(exception.Message,
                    exception.InnerException?.Message ?? "--", exception?.StackTrace ?? "--");
                return errorMessageTrace;
            }
            return null;
        }

        /// <summary>
        /// UpdateDiscovererAsync
        /// </summary>
        /// <param name="discoverer"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<string> UpdateDiscovererAsync(DiscovererInfo discoverer) {
            try {
                await _registryService.UpdateDiscovererAsync(discoverer.DiscovererModel.Id, new DiscovererUpdateApiModel {
                    DiscoveryConfig = discoverer.Patch
                }).ConfigureAwait(false);
                discoverer.Patch = new DiscoveryConfigApiModel();
            }
            catch (UnauthorizedAccessException) {
                return "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to update discoverer");
                var errorMessageTrace = string.Concat(exception.Message,
                    exception.InnerException?.Message ?? "--", exception?.StackTrace ?? "--");
                return errorMessageTrace;
            }
            return null;
        }

        /// <summary>
        /// Discover servers
        /// </summary>
        /// <param name="discoverer"></param>
        /// <returns></returns>
        public async Task<string> DiscoverServersAsync(DiscovererInfo discoverer) {
            try {
                await _registryService.DiscoverAsync(
                    new DiscoveryRequestApiModel {
                        Id = discoverer.DiscoveryRequestId,
                        Discovery = DiscoveryMode.Fast,
                        Configuration = discoverer.Patch
                    }).ConfigureAwait(false);
                discoverer.Patch = new DiscoveryConfigApiModel();
            }
            catch (UnauthorizedAccessException) {
                return "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to discoverer servers.");
                var errorMessageTrace = string.Concat(exception.Message,
                    exception.InnerException?.Message ?? "--", exception?.StackTrace ?? "--");
                return errorMessageTrace;
            }
            return null;
        }

        /// <summary>
        /// GetGatewayListAsync
        /// </summary>
        /// <param name="previousPage"></param>
        /// <returns>GatewayApiModel</returns>
        public async Task<PagedResult<GatewayApiModel>> GetGatewayListAsync(PagedResult<GatewayApiModel> previousPage = null, bool getNextPage = false) {
            var pageResult = new PagedResult<GatewayApiModel>();

            try {
                var gatewayModel = new GatewayQueryApiModel();
                var gateways = new GatewayListApiModel();

                if (getNextPage && string.IsNullOrEmpty(previousPage?.ContinuationToken)) {
                    gateways = await _registryService.QueryGatewaysAsync(gatewayModel, _commonHelper.PageLength).ConfigureAwait(false);
                }
                else {
                    gateways = await _registryService.ListGatewaysAsync(previousPage.ContinuationToken, _commonHelper.PageLength).ConfigureAwait(false);
                }

                if (gateways != null) {
                    foreach (var gw in gateways.Items) {
                        var gateway = (await _registryService.GetGatewayAsync(gw.Id).ConfigureAwait(false)).Gateway;
                        pageResult.Results.Add(gateway);
                    }
                }

                if (previousPage != null) {
                    previousPage.Results.AddRange(pageResult.Results);
                    pageResult.Results = previousPage.Results;
                    pageResult.ContinuationToken = gateways.ContinuationToken;
                }
            }
            catch (UnauthorizedAccessException) {
                pageResult.Error = "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception e) {
                var message = "Cannot get gateways list";
                _logger.Warning(e, message);
                pageResult.Error = message;
            }
            return pageResult;
        }

        /// <summary>
        /// GetPublisherListAsync
        /// </summary>
        /// <param name="previousPage"></param>
        /// <returns>PublisherApiModel</returns>
        public async Task<PagedResult<PublisherApiModel>> GetPublisherListAsync(PagedResult<PublisherApiModel> previousPage = null, bool getNextPage = false) {
            var pageResult = new PagedResult<PublisherApiModel>();

            try {
                var publisherModel = new PublisherQueryApiModel();
                var publishers = new PublisherListApiModel();

                if (getNextPage && string.IsNullOrEmpty(previousPage?.ContinuationToken)) {
                    publishers = await _registryService.QueryPublishersAsync(publisherModel, _commonHelper.PageLengthSmall).ConfigureAwait(false);
                }
                else {
                    publishers = await _registryService.ListPublishersAsync(previousPage.ContinuationToken, _commonHelper.PageLengthSmall).ConfigureAwait(false);
                }

                if (publishers != null) {
                    foreach (var pub in publishers.Items) {
                        var publisher = await _registryService.GetPublisherAsync(pub.Id).ConfigureAwait(false);
                        pageResult.Results.Add(publisher);
                    }
                }
                if (previousPage != null) {
                    previousPage.Results.AddRange(pageResult.Results);
                    pageResult.Results = previousPage.Results;
                    pageResult.ContinuationToken = publishers.ContinuationToken;
                }
            }
            catch (UnauthorizedAccessException) {
                pageResult.Error = "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception e) {
                var message = "Cannot get publisher list";
                _logger.Warning(e, message);
                pageResult.Error = message;
            }
            return pageResult;
        }

        /// <summary>
        /// Update publisher
        /// </summary>
        /// <param name="discoverer"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<string> UpdatePublisherAsync(PublisherInfo publisher) {
            try {
                await _registryService.UpdatePublisherAsync(publisher.PublisherModel.Id, new PublisherUpdateApiModel {
                    LogLevel = publisher.PublisherModel.LogLevel
                }).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) {
                return "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to update publisher");
                var errorMessageTrace = string.Concat(exception.Message,
                    exception.InnerException?.Message ?? "--", exception?.StackTrace ?? "--");
                return errorMessageTrace;
            }
            return null;
        }

        /// <summary>
        /// Unregister application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public async Task<string> UnregisterApplicationAsync(string applicationId) {

            try {
                var application = await _registryService.GetApplicationAsync(applicationId).ConfigureAwait(false);
                await _registryService.UnregisterApplicationAsync(applicationId,
                    application.Application.GenerationId).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) {
                return "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to unregister application");
                var errorMessageTrace = string.Concat(exception.Message, exception.InnerException?.Message ?? "--", exception?.StackTrace ?? "--");
                return errorMessageTrace;
            }
            return null;
        }

        /// <summary>
        /// GetSupervisorListAsync
        /// </summary>
        /// <param name="previousPage"></param>
        /// <returns>SupervisorApiModel</returns>
        public async Task<PagedResult<SupervisorApiModel>> GetSupervisorListAsync(PagedResult<SupervisorApiModel> previousPage = null, bool getNextPage = false) {

            var pageResult = new PagedResult<SupervisorApiModel>();

            try {
                var model = new SupervisorQueryApiModel();
                var supervisors = new SupervisorListApiModel();

                if (getNextPage && string.IsNullOrEmpty(previousPage?.ContinuationToken)) {
                    supervisors = await _registryService.QuerySupervisorsAsync(model, _commonHelper.PageLength).ConfigureAwait(false);
                }
                else {
                    supervisors = await _registryService.ListSupervisorsAsync(previousPage.ContinuationToken, _commonHelper.PageLengthSmall).ConfigureAwait(false);
                }

                if (supervisors != null) {
                    foreach (var sup in supervisors.Items) {
                        var supervisor = await _registryService.GetSupervisorAsync(sup.Id).ConfigureAwait(false);
                        pageResult.Results.Add(supervisor);
                    }
                }
                if (previousPage != null) {
                    previousPage.Results.AddRange(pageResult.Results);
                    pageResult.Results = previousPage.Results;
                    pageResult.ContinuationToken = supervisors.ContinuationToken;
                }
            }
            catch (UnauthorizedAccessException) {
                pageResult.Error = "Unauthorized access: Bad User Access Denied.";
            }
            catch (Exception e) {
                var message = "Cannot get supervisor list";
                _logger.Warning(e, message);
                pageResult.Error = message;
            }
            return pageResult;
        }

        /// <summary>
        /// GetSupervisorStatusAsync
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns>SupervisorStatusApiModel</returns>
        public async Task<SupervisorStatusApiModel> GetSupervisorStatusAsync(string supervisorId) {
            var supervisorStatus = new SupervisorStatusApiModel();

            try {
                supervisorStatus = await _registryService.GetSupervisorStatusAsync(supervisorId).ConfigureAwait(false);
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to get status");
            }

            return supervisorStatus;
        }

        /// <summary>
        /// ResetSupervisorAsync
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <returns>bool</returns>
        public async Task<string> ResetSupervisorAsync(string supervisorId) {
            _ = new SupervisorStatusApiModel();

            try {
                await _registryService.ResetSupervisorAsync(supervisorId).ConfigureAwait(false);
                return string.Empty;
            }
            catch (Exception exception) {
                _logger.Error(exception, "Failed to reset supervisor");
                return exception.Message;
            }
        }

        private readonly IRegistryServiceApi _registryService;
        private readonly ILogger _logger;
        private readonly UICommon _commonHelper;
        public string PathAll = "All";
    }
}
