using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Epsilon.Utils.Security
{
    public static class SecurityHelper
    {

        public static string Md5Hash(string salt, string strChange)
        {
            //Change the syllable into UTF8 code
            byte[] pass = Encoding.UTF8.GetBytes(salt + strChange);
            return Encoding.UTF8.GetString(MD5.Create().ComputeHash(pass));
        }

        public static string Md5Hash64(string salt, string strChange)
        {
            //Change the syllable into UTF8 code
            byte[] pass = Encoding.UTF8.GetBytes(salt + strChange);
            return Convert.ToBase64String(MD5.Create().ComputeHash(pass));
        }

        ///  <summary>
        ///  Generates a repeatable hash key based on the contents of the source string and then uses 
        ///  this key to produce an even shorter string from a set of available characters.  The maximum 
        ///  size of the key is constrained by the size of the specified hash prime * 256 (ASCII characters) 
        ///  + 1.  For a prime of 390001, the maximum key is 99840257.  This is then further constrained by 
        ///  the number of characters provided for use in the final hash string.  Assuming you use all the 
        ///  letters (upper and lower) and numbers (0-9), there will be 62 total characters available.  
        ///  The key will be successively integer divided by the number of available characters until 0 
        ///  remains, producing one character during each interval for the hash string.  For the maximum 
        ///  hash key example just given this will result in 5 intervals, so at most you'll get a hash string 
        ///  with a length of 5 characters.  At the other end, the minimum size of the key is 1 * 1 (smallest 
        ///  ASCII character) + 1, which will not last many iterations of division by the number of characters 
        ///  available for the hash (just 2 in the case just given).  So, the key is seeded with a number 
        ///  larger than the prime to ensure that even for a single character input string of ASCII character 
        ///  1, there will still be at least 3 characters in the final hash string.
        /// 
        ///  There's more to it than that, but too much to describe here.  Note that the implementation
        ///  below, with a 5 length final hash string consisting of up to 62 possible values, can yield up to
        ///  916,132,832 unique hash strings.  The chances of two different source strings resulting in the
        ///  same hash key/string is slim but, obviously, not impossible.
        /// 
        ///  The available hash characters are scrambled to avoid presenting a pattern.  The hash seed, prime,
        ///  and scrambled character set must be shared between implementations wanting to verify each other.
        ///  </summary>
        ///  <param name="sourceText"></param>
        /// <param name="seed"></param>
        /// <param name="prime"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string HashKeyBased(string sourceText, int seed, int prime, string chars)
        {
            if (string.IsNullOrEmpty(sourceText)) return null;

            string strHash = null;
            var lngHashKey = seed;

            var sourceChars = sourceText.ToCharArray();
            lngHashKey = sourceChars.Aggregate(lngHashKey, (current, t) => ((current % prime) * t) + 1);

            do
            {
                strHash = strHash + chars.Substring(lngHashKey % chars.Length, 1);
                lngHashKey = lngHashKey / chars.Length;
            } while (lngHashKey > 0);

            return strHash;
        }


    }
    
}
