using System;
using System.Security.Cryptography;
using System.Text;

namespace Epsilon.Utils.Security;

public class Hmac
{
    public static string ComputeHmac(byte[] privateKey, string sData)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(sData);
        
        HMACSHA1 hmac = new HMACSHA1(privateKey);
        hmac.Initialize();
        byte[] ba = hmac.ComputeHash(bytes);

        return Convert.ToHexString(ba);
    }

}