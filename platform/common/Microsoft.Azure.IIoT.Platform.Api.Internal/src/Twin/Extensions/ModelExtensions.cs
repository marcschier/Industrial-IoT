// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System.Linq;
    using System;

    /// <summary>
    /// Model conversion extensions
    /// </summary>
    public static class ModelExtensions {

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static AttributeReadRequestApiModel ToApiModel(
            this AttributeReadRequestModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeReadRequestApiModel {
                NodeId = model.NodeId,
                Attribute = (Core.Api.Models.NodeAttribute)model.Attribute
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static AttributeReadRequestModel ToServiceModel(
            this AttributeReadRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeReadRequestModel {
                NodeId = model.NodeId,
                Attribute = (Core.Models.NodeAttribute)model.Attribute
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static AttributeReadResponseApiModel ToApiModel(
            this AttributeReadResultModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeReadResponseApiModel {
                Value = model.Value,
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static AttributeReadResultModel ToServiceModel(
            this AttributeReadResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeReadResultModel {
                Value = model.Value,
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static AttributeWriteRequestApiModel ToApiModel(
            this AttributeWriteRequestModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeWriteRequestApiModel {
                NodeId = model.NodeId,
                Value = model.Value,
                Attribute = (Core.Api.Models.NodeAttribute)model.Attribute
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static AttributeWriteRequestModel ToServiceModel(
            this AttributeWriteRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeWriteRequestModel {
                NodeId = model.NodeId,
                Value = model.Value,
                Attribute = (Core.Models.NodeAttribute)model.Attribute
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static AttributeWriteResponseApiModel ToApiModel(
            this AttributeWriteResultModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeWriteResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static AttributeWriteResultModel ToServiceModel(
            this AttributeWriteResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new AttributeWriteResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowseNextRequestApiModel ToApiModel(
            this BrowseNextRequestModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseNextRequestApiModel {
                Abort = model.Abort,
                TargetNodesOnly = model.TargetNodesOnly,
                NodeIdsOnly = model.NodeIdsOnly,
                ReadVariableValues = model.ReadVariableValues,
                ContinuationToken = model.ContinuationToken,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static BrowseNextRequestModel ToServiceModel(
            this BrowseNextRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseNextRequestModel {
                Abort = model.Abort,
                TargetNodesOnly = model.TargetNodesOnly,
                NodeIdsOnly = model.NodeIdsOnly,
                ReadVariableValues = model.ReadVariableValues,
                ContinuationToken = model.ContinuationToken,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowseNextResponseApiModel ToApiModel(
            this BrowseNextResultModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseNextResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel(),
                ContinuationToken = model.ContinuationToken,
                References = model.References?
                    .Select(r => r.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowseNextResultModel ToServiceModel(
            this BrowseNextResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseNextResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel(),
                ContinuationToken = model.ContinuationToken,
                References = model.References?
                    .Select(r => r.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowsePathRequestApiModel ToApiModel(
            this BrowsePathRequestModel model) {
            if (model == null) {
                return null;
            }
            return new BrowsePathRequestApiModel {
                NodeId = model.NodeId,
                BrowsePaths = model.BrowsePaths,
                NodeIdsOnly = model.NodeIdsOnly,
                ReadVariableValues = model.ReadVariableValues,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static BrowsePathRequestModel ToServiceModel(
            this BrowsePathRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowsePathRequestModel {
                NodeId = model.NodeId,
                BrowsePaths = model.BrowsePaths,
                NodeIdsOnly = model.NodeIdsOnly,
                ReadVariableValues = model.ReadVariableValues,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowsePathResponseApiModel ToApiModel(
            this BrowsePathResultModel model) {
            if (model == null) {
                return null;
            }
            return new BrowsePathResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel(),
                Targets = model.Targets?
                    .Select(r => r.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowsePathResultModel ToServiceModel(
            this BrowsePathResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowsePathResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel(),
                Targets = model.Targets?
                    .Select(r => r.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowseRequestApiModel ToApiModel(
            this BrowseRequestModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseRequestApiModel {
                NodeId = model.NodeId,
                MaxReferencesToReturn = model.MaxReferencesToReturn,
                Direction = (Core.Api.Models.BrowseDirection?)model.Direction,
                View = model.View.ToApiModel(),
                ReferenceTypeId = model.ReferenceTypeId,
                TargetNodesOnly = model.TargetNodesOnly,
                NodeIdsOnly = model.NodeIdsOnly,
                ReadVariableValues = model.ReadVariableValues,
                NodeClassFilter = model.NodeClassFilter?
                    .Select(f => (Core.Api.Models.NodeClass)f)
                    .ToList(),
                NoSubtypes = model.NoSubtypes,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static BrowseRequestModel ToServiceModel(
            this BrowseRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseRequestModel {
                NodeId = model.NodeId,
                MaxReferencesToReturn = model.MaxReferencesToReturn,
                Direction = (Core.Models.BrowseDirection?)model.Direction,
                View = model.View.ToServiceModel(),
                ReferenceTypeId = model.ReferenceTypeId,
                TargetNodesOnly = model.TargetNodesOnly,
                NodeIdsOnly = model.NodeIdsOnly,
                ReadVariableValues = model.ReadVariableValues,
                NodeClassFilter = model.NodeClassFilter?
                    .Select(f => (Core.Models.NodeClass)f)
                    .ToList(),
                NoSubtypes = model.NoSubtypes,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowseResponseApiModel ToApiModel(
            this BrowseResultModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseResponseApiModel {
                Node = model.Node.ToApiModel(),
                ErrorInfo = model.ErrorInfo.ToApiModel(),
                ContinuationToken = model.ContinuationToken,
                References = model.References?
                    .Select(r => r.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static BrowseResultModel ToServiceModel(
            this BrowseResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseResultModel {
                Node = model.Node.ToServiceModel(),
                ErrorInfo = model.ErrorInfo.ToServiceModel(),
                ContinuationToken = model.ContinuationToken,
                References = model.References?
                    .Select(r => r.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static BrowseViewModel ToServiceModel(
            this BrowseViewApiModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseViewModel {
                ViewId = model.ViewId,
                Version = model.Version,
                Timestamp = model.Timestamp
            };
        }

        /// <summary>
        /// Convert back to api model
        /// </summary>
        /// <returns></returns>
        public static BrowseViewApiModel ToApiModel(
            this BrowseViewModel model) {
            if (model == null) {
                return null;
            }
            return new BrowseViewApiModel {
                ViewId = model.ViewId,
                Version = model.Version,
                Timestamp = model.Timestamp
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static CredentialApiModel ToApiModel(
            this CredentialModel model) {
            if (model == null) {
                return null;
            }
            return new CredentialApiModel {
                Value = model.Value,
                Type = (Core.Api.Models.CredentialType?)model.Type
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        public static CredentialModel ToServiceModel(
            this CredentialApiModel model) {
            if (model == null) {
                return null;
            }
            return new CredentialModel {
                Value = model.Value,
                Type = (Core.Models.CredentialType?)model.Type
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static DiagnosticsApiModel ToApiModel(
            this DiagnosticsModel model) {
            if (model == null) {
                return null;
            }
            return new DiagnosticsApiModel {
                AuditId = model.AuditId,
                Level = (Core.Api.Models.DiagnosticsLevel?)model.Level,
                TimeStamp = model.TimeStamp
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        public static DiagnosticsModel ToServiceModel(
            this DiagnosticsApiModel model) {
            if (model == null) {
                return null;
            }
            return new DiagnosticsModel {
                AuditId = model.AuditId,
                Level = (Core.Models.DiagnosticsLevel?)model.Level,
                TimeStamp = model.TimeStamp
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodCallArgumentApiModel ToApiModel(
            this MethodCallArgumentModel model) {
            if (model == null) {
                return null;
            }
            return new MethodCallArgumentApiModel {
                Value = model.Value,
                DataType = model.DataType
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static MethodCallArgumentModel ToServiceModel(
            this MethodCallArgumentApiModel model) {
            if (model == null) {
                return null;
            }
            return new MethodCallArgumentModel {
                Value = model.Value,
                DataType = model.DataType
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodCallRequestApiModel ToApiModel(
            this MethodCallRequestModel model) {
            if (model == null) {
                return null;
            }
            return new MethodCallRequestApiModel {
                MethodId = model.MethodId,
                ObjectId = model.ObjectId,
                MethodBrowsePath = model.MethodBrowsePath,
                ObjectBrowsePath = model.ObjectBrowsePath,
                Arguments = model.Arguments?
                    .Select(s => s.ToApiModel()).ToList(),
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static MethodCallRequestModel ToServiceModel(
            this MethodCallRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new MethodCallRequestModel {
                MethodId = model.MethodId,
                ObjectId = model.ObjectId,
                MethodBrowsePath = model.MethodBrowsePath,
                ObjectBrowsePath = model.ObjectBrowsePath,
                Arguments = model.Arguments?
                    .Select(s => s.ToServiceModel()).ToList(),
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodCallResponseApiModel ToApiModel(
            this MethodCallResultModel model) {
            if (model == null) {
                return null;
            }
            return new MethodCallResponseApiModel {
                Results = model.Results?
                    .Select(a => a.ToApiModel())
                    .ToList(),
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodCallResultModel ToServiceModel(
            this MethodCallResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new MethodCallResultModel {
                Results = model.Results?
                    .Select(a => a.ToServiceModel())
                    .ToList(),
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodMetadataArgumentApiModel ToApiModel(
            this MethodMetadataArgumentModel model) {
            if (model == null) {
                return null;
            }
            return new MethodMetadataArgumentApiModel {
                DefaultValue = model.DefaultValue,
                Type = model.Type.ToApiModel(),
                ValueRank = (Core.Api.Models.NodeValueRank?)model.ValueRank,
                ArrayDimensions = model.ArrayDimensions,
                Description = model.Description,
                Name = model.Name
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static MethodMetadataArgumentModel ToServiceModel(
            this MethodMetadataArgumentApiModel model) {
            if (model == null) {
                return null;
            }
            return new MethodMetadataArgumentModel {
                DefaultValue = model.DefaultValue,
                Type = model.Type.ToServiceModel(),
                ValueRank = (Core.Models.NodeValueRank?)model.ValueRank,
                ArrayDimensions = model.ArrayDimensions,
                Description = model.Description,
                Name = model.Name
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodMetadataRequestApiModel ToApiModel(
            this MethodMetadataRequestModel model) {
            if (model == null) {
                return null;
            }
            return new MethodMetadataRequestApiModel {
                MethodId = model.MethodId,
                MethodBrowsePath = model.MethodBrowsePath,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static MethodMetadataRequestModel ToServiceModel(
            this MethodMetadataRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new MethodMetadataRequestModel {
                MethodId = model.MethodId,
                MethodBrowsePath = model.MethodBrowsePath,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodMetadataResponseApiModel ToApiModel(
            this MethodMetadataResultModel model) {
            if (model == null) {
                return null;
            }
            return new MethodMetadataResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel(),
                ObjectId = model.ObjectId,
                InputArguments = model.InputArguments?
                    .Select(a => a.ToApiModel())
                    .ToList(),
                OutputArguments = model.OutputArguments?
                    .Select(a => a.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static MethodMetadataResultModel ToServiceModel(
            this MethodMetadataResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new MethodMetadataResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel(),
                ObjectId = model.ObjectId,
                InputArguments = model.InputArguments?
                    .Select(a => a.ToServiceModel())
                    .ToList(),
                OutputArguments = model.OutputArguments?
                    .Select(a => a.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create node api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static NodeApiModel ToApiModel(
            this NodeModel model) {
            if (model == null) {
                return null;
            }
            return new NodeApiModel {
                NodeId = model.NodeId,
                Children = model.Children,
                BrowseName = model.BrowseName,
                DisplayName = model.DisplayName,
                Description = model.Description,
                NodeClass = (Core.Api.Models.NodeClass?)model.NodeClass,
                IsAbstract = model.IsAbstract,
                AccessLevel = (Core.Api.Models.NodeAccessLevel?)model.AccessLevel,
                EventNotifier = (Core.Api.Models.NodeEventNotifier?)model.EventNotifier,
                Executable = model.Executable,
                DataType = model.DataType,
                ValueRank = (Core.Api.Models.NodeValueRank?)model.ValueRank,
                AccessRestrictions = (Core.Api.Models.NodeAccessRestrictions?)model.AccessRestrictions,
                ArrayDimensions = model.ArrayDimensions,
                ContainsNoLoops = model.ContainsNoLoops,
                DataTypeDefinition = model.DataTypeDefinition,
                Value = model.Value,
                Historizing = model.Historizing,
                ErrorInfo = model.ErrorInfo.ToApiModel(),
                ServerPicoseconds = model.ServerPicoseconds,
                SourcePicoseconds = model.SourcePicoseconds,
                SourceTimestamp = model.SourceTimestamp,
                ServerTimestamp = model.ServerTimestamp,
                InverseName = model.InverseName,
                MinimumSamplingInterval = model.MinimumSamplingInterval,
                Symmetric = model.Symmetric,
                UserAccessLevel = (Core.Api.Models.NodeAccessLevel?)model.UserAccessLevel,
                UserExecutable = model.UserExecutable,
                UserWriteMask = model.UserWriteMask,
                WriteMask = model.WriteMask,
                RolePermissions = model.RolePermissions?
                    .Select(p => p.ToApiModel())
                    .ToList(),
                UserRolePermissions = model.UserRolePermissions?
                    .Select(p => p.ToApiModel())
                    .ToList(),
                TypeDefinitionId = model.TypeDefinitionId
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static NodeModel ToServiceModel(
            this NodeApiModel model) {
            if (model == null) {
                return null;
            }
            return new NodeModel {
                NodeId = model.NodeId,
                Children = model.Children,
                BrowseName = model.BrowseName,
                DisplayName = model.DisplayName,
                Description = model.Description,
                NodeClass = (Core.Models.NodeClass?)model.NodeClass,
                IsAbstract = model.IsAbstract,
                AccessLevel = (Core.Models.NodeAccessLevel?)model.AccessLevel,
                EventNotifier = (Core.Models.NodeEventNotifier?)model.EventNotifier,
                Executable = model.Executable,
                DataType = model.DataType,
                ValueRank = (Core.Models.NodeValueRank?)model.ValueRank,
                AccessRestrictions = (Core.Models.NodeAccessRestrictions?)model.AccessRestrictions,
                ArrayDimensions = model.ArrayDimensions,
                ContainsNoLoops = model.ContainsNoLoops,
                DataTypeDefinition = model.DataTypeDefinition,
                Value = model.Value,
                Historizing = model.Historizing,
                InverseName = model.InverseName,
                ErrorInfo = model.ErrorInfo.ToServiceModel(),
                ServerPicoseconds = model.ServerPicoseconds,
                SourcePicoseconds = model.SourcePicoseconds,
                SourceTimestamp = model.SourceTimestamp,
                ServerTimestamp = model.ServerTimestamp,
                MinimumSamplingInterval = model.MinimumSamplingInterval,
                Symmetric = model.Symmetric,
                UserAccessLevel = (Core.Models.NodeAccessLevel?)model.UserAccessLevel,
                UserExecutable = model.UserExecutable,
                UserWriteMask = model.UserWriteMask,
                WriteMask = model.WriteMask,
                RolePermissions = model.RolePermissions?
                    .Select(p => p.ToServiceModel())
                    .ToList(),
                UserRolePermissions = model.UserRolePermissions?
                    .Select(p => p.ToServiceModel())
                    .ToList(),
                TypeDefinitionId = model.TypeDefinitionId
            };
        }

        /// <summary>
        /// Create reference api model
        /// </summary>
        /// <param name="model"></param>
        public static NodePathTargetApiModel ToApiModel(
            this NodePathTargetModel model) {
            if (model == null) {
                return null;
            }
            return new NodePathTargetApiModel {
                BrowsePath = model.BrowsePath,
                RemainingPathIndex = model.RemainingPathIndex,
                Target = model.Target.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static NodePathTargetModel ToServiceModel(
            this NodePathTargetApiModel model) {
            if (model == null) {
                return null;
            }
            return new NodePathTargetModel {
                BrowsePath = model.BrowsePath,
                RemainingPathIndex = model.RemainingPathIndex,
                Target = model.Target.ToServiceModel()
            };
        }

        /// <summary>
        /// Create reference api model
        /// </summary>
        /// <param name="model"></param>
        public static NodeReferenceApiModel ToApiModel(
            this NodeReferenceModel model) {
            if (model == null) {
                return null;
            }
            return new NodeReferenceApiModel {
                ReferenceTypeId = model.ReferenceTypeId,
                Direction = (Core.Api.Models.BrowseDirection?)model.Direction,
                Target = model.Target.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static NodeReferenceModel ToServiceModel(
            this NodeReferenceApiModel model) {
            if (model == null) {
                return null;
            }
            return new NodeReferenceModel {
                ReferenceTypeId = model.ReferenceTypeId,
                Direction = (Core.Models.BrowseDirection?)model.Direction,
                Target = model.Target.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ReadRequestApiModel ToApiModel(
            this ReadRequestModel model) {
            if (model == null) {
                return null;
            }
            return new ReadRequestApiModel {
                Attributes = model.Attributes?
                    .Select(a => a.ToApiModel())
                    .ToList(),
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static ReadRequestModel ToServiceModel(
            this ReadRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadRequestModel {
                Attributes = model.Attributes?
                    .Select(a => a.ToServiceModel())
                    .ToList(),
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ReadResponseApiModel ToApiModel(
            this ReadResultModel model) {
            if (model == null) {
                return null;
            }
            return new ReadResponseApiModel {
                Results = model.Results?
                    .Select(a => a.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <param name="model"></param>
        public static ReadResultModel ToServiceModel(
            this ReadResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadResultModel {
                Results = model.Results?
                    .Select(a => a.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static RequestHeaderApiModel ToApiModel(
            this RequestHeaderModel model) {
            if (model == null) {
                return null;
            }
            return new RequestHeaderApiModel {
                Diagnostics = model.Diagnostics.ToApiModel(),
                Elevation = model.Elevation.ToApiModel(),
                Locales = model.Locales?.ToList()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static RequestHeaderModel ToServiceModel(
            this RequestHeaderApiModel model) {
            if (model == null) {
                return null;
            }
            return new RequestHeaderModel {
                Diagnostics = model.Diagnostics.ToServiceModel(),
                Elevation = model.Elevation.ToServiceModel(),
                Locales = model.Locales?.ToList()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static RolePermissionApiModel ToApiModel(
            this RolePermissionModel model) {
            if (model == null) {
                return null;
            }
            return new RolePermissionApiModel {
                RoleId = model.RoleId,
                Permissions = (Core.Api.Models.RolePermissions?)model.Permissions
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static RolePermissionModel ToServiceModel(
            this RolePermissionApiModel model) {
            if (model == null) {
                return null;
            }
            return new RolePermissionModel {
                RoleId = model.RoleId,
                Permissions = (Core.Models.RolePermissions?)model.Permissions
            };
        }

        /// <summary>
        /// Create node api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static ServiceResultApiModel ToApiModel(
            this ServiceResultModel model) {
            if (model == null) {
                return null;
            }
            return new ServiceResultApiModel {
                Diagnostics = model.Diagnostics,
                StatusCode = model.StatusCode,
                ErrorMessage = model.ErrorMessage
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static ServiceResultModel ToServiceModel(
            this ServiceResultApiModel model) {
            if (model == null) {
                return null;
            }
            return new ServiceResultModel {
                Diagnostics = model.Diagnostics,
                StatusCode = model.StatusCode,
                ErrorMessage = model.ErrorMessage
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ValueReadRequestApiModel ToApiModel(
            this ValueReadRequestModel model) {
            if (model == null) {
                return null;
            }
            return new ValueReadRequestApiModel {
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static ValueReadRequestModel ToServiceModel(
            this ValueReadRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new ValueReadRequestModel {
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ValueReadResponseApiModel ToApiModel(
            this ValueReadResultModel model) {
            if (model == null) {
                return null;
            }
            return new ValueReadResponseApiModel {
                Value = model.Value,
                DataType = model.DataType,
                SourcePicoseconds = model.SourcePicoseconds,
                SourceTimestamp = model.SourceTimestamp,
                ServerPicoseconds = model.ServerPicoseconds,
                ServerTimestamp = model.ServerTimestamp,
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ValueReadResultModel ToServiceModel(
            this ValueReadResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new ValueReadResultModel {
                Value = model.Value,
                DataType = model.DataType,
                SourcePicoseconds = model.SourcePicoseconds,
                SourceTimestamp = model.SourceTimestamp,
                ServerPicoseconds = model.ServerPicoseconds,
                ServerTimestamp = model.ServerTimestamp,
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ValueWriteRequestApiModel ToApiModel(
            this ValueWriteRequestModel model) {
            if (model == null) {
                return null;
            }
            return new ValueWriteRequestApiModel {
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                DataType = model.DataType,
                IndexRange = model.IndexRange,
                Value = model.Value,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static ValueWriteRequestModel ToServiceModel(
            this ValueWriteRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new ValueWriteRequestModel {
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                DataType = model.DataType,
                IndexRange = model.IndexRange,
                Value = model.Value,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ValueWriteResponseApiModel ToApiModel(
            this ValueWriteResultModel model) {
            if (model == null) {
                return null;
            }
            return new ValueWriteResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ValueWriteResultModel ToServiceModel(
            this ValueWriteResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new ValueWriteResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static WriteRequestApiModel ToApiModel(
            this WriteRequestModel model) {
            if (model == null) {
                return null;
            }
            return new WriteRequestApiModel {
                Attributes = model.Attributes?
                    .Select(a => a.ToApiModel())
                    .ToList(),
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static WriteRequestModel ToServiceModel(
            this WriteRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new WriteRequestModel {
                Attributes = model.Attributes?
                    .Select(a => a.ToServiceModel())
                    .ToList(),
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static WriteResponseApiModel ToApiModel(
            this WriteResultModel model) {
            if (model == null) {
                return null;
            }
            return new WriteResponseApiModel {
                Results = model.Results?
                    .Select(a => a.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static WriteResultModel ToServiceModel(
            this WriteResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new WriteResultModel {
                Results = model.Results?
                    .Select(a => a.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static ModelUploadStartRequestApiModel ToApiModel(
            this ModelUploadStartRequestModel model) {
            if (model == null) {
                return null;
            }
            return new ModelUploadStartRequestApiModel {
                ContentMimeType = model.ContentMimeType,
                UploadEndpointUrl = model.UploadEndpointUrl,
                AuthorizationHeader = model.AuthorizationHeader,
                Diagnostics = model.Diagnostics.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ModelUploadStartRequestModel ToServiceModel(
            this ModelUploadStartRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new ModelUploadStartRequestModel {
                ContentMimeType = model.ContentMimeType,
                UploadEndpointUrl = model.UploadEndpointUrl,
                AuthorizationHeader = model.AuthorizationHeader,
                Diagnostics = model.Diagnostics.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static ModelUploadStartResponseApiModel ToApiModel(
            this ModelUploadStartResultModel model) {
            if (model == null) {
                return null;
            }
            return new ModelUploadStartResponseApiModel {
                FileName = model.FileName,
                ContentMimeType = model.ContentMimeType,
                TimeStamp = model.TimeStamp
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ModelUploadStartResultModel ToServiceModel(
            this ModelUploadStartResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new ModelUploadStartResultModel {
                FileName = model.FileName,
                ContentMimeType = model.ContentMimeType,
                TimeStamp = model.TimeStamp
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static PublishedItemApiModel ToApiModel(
            this PublishedItemModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedItemApiModel {
                NodeId = model.NodeId,
                DisplayName = model.DisplayName,
                HeartbeatInterval = model.HeartbeatInterval,
                SamplingInterval = model.SamplingInterval,
                PublishingInterval = model.PublishingInterval
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static PublishedItemModel ToServiceModel(
            this PublishedItemApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedItemModel {
                NodeId = model.NodeId,
                DisplayName = model.DisplayName,
                HeartbeatInterval = model.HeartbeatInterval,
                SamplingInterval = model.SamplingInterval,
                PublishingInterval = model.PublishingInterval
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        public static PublishedItemListRequestApiModel ToApiModel(
            this PublishedItemListRequestModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedItemListRequestApiModel {
                ContinuationToken = model.ContinuationToken
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static PublishedItemListRequestModel ToServiceModel(
            this PublishedItemListRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedItemListRequestModel {
                ContinuationToken = model.ContinuationToken
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static PublishedItemListResponseApiModel ToApiModel(
            this PublishedItemListResultModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedItemListResponseApiModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(n => n.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        /// <param name="model"></param>
        public static PublishedItemListResultModel ToServiceModel(
            this PublishedItemListResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedItemListResultModel {
                ContinuationToken = model.ContinuationToken,
                Items = model.Items?
                    .Select(n => n.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        public static PublishStartRequestApiModel ToApiModel(
            this PublishStartRequestModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStartRequestApiModel {
                Item = model.Item?.ToApiModel(),
                Header = model.Header?.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static PublishStartRequestModel ToServiceModel(
            this PublishStartRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStartRequestModel {
                Item = model.Item?.ToServiceModel(),
                Header = model.Header?.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static PublishStartResponseApiModel ToApiModel(
            this PublishStartResultModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStartResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        /// <param name="model"></param>
        public static PublishStartResultModel ToServiceModel(
            this PublishStartResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStartResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        public static PublishStopRequestApiModel ToApiModel(
            this PublishStopRequestModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStopRequestApiModel {
                NodeId = model.NodeId,
                Header = model.Header?.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static PublishStopRequestModel ToServiceModel(
            this PublishStopRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStopRequestModel {
                NodeId = model.NodeId,
                Header = model.Header?.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static PublishStopResponseApiModel ToApiModel(
            this PublishStopResultModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStopResponseApiModel {
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        /// <param name="model"></param>
        public static PublishStopResultModel ToServiceModel(
            this PublishStopResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishStopResultModel {
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        public static PublishBulkRequestApiModel ToApiModel(
            this PublishBulkRequestModel model) {
            if (model == null) {
                return null;
            }
            return new PublishBulkRequestApiModel {
                NodesToAdd = model.NodesToAdd?
                    .Select(n => n.ToApiModel())
                    .ToList(),
                NodesToRemove = model.NodesToRemove?
                    .ToList(),
                Header = model.Header?.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static PublishBulkRequestModel ToServiceModel(
            this PublishBulkRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishBulkRequestModel {
                NodesToAdd = model.NodesToAdd?
                    .Select(n => n.ToServiceModel())
                    .ToList(),
                NodesToRemove = model.NodesToRemove?
                    .ToList(),
                Header = model.Header?.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model from service model
        /// </summary>
        /// <param name="model"></param>
        public static PublishBulkResponseApiModel ToApiModel(
            this PublishBulkResultModel model) {
            if (model == null) {
                return null;
            }
            return new PublishBulkResponseApiModel {
                NodesToAdd = model.NodesToAdd?
                    .Select(n => n.ToApiModel())
                    .ToList(),
                NodesToRemove = model.NodesToRemove?
                    .Select(n => n.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        /// <param name="model"></param>
        public static PublishBulkResultModel ToServiceModel(
            this PublishBulkResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new PublishBulkResultModel {
                NodesToAdd = model.NodesToAdd?
                    .Select(n => n.ToServiceModel())
                    .ToList(),
                NodesToRemove = model.NodesToRemove?
                    .Select(n => n.ToServiceModel())
                    .ToList()
            };
        }
        /// <summary>
        /// Create api model
        /// </summary>
        public static AggregateConfigurationApiModel ToApiModel(
            this AggregateConfigurationModel model) {
            if (model == null) {
                return null;
            }
            return new AggregateConfigurationApiModel {
                UseServerCapabilitiesDefaults = model.UseServerCapabilitiesDefaults,
                TreatUncertainAsBad = model.TreatUncertainAsBad,
                PercentDataBad = model.PercentDataBad,
                PercentDataGood = model.PercentDataGood,
                UseSlopedExtrapolation = model.UseSlopedExtrapolation
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static AggregateConfigurationModel ToServiceModel(
            this AggregateConfigurationApiModel model) {
            if (model == null) {
                return null;
            }
            return new AggregateConfigurationModel {
                UseServerCapabilitiesDefaults = model.UseServerCapabilitiesDefaults,
                TreatUncertainAsBad = model.TreatUncertainAsBad,
                PercentDataBad = model.PercentDataBad,
                PercentDataGood = model.PercentDataGood,
                UseSlopedExtrapolation = model.UseSlopedExtrapolation
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ContentFilterApiModel ToApiModel(
            this ContentFilterModel model) {
            if (model == null) {
                return null;
            }
            return new ContentFilterApiModel {
                Elements = model.Elements?
                    .Select(e => e.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ContentFilterModel ToServiceModel(
            this ContentFilterApiModel model) {
            if (model == null) {
                return null;
            }
            return new ContentFilterModel {
                Elements = model.Elements?
                    .Select(e => e.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ContentFilterElementApiModel ToApiModel(
            this ContentFilterElementModel model) {
            if (model == null) {
                return null;
            }
            return new ContentFilterElementApiModel {
                FilterOperands = model.FilterOperands?
                    .Select(f => f.ToApiModel())
                    .ToList(),
                FilterOperator = (Core.Api.Models.FilterOperatorType)model.FilterOperator
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ContentFilterElementModel ToServiceModel(
            this ContentFilterElementApiModel model) {
            if (model == null) {
                return null;
            }
            return new ContentFilterElementModel {
                FilterOperands = model.FilterOperands?
                    .Select(f => f.ToServiceModel())
                    .ToList(),
                FilterOperator = (Platform.Core.Models.FilterOperatorType)model.FilterOperator
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static DeleteEventsDetailsApiModel ToApiModel(
            this DeleteEventsDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteEventsDetailsApiModel {
                EventIds = model.EventIds
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static DeleteEventsDetailsModel ToServiceModel(
            this DeleteEventsDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteEventsDetailsModel {
                EventIds = model.EventIds
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static DeleteModifiedValuesDetailsApiModel ToApiModel(
            this DeleteModifiedValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteModifiedValuesDetailsApiModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static DeleteModifiedValuesDetailsModel ToServiceModel(
            this DeleteModifiedValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteModifiedValuesDetailsModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static DeleteValuesAtTimesDetailsApiModel ToApiModel(
            this DeleteValuesAtTimesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteValuesAtTimesDetailsApiModel {
                ReqTimes = model.ReqTimes
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static DeleteValuesAtTimesDetailsModel ToServiceModel(
            this DeleteValuesAtTimesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteValuesAtTimesDetailsModel {
                ReqTimes = model.ReqTimes
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static DeleteValuesDetailsApiModel ToApiModel(
            this DeleteValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteValuesDetailsApiModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static DeleteValuesDetailsModel ToServiceModel(
            this DeleteValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new DeleteValuesDetailsModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static EventFilterApiModel ToApiModel(
            this EventFilterModel model) {
            if (model == null) {
                return null;
            }
            return new EventFilterApiModel {
                SelectClauses = model.SelectClauses?
                    .Select(e => e.ToApiModel())
                    .ToList(),
                WhereClause = model.WhereClause.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static EventFilterModel ToServiceModel(
            this EventFilterApiModel model) {
            if (model == null) {
                return null;
            }
            return new EventFilterModel {
                SelectClauses = model.SelectClauses?
                    .Select(e => e.ToServiceModel())
                    .ToList(),
                WhereClause = model.WhereClause.ToServiceModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static FilterOperandApiModel ToApiModel(
            this FilterOperandModel model) {
            if (model == null) {
                return null;
            }
            return new FilterOperandApiModel {
                Index = model.Index,
                Alias = model.Alias,
                Value = model.Value,
                NodeId = model.NodeId,
                AttributeId = (Core.Api.Models.NodeAttribute?)model.AttributeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static FilterOperandModel ToServiceModel(
            this FilterOperandApiModel model) {
            if (model == null) {
                return null;
            }
            return new FilterOperandModel {
                Index = model.Index,
                Alias = model.Alias,
                Value = model.Value,
                NodeId = model.NodeId,
                AttributeId = (Platform.Core.Models.NodeAttribute?)model.AttributeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoricEventApiModel ToApiModel(
            this HistoricEventModel model) {
            if (model == null) {
                return null;
            }
            return new HistoricEventApiModel {
                EventFields = model.EventFields
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static HistoricEventModel ToServiceModel(
            this HistoricEventApiModel model) {
            if (model == null) {
                return null;
            }
            return new HistoricEventModel {
                EventFields = model.EventFields
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoricValueApiModel ToApiModel(
            this HistoricValueModel model) {
            if (model == null) {
                return null;
            }
            return new HistoricValueApiModel {
                Value = model.Value,
                StatusCode = model.StatusCode,
                SourceTimestamp = model.SourceTimestamp,
                SourcePicoseconds = model.SourcePicoseconds,
                ServerTimestamp = model.ServerTimestamp,
                ServerPicoseconds = model.ServerPicoseconds,
                ModificationInfo = model.ModificationInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static HistoricValueModel ToServiceModel(
            this HistoricValueApiModel model) {
            if (model == null) {
                return null;
            }
            return new HistoricValueModel {
                Value = model.Value,
                StatusCode = model.StatusCode,
                SourceTimestamp = model.SourceTimestamp,
                SourcePicoseconds = model.SourcePicoseconds,
                ServerTimestamp = model.ServerTimestamp,
                ServerPicoseconds = model.ServerPicoseconds,
                ModificationInfo = model.ModificationInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadNextRequestApiModel ToApiModel(
            this HistoryReadNextRequestModel model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadNextRequestApiModel {
                ContinuationToken = model.ContinuationToken,
                Abort = model.Abort,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static HistoryReadNextRequestModel ToServiceModel(
            this HistoryReadNextRequestApiModel model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadNextRequestModel {
                ContinuationToken = model.ContinuationToken,
                Abort = model.Abort,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        public static HistoryReadNextResponseApiModel<T> ToApiModel<S, T>(
            this HistoryReadNextResultModel<S> model, Func<S, T> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryReadNextResponseApiModel<T> {
                History = convert(model.History),
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create from api model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        public static HistoryReadNextResultModel<T> ToServiceModel<S, T>(
            this HistoryReadNextResponseApiModel<S> model, Func<S, T> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryReadNextResultModel<T> {
                History = convert(model.History),
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadRequestApiModel<VariantValue> ToApiModel(
            this HistoryReadRequestModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadRequestApiModel<VariantValue> {
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange,
                Details = model.Details,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadRequestModel<VariantValue> ToServiceModel(
            this HistoryReadRequestApiModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadRequestModel<VariantValue> {
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange,
                Details = model.Details,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadNextResponseApiModel<VariantValue> ToApiModel(
            this HistoryReadNextResultModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadNextResponseApiModel<VariantValue> {
                History = model.History,
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadNextResultModel<VariantValue> ToServiceModel(
            this HistoryReadNextResponseApiModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadNextResultModel<VariantValue> {
                History = model.History,
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadResponseApiModel<VariantValue> ToApiModel(
            this HistoryReadResultModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadResponseApiModel<VariantValue> {
                History = model.History,
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryReadResultModel<VariantValue> ToServiceModel(
            this HistoryReadResponseApiModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryReadResultModel<VariantValue> {
                History = model.History,
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryUpdateRequestApiModel<VariantValue> ToApiModel(
            this HistoryUpdateRequestModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryUpdateRequestApiModel<VariantValue> {
                Details = model.Details,
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert back to service model
        /// </summary>
        /// <returns></returns>
        public static HistoryUpdateRequestModel<VariantValue> ToServiceModel(
            this HistoryUpdateRequestApiModel<VariantValue> model) {
            if (model == null) {
                return null;
            }
            return new HistoryUpdateRequestModel<VariantValue> {
                Details = model.Details,
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static HistoryReadRequestApiModel<S> ToApiModel<S, T>(
            this HistoryReadRequestModel<T> model, Func<T, S> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryReadRequestApiModel<S> {
                Details = convert(model.Details),
                BrowsePath = model.BrowsePath,
                NodeId = model.NodeId,
                IndexRange = model.IndexRange,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static HistoryReadRequestModel<S> ToServiceModel<S, T>(
            this HistoryReadRequestApiModel<T> model, Func<T, S> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryReadRequestModel<S> {
                Details = convert(model.Details),
                BrowsePath = model.BrowsePath,
                NodeId = model.NodeId,
                IndexRange = model.IndexRange,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        public static HistoryReadResponseApiModel<T> ToApiModel<S, T>(
            this HistoryReadResultModel<S> model, Func<S, T> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryReadResponseApiModel<T> {
                History = convert(model.History),
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Create to service model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        public static HistoryReadResultModel<T> ToServiceModel<S, T>(
            this HistoryReadResponseApiModel<S> model, Func<S, T> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryReadResultModel<T> {
                History = convert(model.History),
                ContinuationToken = model.ContinuationToken,
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static HistoryUpdateRequestApiModel<S> ToApiModel<S, T>(
            this HistoryUpdateRequestModel<T> model, Func<T, S> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryUpdateRequestApiModel<S> {
                Details = convert(model.Details),
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                Header = model.Header.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static HistoryUpdateRequestModel<S> ToServiceModel<S, T>(
            this HistoryUpdateRequestApiModel<T> model, Func<T, S> convert) {
            if (model == null) {
                return null;
            }
            return new HistoryUpdateRequestModel<S> {
                Details = convert(model.Details),
                NodeId = model.NodeId,
                BrowsePath = model.BrowsePath,
                Header = model.Header.ToServiceModel()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryUpdateResponseApiModel ToApiModel(
            this HistoryUpdateResultModel model) {
            if (model == null) {
                return null;
            }
            return new HistoryUpdateResponseApiModel {
                Results = model.Results?
                    .Select(r => r.ToApiModel())
                    .ToList(),
                ErrorInfo = model.ErrorInfo.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to service model
        /// </summary>
        /// <param name="model"></param>
        public static HistoryUpdateResultModel ToServiceModel(
            this HistoryUpdateResponseApiModel model) {
            if (model == null) {
                return null;
            }
            return new HistoryUpdateResultModel {
                Results = model.Results?
                    .Select(r => r.ToServiceModel())
                    .ToList(),
                ErrorInfo = model.ErrorInfo.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static InsertEventsDetailsApiModel ToApiModel(
            this InsertEventsDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new InsertEventsDetailsApiModel {
                Filter = model.Filter.ToApiModel(),
                Events = model.Events?
                    .Select(v => v.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static InsertEventsDetailsModel ToServiceModel(
            this InsertEventsDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new InsertEventsDetailsModel {
                Filter = model.Filter.ToServiceModel(),
                Events = model.Events?
                    .Select(v => v.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static InsertValuesDetailsApiModel ToApiModel(
            this InsertValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new InsertValuesDetailsApiModel {
                Values = model.Values?
                    .Select(v => v.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static InsertValuesDetailsModel ToServiceModel(
            this InsertValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new InsertValuesDetailsModel {
                Values = model.Values?
                    .Select(v => v.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create from service model
        /// </summary>
        /// <param name="model"></param>
        public static ModificationInfoApiModel ToApiModel(
            this ModificationInfoModel model) {
            if (model == null) {
                return null;
            }
            return new ModificationInfoApiModel {
                ModificationTime = model.ModificationTime,
                UpdateType = (HistoryUpdateOperation?)model.UpdateType,
                UserName = model.UserName
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ModificationInfoModel ToServiceModel(
            this ModificationInfoApiModel model) {
            if (model == null) {
                return null;
            }
            return new ModificationInfoModel {
                ModificationTime = model.ModificationTime,
                UpdateType = (Platform.Twin.Models.HistoryUpdateOperation?)model.UpdateType,
                UserName = model.UserName
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ReadEventsDetailsApiModel ToApiModel(
            this ReadEventsDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReadEventsDetailsApiModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                NumEvents = model.NumEvents,
                Filter = model.Filter.ToApiModel()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReadEventsDetailsModel ToServiceModel(
            this ReadEventsDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadEventsDetailsModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                NumEvents = model.NumEvents,
                Filter = model.Filter.ToServiceModel()
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ReadModifiedValuesDetailsApiModel ToApiModel(
            this ReadModifiedValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReadModifiedValuesDetailsApiModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                NumValues = model.NumValues
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReadModifiedValuesDetailsModel ToServiceModel(
            this ReadModifiedValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadModifiedValuesDetailsModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                NumValues = model.NumValues
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ReadProcessedValuesDetailsApiModel ToApiModel(
            this ReadProcessedValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReadProcessedValuesDetailsApiModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                ProcessingInterval = model.ProcessingInterval,
                AggregateConfiguration = model.AggregateConfiguration.ToApiModel(),
                AggregateTypeId = model.AggregateTypeId
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReadProcessedValuesDetailsModel ToServiceModel(
            this ReadProcessedValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadProcessedValuesDetailsModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                ProcessingInterval = model.ProcessingInterval,
                AggregateConfiguration = model.AggregateConfiguration.ToServiceModel(),
                AggregateTypeId = model.AggregateTypeId
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ReadValuesAtTimesDetailsApiModel ToApiModel(
            this ReadValuesAtTimesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReadValuesAtTimesDetailsApiModel {
                ReqTimes = model.ReqTimes,
                UseSimpleBounds = model.UseSimpleBounds
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReadValuesAtTimesDetailsModel ToServiceModel(
            this ReadValuesAtTimesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadValuesAtTimesDetailsModel {
                ReqTimes = model.ReqTimes,
                UseSimpleBounds = model.UseSimpleBounds
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ReadValuesDetailsApiModel ToApiModel(
            this ReadValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReadValuesDetailsApiModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                NumValues = model.NumValues,
                ReturnBounds = model.ReturnBounds
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReadValuesDetailsModel ToServiceModel(
            this ReadValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReadValuesDetailsModel {
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                NumValues = model.NumValues,
                ReturnBounds = model.ReturnBounds
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static ReplaceEventsDetailsApiModel ToApiModel(
            this ReplaceEventsDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReplaceEventsDetailsApiModel {
                Filter = model.Filter.ToApiModel(),
                Events = model.Events?
                    .Select(v => v.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReplaceEventsDetailsModel ToServiceModel(
            this ReplaceEventsDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReplaceEventsDetailsModel {
                Filter = model.Filter.ToServiceModel(),
                Events = model.Events?
                    .Select(v => v.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReplaceValuesDetailsApiModel ToApiModel(
            this ReplaceValuesDetailsModel model) {
            if (model == null) {
                return null;
            }
            return new ReplaceValuesDetailsApiModel {
                Values = model.Values?
                    .Select(v => v.ToApiModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static ReplaceValuesDetailsModel ToServiceModel(
            this ReplaceValuesDetailsApiModel model) {
            if (model == null) {
                return null;
            }
            return new ReplaceValuesDetailsModel {
                Values = model.Values?
                    .Select(v => v.ToServiceModel())
                    .ToList()
            };
        }

        /// <summary>
        /// Create service model from api model
        /// </summary>
        public static SimpleAttributeOperandModel ToServiceModel(
            this SimpleAttributeOperandApiModel model) {
            if (model == null) {
                return null;
            }
            return new SimpleAttributeOperandModel {
                NodeId = model.NodeId,
                AttributeId = (Platform.Core.Models.NodeAttribute?)model.AttributeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange
            };
        }

        /// <summary>
        /// Create api model
        /// </summary>
        public static SimpleAttributeOperandApiModel ToApiModel(
            this SimpleAttributeOperandModel model) {
            if (model == null) {
                return null;
            }
            return new SimpleAttributeOperandApiModel {
                NodeId = model.NodeId,
                AttributeId = (Core.Api.Models.NodeAttribute?)model.AttributeId,
                BrowsePath = model.BrowsePath,
                IndexRange = model.IndexRange
            };
        }
    }
}
