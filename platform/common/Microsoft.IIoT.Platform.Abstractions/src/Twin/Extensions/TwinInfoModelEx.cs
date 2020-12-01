// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Twin extensions
    /// </summary>
    public static class TwinInfoModelEx {

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEnumerable<TwinInfoModel> model,
            IEnumerable<TwinInfoModel> that) {
            if (model == that) {
                return true;
            }
            return model.SetEqualsSafe(that, (x, y) => x.IsSameAs(y));
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this TwinInfoModel model,
            TwinInfoModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            return
                model.EndpointId == that.EndpointId &&
                model.Id == that.Id &&
                model.User.IsSameAs(that.User) &&
                model.OperationTimeout == that.OperationTimeout &&
              //  model.Created.IsSameAs(that.Created) &&
              //  model.Updated.IsSameAs(that.Updated) &&
              //  model.Diagnostics.IsSameAs(that.Diagnostics) &&
                model.ConnectionState.IsSameAs(that.ConnectionState);
        }

        /// <summary>
        /// Get model from request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TwinInfoModel AsTwinInfo(
            this TwinActivationRequestModel model, OperationContextModel context) {
            if (model == null) {
                return null;
            }
            return new TwinInfoModel {
                GenerationId = null,
                ConnectionState = null,
                Id = model.Id,
                EndpointId = model.EndpointId,
                User = model.User.Clone(),
                Diagnostics = model.Diagnostics.Clone(),
                OperationTimeout = model.OperationTimeout,
                Created = context.Clone(),
                Updated = context.Clone()
            };
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static TwinInfoModel Clone(this TwinInfoModel model) {
            if (model == null) {
                return null;
            }
            return new TwinInfoModel {
                EndpointId = model.EndpointId,
                GenerationId = model.GenerationId,
                Id = model.Id,
                Created = model.Created.Clone(),
                Updated = model.Updated.Clone(),
                ConnectionState = model.ConnectionState.Clone(),
                OperationTimeout = model.OperationTimeout,
                User = model.User.Clone(),
                Diagnostics = model.Diagnostics.Clone(),
            };
        }

        /// <summary>
        /// Patch twin
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="model"></param>
        public static TwinInfoModel Patch(this TwinInfoModel twin,
            TwinInfoModel model) {
            if (twin == null) {
                return model;
            }
            if (model == null) {
                return twin;
            }
            twin.EndpointId = model.EndpointId;
            twin.GenerationId = model.GenerationId;
            twin.Id = model.Id;
            twin.Updated = model.Updated.Clone();
            twin.ConnectionState = model.ConnectionState.Clone();
            twin.OperationTimeout = model.OperationTimeout;
            twin.User = model.User.Clone();
            twin.Diagnostics = model.Diagnostics.Clone();
            return twin;
        }
    }
}
