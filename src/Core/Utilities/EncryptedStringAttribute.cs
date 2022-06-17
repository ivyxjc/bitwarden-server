using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Bit.Core.Enums;

namespace Bit.Core.Utilities
{
    /// <summary>
    /// Validates a string that is in encrypted form: "head.b64iv=|b64ct=|b64mac="
    /// </summary>
    public class EncryptedStringAttribute : ValidationAttribute
    {
        private static readonly Dictionary<EncryptionType, int> _encryptionTypeRules = new()
        {
            { EncryptionType.AesCbc256_B64, 1 },
            { EncryptionType.AesCbc128_HmacSha256_B64, 3 },
            { EncryptionType.AesCbc256_HmacSha256_B64, 3 },
            { EncryptionType.Rsa2048_OaepSha256_B64, 1 },
            { EncryptionType.Rsa2048_OaepSha1_B64, 1 },
            { EncryptionType.Rsa2048_OaepSha256_HmacSha256_B64, 2 },
            { EncryptionType.Rsa2048_OaepSha1_HmacSha256_B64, 2 },
        };


        public EncryptedStringAttribute()
            : base("{0} is not a valid encrypted string.")
        { }

        public override bool IsValid(object value)
        {
            if (value is string stringVal)
            {
                return IsValidCore(stringVal);
            }
            else
            {
                return IsValidCore(value?.ToString());
            }

            if (value == null)
            {
                return true;
            }

            try
            {
                var encString = value.ToString();
                if (string.IsNullOrWhiteSpace(encString))
                {
                    return false;
                }

                var headerPieces = encString.Split('.');
                string[] encStringPieces = null;
                var encType = Enums.EncryptionType.AesCbc256_B64;

                if (headerPieces.Length == 1)
                {
                    encStringPieces = headerPieces[0].Split('|');
                    if (encStringPieces.Length == 3)
                    {
                        encType = Enums.EncryptionType.AesCbc128_HmacSha256_B64;
                    }
                    else
                    {
                        encType = Enums.EncryptionType.AesCbc256_B64;
                    }
                }
                else if (headerPieces.Length == 2)
                {
                    encStringPieces = headerPieces[1].Split('|');
                    if (!Enum.TryParse(headerPieces[0], out encType))
                    {
                        return false;
                    }
                }

                switch (encType)
                {
                    case Enums.EncryptionType.AesCbc256_B64:
                    case Enums.EncryptionType.Rsa2048_OaepSha1_HmacSha256_B64:
                    case Enums.EncryptionType.Rsa2048_OaepSha256_HmacSha256_B64:
                        if (encStringPieces.Length != 2)
                        {
                            return false;
                        }
                        break;
                    case Enums.EncryptionType.AesCbc128_HmacSha256_B64:
                    case Enums.EncryptionType.AesCbc256_HmacSha256_B64:
                        if (encStringPieces.Length != 3)
                        {
                            return false;
                        }
                        break;
                    case Enums.EncryptionType.Rsa2048_OaepSha256_B64:
                    case Enums.EncryptionType.Rsa2048_OaepSha1_B64:
                        if (encStringPieces.Length != 1)
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }

                switch (encType)
                {
                    case Enums.EncryptionType.AesCbc256_B64:
                    case Enums.EncryptionType.AesCbc128_HmacSha256_B64:
                    case Enums.EncryptionType.AesCbc256_HmacSha256_B64:
                        var iv = Convert.FromBase64String(encStringPieces[0]);
                        var ct = Convert.FromBase64String(encStringPieces[1]);
                        if (iv.Length < 1 || ct.Length < 1)
                        {
                            return false;
                        }

                        if (encType == Enums.EncryptionType.AesCbc128_HmacSha256_B64 ||
                            encType == Enums.EncryptionType.AesCbc256_HmacSha256_B64)
                        {
                            var mac = Convert.FromBase64String(encStringPieces[2]);
                            if (mac.Length < 1)
                            {
                                return false;
                            }
                        }

                        break;
                    case Enums.EncryptionType.Rsa2048_OaepSha256_B64:
                    case Enums.EncryptionType.Rsa2048_OaepSha1_B64:
                    case Enums.EncryptionType.Rsa2048_OaepSha1_HmacSha256_B64:
                    case Enums.EncryptionType.Rsa2048_OaepSha256_HmacSha256_B64:
                        var rsaCt = Convert.FromBase64String(encStringPieces[0]);
                        if (rsaCt.Length < 1)
                        {
                            return false;
                        }

                        if (encType == Enums.EncryptionType.Rsa2048_OaepSha1_HmacSha256_B64 ||
                            encType == Enums.EncryptionType.Rsa2048_OaepSha256_HmacSha256_B64)
                        {
                            var mac = Convert.FromBase64String(encStringPieces[1]);
                            if (mac.Length < 1)
                            {
                                return false;
                            }
                        }

                        break;
                    default:
                        throw new InvalidOperationException("This should be unreachable based on the previous switch statement");
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        internal static bool IsValidCore(ReadOnlySpan<char> value)
        {
            if (!value.TrySplitBy('.', out var head, out var rest))
            {
                // We could not get a header value of the encryption type
                // This is our slow path because we have to evaluate the amount of parts it has
                // TODO: Implement
            }

            EncryptionType encryptionType;
            if (!byte.TryParse(head, out var encryptionTypeByte))
            {
                // Cannot read it as a number attempt to read it as a string
                // This is our slow path
                if (!Enum.TryParse(head.ToString(), out encryptionType))
                {
                    return false;
                }
            }
            else
            {
                // Just cast to the enum and we validate it's a valid enum by if it exists in the dictionary or not
                encryptionType = (EncryptionType)encryptionTypeByte;
            }

            if (!_encryptionTypeRules.TryGetValue(encryptionType, out var requiredSections))
            {
                
            }
        }
    }
}
