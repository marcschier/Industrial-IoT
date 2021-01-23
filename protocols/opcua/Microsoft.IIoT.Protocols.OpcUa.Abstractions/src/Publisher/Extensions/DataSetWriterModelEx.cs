﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Dataset writer model extensions
    /// </summary>
    public static class DataSetWriterModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataSetWriterModel Clone(this DataSetWriterModel model) {
            if (model == null) {
                return null;
            }
            return new DataSetWriterModel {
                DataSet = model.DataSet.Clone(),
                DataSetFieldContentMask = model.DataSetFieldContentMask,
                DataSetWriterId = model.DataSetWriterId,
                GenerationId = model.GenerationId,
                MessageSettings = model.MessageSettings.Clone()
            };
        }

        /// <summary>
        /// Convert to info model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="endpointId"></param>
        /// <param name="writerGroupId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataSetWriterInfoModel AsDataSetWriterInfo(
            this DataSetWriterModel model, string writerGroupId, string endpointId,
            OperationContextModel context) {
            if (model == null) {
                return null;
            }
            return new DataSetWriterInfoModel {
                DataSet = model.DataSet.AsPublishedDataSetSourceInfo(endpointId),
                WriterGroupId = writerGroupId,
                IsDisabled = false,
                Created = context,
                Updated = context,
                DataSetFieldContentMask = model.DataSetFieldContentMask,
                DataSetWriterId = model.DataSetWriterId,
                GenerationId = model.GenerationId,
                MessageSettings = model.MessageSettings.Clone()
            };
        }
    }
}