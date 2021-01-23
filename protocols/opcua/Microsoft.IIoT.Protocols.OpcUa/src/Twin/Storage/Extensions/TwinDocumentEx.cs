// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Storage.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;

    /// <summary>
    /// Twin document extensions
    /// </summary>
    public static class TwinDocumentEx {

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="document"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static TwinInfoModel ToServiceModel(this TwinDocument document,
            string etag) {
            if (document == null) {
                return null;
            }
            return new TwinInfoModel {
                GenerationId = etag,
                Id = document.Id,
                EndpointId = document.EndpointId,
                Updated = ToOperationModel(document.UpdateAuthorityId, document.UpdateTime),
                Created = ToOperationModel(document.CreateAuthorityId, document.CreateTime),
                Diagnostics = ToDiagnosticsModel(document),
                OperationTimeout = document.OperationTimeout,
                ConnectionState = ToConnectionStateModel(document),
                User = ToCredentialModel(document)
            };
        }

        /// <summary>
        /// Convert into document object
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static TwinDocument ToDocumentModel(this TwinInfoModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            return new TwinDocument {
                Id = model.Id,
                EndpointId = model.EndpointId,
                CreateAuthorityId = model.Created?.AuthorityId,
                CreateTime = model.Created?.Time ?? DateTime.UtcNow,
                UpdateAuthorityId = model.Updated?.AuthorityId,
                UpdateTime = model.Updated?.Time ?? DateTime.UtcNow,
                DiagnosticsLevel = model.Diagnostics?.Level,
                DiagnosticsAuditId = model.Diagnostics?.AuditId,
                OperationTimeout = model.OperationTimeout,
                ConnectionState = model.ConnectionState?.State ?? ConnectionStatus.Disconnected,
                LastResultDiagnostics = model.ConnectionState?.LastResult?.Diagnostics,
                LastResultErrorMessage = model.ConnectionState?.LastResult?.ErrorMessage,
                LastResultStatusCode = model.ConnectionState?.LastResult?.StatusCode,
                LastResultChange = model.ConnectionState?.LastResultChange,
                CredentialType = model.User?.Type ?? CredentialType.None,
                Credential = model.User.Value
            };
        }

        /// <summary>
        /// Create twin state model
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static ConnectionStateModel ToConnectionStateModel(TwinDocument document) {
            if (document == null) {
                return null;
            }
            var lastConnectionResult = ToServiceResultModel(document.LastResultDiagnostics,
                document.LastResultStatusCode, document.LastResultErrorMessage);
            if (lastConnectionResult == null &&
                document.LastResultChange == null &&
                (document.ConnectionState == ConnectionStatus.Disconnected)) {
                return null;
            }
            return new ConnectionStateModel {
                State = document.ConnectionState,
                LastResult = lastConnectionResult,
                LastResultChange = document.LastResultChange
            };
        }

        /// <summary>
        /// Convert to credential model
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static CredentialModel ToCredentialModel(TwinDocument document) {
            if (document == null) {
                return null;
            }
            if (document.CredentialType == CredentialType.None) {
                return null;
            }
            return new CredentialModel {
                Type = document.CredentialType,
                Value = document.Credential?.Copy()
            };
        }

        /// <summary>
        /// Convert to diagnostic model
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static DiagnosticsModel ToDiagnosticsModel(TwinDocument document) {
            if (document == null) {
                return null;
            }
            if (string.IsNullOrEmpty(document.DiagnosticsAuditId) &&
                document.DiagnosticsLevel == null) {
                return null;
            }
            return new DiagnosticsModel {
                AuditId = document.DiagnosticsAuditId,
                Level = document.DiagnosticsLevel
            };
        }

        /// <summary>
        /// Create service result model
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private static ServiceResultModel ToServiceResultModel(
            VariantValue diagnostics, uint? statusCode, string errorMessage) {
            if (diagnostics.IsNull() &&
                statusCode == null &&
                string.IsNullOrEmpty(errorMessage)) {
                return null;
            }
            return new ServiceResultModel {
                ErrorMessage = errorMessage,
                Diagnostics = diagnostics,
                StatusCode = statusCode,
            };
        }

        /// <summary>
        /// Create operation model
        /// </summary>
        /// <param name="authorityId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static OperationContextModel ToOperationModel(
            string authorityId, DateTime? time) {
            if (string.IsNullOrEmpty(authorityId) && time == null) {
                return null;
            }
            return new OperationContextModel {
                AuthorityId = authorityId,
                Time = time ?? DateTime.MinValue
            };
        }
    }
}
