// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Opc.Ua.Encoders {
    using System;
    using System.Xml;
    using System.IO;
    using Newtonsoft.Json;
    using Microsoft.Azure.IIoT;

    /// <summary>
    /// Type serializer service implementation
    /// </summary>
    public class TypeSerializer : ITypeSerializer {

        /// <summary>
        /// Create codec
        /// </summary>
        /// <param name="context"></param>
        public TypeSerializer(ServiceMessageContext context = null) {
            _context = context ?? ServiceMessageContext.GlobalContext;
        }

        /// <inheritdoc/>
        public T Decode<T>(string contentType, byte[] input,
            Func<IDecoder, T> reader) {
            if (contentType is null) {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (input is null) {
                throw new ArgumentNullException(nameof(input));
            }
            if (reader is null) {
                throw new ArgumentNullException(nameof(reader));
            }
            using (var stream = new MemoryStream(input)) {
                IDecoder decoder = null;
                try {
                    decoder = CreateDecoder(contentType, stream);
                    return reader(decoder);
                }
                finally {
                    if (decoder is IDisposable dispose) {
                        dispose.Dispose();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public byte[] Encode(string contentType, Action<IEncoder> writer) {
            if (contentType is null) {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (writer is null) {
                throw new ArgumentNullException(nameof(writer));
            }
            using (var stream = new MemoryStream()) {
                IEncoder encoder = null;
                try {
                    encoder = CreateEncoder(contentType, stream);
                    writer(encoder);

                    // Dispose should flush
                    if (encoder is IDisposable dispose) {
                        dispose.Dispose();
                    }
                    return stream.ToArray();
                }
                catch {
                    if (encoder is IDisposable dispose) {
                        dispose.Dispose();
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Create decoder
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private IDecoder CreateDecoder(string contentType, Stream stream) {
            if (contentType is null) {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaJson) ||
                contentType.EqualsIgnoreCase(ContentMimeType.UaNonReversibleJson) ||
                contentType.EqualsIgnoreCase(ContentMimeType.UaNonReversibleJsonReference)) {
                return new JsonDecoderEx(stream, _context);
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaBinary)) {
                return new BinaryDecoder(stream, _context);
            }

            if (contentType.EqualsIgnoreCase(ContentMimeType.UaXml)) {
                return new XmlDecoder(null, XmlReader.Create(stream), _context);
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaJsonReference)) {
                return new JsonDecoder(null, new JsonTextReader(
                    new StreamReader(stream)), _context);
            }

            throw new ArgumentException("Bad content type", nameof(contentType));
        }

        /// <summary>
        /// Create encoder
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private IEncoder CreateEncoder(string contentType, Stream stream) {
            if (contentType is null) {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaJson)) {
                return new JsonEncoderEx(stream, _context);
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaNonReversibleJson)) {
                return new JsonEncoderEx(stream, _context) {
                    UseReversibleEncoding = false
                };
            }

            if (contentType.EqualsIgnoreCase(ContentMimeType.UaJsonReference)) {
                return new JsonEncoder(_context, true, new StreamWriter(stream));
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaNonReversibleJsonReference)) {
                return new JsonEncoder(_context, false, new StreamWriter(stream));
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaBinary)) {
                return new BinaryEncoder(stream, _context);
            }
            if (contentType.EqualsIgnoreCase(ContentMimeType.UaXml)) {
                return new XmlEncoder(
                    new XmlQualifiedName("ua", Namespaces.OpcUaXsd),
                        XmlWriter.Create(stream), _context);
            }

            throw new ArgumentException("Bad content type", nameof(contentType));
        }

        private readonly ServiceMessageContext _context;
    }
}
