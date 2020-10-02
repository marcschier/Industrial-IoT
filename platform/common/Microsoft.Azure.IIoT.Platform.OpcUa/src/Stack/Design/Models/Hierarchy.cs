/* ========================================================================
 * Copyright (c) 2005-2016 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

namespace Opc.Ua.Models {
    using Opc.Ua.Design.Schema;
    using System.Collections.Generic;

    /// <summary>
    /// A node hierarchy
    /// </summary>
    public class Hierarchy {

        /// <summary>
        /// Type
        /// </summary>
        public TypeDesign Type { get; set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public Dictionary<string, HierarchyNode> Nodes { get; } =
            new Dictionary<string, HierarchyNode>();

        /// <summary>
        /// Node lsit
        /// </summary>
        public List<HierarchyNode> NodeList { get; } =
            new List<HierarchyNode>();

        /// <summary>
        /// References
        /// </summary>
        public List<HierarchyReference> References { get; } =
            new List<HierarchyReference>();
    }
}
