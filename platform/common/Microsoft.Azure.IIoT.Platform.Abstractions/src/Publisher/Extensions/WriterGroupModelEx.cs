﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Clone
    /// </summary>
    public static class WriterGroupModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static WriterGroupModel Clone(this WriterGroupModel model) {
            if (model?.DataSetWriters == null) {
                return null;
            }
            return new WriterGroupModel {
                WriterGroupId = model.WriterGroupId,
                DataSetWriters = model.DataSetWriters
                    .Select(f => f.Clone())
                    .ToList(),
                HeaderLayoutUri = model.HeaderLayoutUri,
                KeepAliveTime = model.KeepAliveTime,
                State = model.State.Clone(),
                GenerationId = model.GenerationId,
                SiteId = model.SiteId,
                LocaleIds = model.LocaleIds?.ToList(),
                MaxNetworkMessageSize = model.MaxNetworkMessageSize,
                MessageSettings = model.MessageSettings.Clone(),
                Encoding = model.Encoding,
                Schema = model.Schema,
                Name = model.Name,
                Priority = model.Priority,
                PublishingInterval = model.PublishingInterval,
                SecurityGroupId = model.SecurityGroupId,
                SecurityKeyServices = model.SecurityKeyServices?
                    .Select(c => c.Clone())
                    .ToList(),
                SecurityMode = model.SecurityMode,
            };
        }

        /// <summary>
        /// Convert to writer group info
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static WriterGroupInfoModel AsWriterGroupInfo(this WriterGroupModel model,
            PublisherOperationContextModel context) {
            return new WriterGroupInfoModel {
                BatchSize = model.BatchSize,
                PublishingInterval = model.PublishingInterval,
                HeaderLayoutUri = model.HeaderLayoutUri,
                WriterGroupId = model.WriterGroupId,
                KeepAliveTime = model.KeepAliveTime,
                LocaleIds = model.LocaleIds?.ToList(),
                MaxNetworkMessageSize = model.MaxNetworkMessageSize,
                Encoding = model.Encoding,
                Schema = model.Schema,
                Name = model.Name,
                Priority = model.Priority,
                SiteId = model.SiteId,
                State = model.State.Clone(),
                MessageSettings = model.MessageSettings == null ? null :
                    new WriterGroupMessageSettingsModel {
                        DataSetOrdering = model.MessageSettings.DataSetOrdering,
                        GroupVersion = model.MessageSettings.GroupVersion,
                        NetworkMessageContentMask =
                            model.MessageSettings.NetworkMessageContentMask,
                        PublishingOffset = model.MessageSettings.PublishingOffset,
                        SamplingOffset = model.MessageSettings.SamplingOffset
                    },
                Updated = context,
                Created = context,
                GenerationId = null,
                SecurityGroupId = null,
                SecurityKeyServices = null,
                SecurityMode = null // TODO
            };
        }

        /// <summary>
        /// Create version hash from generations
        /// </summary>
        /// <returns></returns>
        public static string CalculateVersionHash(this WriterGroupModel model) {
            var sb = new StringBuilder();
            sb.Append(model.GenerationId);
            if (model.DataSetWriters != null) {
                foreach (var writer in model.DataSetWriters) {
                    sb.Append(writer.GenerationId);
                    var dataset =
                        writer.DataSet?.DataSetSource?.PublishedVariables?.PublishedData;
                    if (dataset != null) {
                        foreach (var item in dataset) {
                            sb.Append(item.GenerationId);
                        }
                    }
                    var events = writer.DataSet?.DataSetSource.PublishedEvents;
                    if (events != null) {
                        sb.Append(events.GenerationId);
                    }
                }
            }
            return sb.ToString().ToSha256Hash();
        }
    }
}