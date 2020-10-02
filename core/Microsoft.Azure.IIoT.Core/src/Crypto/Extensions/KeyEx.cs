// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Crypto.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

    /// <summary>
    /// Key extensions
    /// </summary>
    public static class KeyEx {

        /// <summary>
        /// Clone key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Key Clone(this Key key) {
            if (key == null) {
                return null;
            }
            return new Key {
                Type = key.Type,
                Parameters = key.Parameters.Clone()
            };
        }

        /// <summary>
        /// Compare key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool SameAs(this Key key, Key other) {
            if (key == null) {
                return key == null;
            }
            if (other == null) {
                return false;
            }
            if (key.Type != other.Type) {
                return false;
            }
            return key.Parameters.SameAs(other.Parameters);
        }

        /// <summary>
        /// Returns the public part of the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Key GetPublicKey(this Key key) {
            if (key is null) {
                return null;
            }
            switch (key.Type) {
                case KeyType.RSA:
                    return (key.Parameters as RsaParams)?.GetPublicKey();
                case KeyType.ECC:
                    return (key.Parameters as EccParams)?.GetPublicKey();
                default:
                    throw new NotSupportedException("Key type not supported");
            }
        }

        /// <summary>
        /// Convert to assymmetric algorithm
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static AsymmetricAlgorithm ToAsymmetricAlgorithm(this Key key) {
            if (key is null) {
                return null;
            }
            switch (key.Type) {
                case KeyType.RSA:
                    return key.ToRSA();
                case KeyType.ECC:
                    return key.ToECDsa();
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Convert to symmetric algorithm
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static SymmetricAlgorithm ToSymmetricAlgorithm(this Key key) {
            if (key is null) {
                return null;
            }
            switch (key.Type) {
                case KeyType.AES:
                    return key.ToAes();
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Convert to key
        /// </summary>
        /// <param name="alg"></param>
        /// <returns></returns>
        public static Key ToKey(this AsymmetricAlgorithm alg) {
            switch (alg) {
                case RSA rsa:
                    return rsa.ToKey();
                case ECDsa ecc:
                    return ecc.ToKey();
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Convert to key
        /// </summary>
        /// <param name="alg"></param>
        /// <returns></returns>
        public static Key ToPublicKey(this AsymmetricAlgorithm alg) {
            switch (alg) {
                case RSA rsa:
                    return rsa.ToPublicKey();
                case ECDsa ecc:
                    return ecc.ToPublicKey();
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Convert to key
        /// </summary>
        /// <param name="alg"></param>
        /// <returns></returns>
        public static Key ToKey(this SymmetricAlgorithm alg) {
            switch (alg) {
                case Aes aes:
                    return aes.ToKey();
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Json to key
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Key ToKey(this VariantValue token) {
            var type = token.GetValueOrDefault<KeyType>(nameof(Key.Type));
            switch (type) {
                case KeyType.RSA:
                    return new Key {
                        Type = type,
                        Parameters = token.GetValueOrDefault<RsaParams>(
                            nameof(Key.Parameters))
                    };
                case KeyType.ECC:
                    return new Key {
                        Type = type,
                        Parameters = token.GetValueOrDefault<EccParams>(
                            nameof(Key.Parameters))
                    };
                case KeyType.AES:
                    return new Key {
                        Type = type,
                        Parameters = token.GetValueOrDefault<AesParams>(
                            nameof(Key.Parameters))
                    };
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Verifies whether this object has a private key
        /// </summary>
        /// <returns> True if the object has private key; false otherwise.</returns>
        public static bool HasPrivateKey(this Key key) {
            if (key is null) {
                return false;
            }
            switch (key.Type) {
                case KeyType.RSA:
                    return (key.Parameters as RsaParams)?.HasPrivateKey() ?? false;
                case KeyType.ECC:
                    return (key.Parameters as EccParams)?.HasPrivateKey() ?? false;
                case KeyType.AES:
                    return (key.Parameters as AesParams)?.HasPrivateKey() ?? false;
                default:
                    throw new NotSupportedException("Algorithm not supported");
            }
        }

        /// <summary>
        /// Check non zero
        /// </summary>
        /// <param name="value"></param>
        internal static void VerifyNonZero(IReadOnlyCollection<byte> value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            if (!value.All(b => b != 0)) {
                throw new ArgumentException("Value has 0", nameof(value));
            }
        }

        /// <summary>
        /// Remove leading zeroes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static byte[] RemoveLeadingZeros(
            IReadOnlyCollection<byte> value) {
            if (value == null) {
                return null;
            }
            var copy = value.ToArray();
            if (copy.Length > 1 && copy[0] == 0) {
                for (var i = 1; i < copy.Length; i++) {
                    if (copy[i] != 0) {
                        var array = new byte[copy.Length - i];
                        Array.Copy(copy, i, array, 0, array.Length);
                        return array;
                    }
                }
                return new byte[1];
            }
            return copy;
        }

        /// <summary>
        /// Remove leading zeroes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        internal static bool SameNoLeadingZeros(
            IReadOnlyCollection<byte> value, IReadOnlyCollection<byte> other) {
            if (value == other) {
                return true;
            }
            if (value?.Count != other?.Count) {
                value = RemoveLeadingZeros(value);
                other = RemoveLeadingZeros(other);
                if (value?.Count != other?.Count) {
                    return false;
                }
            }
            return value.SequenceEqualsSafe(other);
        }

        /// <summary>
        /// Ensure length
        /// </summary>
        /// <param name="value"></param>
        /// <param name="requiredLength"></param>
        /// <returns></returns>
        internal static byte[] ForceLength(IReadOnlyCollection<byte> value, int requiredLength) {
            if (value == null || value.Count == 0) {
                return null;
            }
            var copy = value.ToArray();
            if (copy.Length == requiredLength) {
                return copy;
            }
            if (copy.Length < requiredLength) {
                var array = new byte[requiredLength];
                Array.Copy(copy, 0, array,
                    requiredLength - copy.Length, copy.Length);
                return array;
            }
            var num = copy.Length - requiredLength;
            for (var i = 0; i < num; i++) {
                if (copy[i] != 0) {
                    throw new ArgumentException($"Expected at most {requiredLength} " +
                        $"but found {copy.Length - i} bytes.");
                }
            }
            var array2 = new byte[requiredLength];
            Array.Copy(copy, copy.Length - requiredLength,
                array2, 0, requiredLength);
            return array2;
        }

    }
}