// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa.Services {
    using Microsoft.Azure.IIoT.Platform.OpcUa;
    using Opc.Ua;
    using Opc.Ua.Nodeset;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles adding variables
    /// </summary>
    public abstract class NodeHandlerBase : INodeHandler {

        /// <inheritdoc/>
        public abstract Task CompleteAsync(ISystemContext context, bool abort);

        /// <inheritdoc/>
        public virtual Task HandleAsync(ObjectNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(PropertyNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(VariableNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(MethodNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(ViewNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(ObjectTypeNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(PropertyTypeNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(VariableTypeNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(DataTypeNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task HandleAsync(ReferenceTypeNodeModel node, ISystemContext context) {
            return Task.CompletedTask;
        }

    }
}