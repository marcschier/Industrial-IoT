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

namespace Opc.Ua.Design.Schema {
    using System.Collections;

    /// <summary>
    /// Stores the information that describes how to initialize and process a template.
    /// </summary>
    public class TemplateDefinition {

        /// <summary>
        /// The path of the template to load.
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// The targets that the template should be applied to.
        /// </summary>
        public ICollection Targets { get; internal set; }

        /// <summary>
        /// The callback to call when loading the template.
        /// </summary>
        public event LoadTemplateEventHandler LoadTemplate {
            add { _loadTemplate += value; }
            remove { _loadTemplate -= value; }
        }

        /// <summary>
        /// The callback to call when writing the template.
        /// </summary>
        public event WriteTemplateEventHandler WriteTemplate {
            add { _writeTemplate += value; }
            remove { _writeTemplate -= value; }
        }

        /// <summary>
        /// Loads the template.
        /// </summary>
        public string Load(Template template, GeneratorContext context) {
            // check for override.
            if (_loadTemplate != null) {
                return _loadTemplate(template, context);
            }

            // use the default function to write the template.
            return context.TemplatePath;
        }

        /// <summary>
        /// Writes the template.
        /// </summary>
        public bool Write(Template template, GeneratorContext context) {
            // check for override.
            if (_writeTemplate != null) {
                return _writeTemplate(template, context);
            }

            // use the default function to write the template.
            return template.WriteTemplate(context);
        }

#pragma warning disable IDE1006 // Naming Styles
        private event LoadTemplateEventHandler _loadTemplate;
        private event WriteTemplateEventHandler _writeTemplate;
#pragma warning restore IDE1006 // Naming Styles
    }
}
