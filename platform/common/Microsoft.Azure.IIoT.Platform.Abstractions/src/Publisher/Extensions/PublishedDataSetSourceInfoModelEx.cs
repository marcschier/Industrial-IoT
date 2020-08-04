// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Published DataSet info model extensions
    /// </summary>
    public static class PublishedDataSetSourceInfoModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublishedDataSetSourceInfoModel Clone(
            this PublishedDataSetSourceInfoModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedDataSetSourceInfoModel {
                Name = model.Name,
                User = model.User.Clone(),
                EndpointId = model.EndpointId,
                ExtensionFields = model.ExtensionFields,
                SubscriptionSettings = model.SubscriptionSettings.Clone(),
                State = model.State.Clone(),
                DiagnosticsLevel = model.DiagnosticsLevel,
                OperationTimeout = model.OperationTimeout
            };
        }

        /// <summary>
        /// Convert to published data set model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="connection"></param>
        /// <param name="variables"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static PublishedDataSetModel AsPublishedDataSetModel(
            this PublishedDataSetSourceInfoModel model, ConnectionModel connection,
            PublishedDataItemsModel variables, PublishedDataSetEventsModel events) {
            if (model == null) {
                return null;
            }
            connection = connection.Clone();
            connection.User = model.User;
            return new PublishedDataSetModel {
                Name = model.Name,
                DataSetSource = new PublishedDataSetSourceModel {
                    Connection = connection,
                    PublishedEvents = events,
                    PublishedVariables = variables,
                    SubscriptionSettings = model.SubscriptionSettings.Clone(),
                    State = model.State.Clone()
                },
                DataSetMetaData = null, // TODO
                ExtensionFields = model.ExtensionFields
            };
        }
    }
}