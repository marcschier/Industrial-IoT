// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Opc.Ua.Nodeset {
    using System.Runtime.Serialization;

    /// <summary>
    /// Data type nodes.
    /// </summary>
    [DataContract(Name = "DataType")]
    public class DataTypeNodeModel : TypeNodeModel {

        /// <summary>
        /// Create data type state.
        /// </summary>
        public DataTypeNodeModel() :
            base(NodeClass.DataType) {
        }

        /// <summary>
        /// The definition of the data type
        /// </summary>
        [DataMember]
        public DataTypeDefinition Definition { get; set; }

        /// <summary>
        /// The purpose of the data type.
        /// </summary>
        [DataMember]
        public Schema.DataTypePurpose Purpose { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (obj is not DataTypeNodeModel model) {
                return false;
            }
            if (!Utils.IsEqual(Definition, model.Definition)) {
                return false;
            }
            if (Purpose != model.Purpose) {
                return false;
            }
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return System.HashCode.Combine(base.GetHashCode(), Purpose);
        }
    }
}