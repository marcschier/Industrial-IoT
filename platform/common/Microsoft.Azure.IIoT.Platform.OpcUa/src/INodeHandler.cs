// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa {
    using Opc.Ua;
    using Opc.Ua.Nodeset;
    using System.Threading.Tasks;

    /// <summary>
    /// Node handler
    /// </summary>
    public interface INodeHandler {

        /// <summary>
        /// Handle object node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(ObjectNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle property node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(PropertyNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle variable node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(VariableNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle method node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(MethodNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle view node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(ViewNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle object type node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(ObjectTypeNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle property type node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(PropertyTypeNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle variable type node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(VariableTypeNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle data type node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(DataTypeNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Handle reference type node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(ReferenceTypeNodeModel node,
            ISystemContext context);

        /// <summary>
        /// Complete handling and close
        /// </summary>
        /// <param name="abort"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task CompleteAsync(ISystemContext context,
            bool abort = false);
    }
}