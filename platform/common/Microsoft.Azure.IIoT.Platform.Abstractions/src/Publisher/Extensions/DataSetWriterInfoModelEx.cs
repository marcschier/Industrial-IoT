// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// DataSet Writer model extensions
    /// </summary>
    public static class DataSetWriterInfoModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataSetWriterInfoModel Clone(this DataSetWriterInfoModel model) {
            if (model == null) {
                return null;
            }
            return new DataSetWriterInfoModel {
                DataSetWriterId = model.DataSetWriterId,
                GenerationId = model.GenerationId,
                WriterGroupId = model.WriterGroupId,
                DataSet = model.DataSet.Clone(),
                DataSetFieldContentMask = model.DataSetFieldContentMask,
                DataSetMetaDataSendInterval = model.DataSetMetaDataSendInterval,
                KeyFrameCount = model.KeyFrameCount,
                KeyFrameInterval = model.KeyFrameInterval,
                MessageSettings = model.MessageSettings.Clone()
            };
        }

        /// <summary>
        /// Convert to writer model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="connection"></param>
        /// <param name="variables"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static DataSetWriterModel AsDataSetWriter(this DataSetWriterInfoModel model,
            ConnectionModel connection, PublishedDataItemsModel variables,
            PublishedDataSetEventsModel events) {
            if (model == null) {
                return null;
            }
            return new DataSetWriterModel {
                DataSetWriterId = model.DataSetWriterId,
                GenerationId = model.GenerationId,
                DataSet = model.DataSet.AsPublishedDataSetModel(connection, variables, events),
                DataSetFieldContentMask = model.DataSetFieldContentMask,
                DataSetMetaDataSendInterval = model.DataSetMetaDataSendInterval,
                KeyFrameCount = model.KeyFrameCount,
                KeyFrameInterval = model.KeyFrameInterval,
                MessageSettings = model.MessageSettings.Clone()
            };
        }

        /// <summary>
        /// Get model from request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataSetWriterInfoModel AsDataSetWriterInfo(
            this DataSetWriterAddRequestModel model,
            PublisherOperationContextModel context) {
            if (model == null) {
                return null;
            }
            return new DataSetWriterInfoModel {
                DataSetWriterId = null,
                GenerationId = null,
                WriterGroupId = model.WriterGroupId,
                DataSet = new PublishedDataSetSourceInfoModel {
                    Name = model.DataSetName,
                    EndpointId = model.EndpointId,
                    ExtensionFields = model.ExtensionFields,
                    User = model.User.Clone(),
                    SubscriptionSettings = model.SubscriptionSettings.Clone(),
                    State = null,
                    DiagnosticsLevel = null,
                    OperationTimeout = null
                },
                DataSetFieldContentMask = model.DataSetFieldContentMask,
                KeyFrameCount = model.KeyFrameCount,
                KeyFrameInterval = model.KeyFrameInterval,
                MessageSettings = model.MessageSettings.Clone(),
                Created = context.Clone(),
                Updated = context.Clone(),
                DataSetMetaDataSendInterval = null, // TODO
            };
        }

        /// <summary>
        /// Whether it is default writer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsDefault(DataSetWriterInfoModel model) {
            if (model == null) {
                return false;
            }
            return model.DataSetWriterId == model.DataSet?.EndpointId;
        }
    }
}