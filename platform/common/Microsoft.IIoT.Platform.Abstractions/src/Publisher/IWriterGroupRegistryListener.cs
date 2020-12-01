// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Notified when writer groups change
    /// </summary>
    public interface IWriterGroupRegistryListener {

        /// <summary>
        /// New group added
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writerGroup"></param>
        /// <returns></returns>
        Task OnWriterGroupAddedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup);

        /// <summary>
        /// Called when group or group content was updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writerGroup"></param>
        /// <returns></returns>
        Task OnWriterGroupUpdatedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup);

        /// <summary>
        /// Called when group state changed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writerGroup"></param>
        /// <returns></returns>
        Task OnWriterGroupStateChangeAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup);

        /// <summary>
        /// Called when group was activated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writerGroup"></param>
        /// <returns></returns>
        Task OnWriterGroupActivatedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup);

        /// <summary>
        /// Called when group was deactivated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writerGroup"></param>
        /// <returns></returns>
        Task OnWriterGroupDeactivatedAsync(OperationContextModel context,
            WriterGroupInfoModel writerGroup);

        /// <summary>
        /// Called when writer group.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writerGroupId"></param>
        /// <returns></returns>
        Task OnWriterGroupRemovedAsync(OperationContextModel context,
            string writerGroupId);
    }
}
