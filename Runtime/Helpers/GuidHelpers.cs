using System;

namespace UnityHelpers
{
    public static class GuidHelpers
    {
        /// <summary>
        /// Takes a guid value and encodes it to a shorter url friendly string
        /// Source: https://www.madskristensen.net/blog/A-shorter-and-URL-friendly-GUID
        /// </summary>
        /// <param name="guid">The original guid value</param>
        /// <returns>The encoded string</returns>
        public static string Encode(this Guid guid)
        {
            string enc = Convert.ToBase64String(guid.ToByteArray());
            enc = enc.Replace("/", "_");
            enc = enc.Replace("+", "-");
            return enc.Substring(0, 22);
        }

        /// <summary>
        /// Takes a previously encoded string and decodes it to the original guid value
        /// Source: https://www.madskristensen.net/blog/A-shorter-and-URL-friendly-GUID
        /// </summary>
        /// <param name="encoded">The encoded string</param>
        /// <returns>The original guid</returns>
        public static Guid DecodeToGuid(this string encoded)
        {
            encoded = encoded.Replace("_", "/");
            encoded = encoded.Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(encoded + "==");
            return new Guid(buffer);
        }
    }
}