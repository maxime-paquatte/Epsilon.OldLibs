using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Epsilon.Utils.Security
{
    /// <summary>
    /// http://www.codeproject.com/Articles/14497/Data-Encryption-Decryption-using-RijndaelManaged-a
    /// </summary>
    public sealed class Cryption
    {
        private string _key, _salt;


        public Cryption(string key, string salt)
        {
            _key = key;
            _salt = salt;
        }


        /// <summary>
        /// Method which does the encryption using Rijndeal algorithm
        /// </summary>
        /// <param name="input">Data to be encrypted</param>
        /// <returns>Encrypted Data</returns>
        public byte[] Encrypt(byte[] input)
        {
            //This class uses an extension of the PBKDF1 algorithm defined in the PKCS#5 v2.0 
            //standard to derive bytes suitable for use as key material from a password. 
            //The standard is documented in IETF RRC 2898.
            var secretKey = new PasswordDeriveBytes(_key, Encoding.ASCII.GetBytes(_salt));



            //Creates a symmetric encryptor object. 
            ICryptoTransform Encryptor = Aes.Create().CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));

            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(input, 0, input.Length);
                //Writes the final state and clears the buffer
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Method which does the encryption using Rijndeal algorithm.This is for decrypting the data
        /// which has orginally being encrypted using the above method
        /// </summary>
        /// <param name="InputText">The encrypted data which has to be decrypted</param>
        /// <returns>Decrypted Data</returns>
        public byte[] Decrypt(byte[] input)
        {
            //This class uses an extension of the PBKDF1 algorithm defined in the PKCS#5 v2.0 
            //standard to derive bytes suitable for use as key material from a password. 
            //The standard is documented in IETF RRC 2898.
            var secretKey = new PasswordDeriveBytes(_key, Encoding.ASCII.GetBytes(_salt));

            //Creates a symmetric Rijndael decryptor object.
            ICryptoTransform Decryptor = Aes.Create().CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
            using (MemoryStream inputMs = new MemoryStream(input))
            using (CryptoStream cryptoStream = new CryptoStream(inputMs, Decryptor, CryptoStreamMode.Read))
            using (MemoryStream outputMs = new MemoryStream())
            {
                cryptoStream.CopyTo(outputMs);
                return outputMs.ToArray();
            }
        }


    }

    public static class CryptionExt
    {
        public static string Encrypt(this Cryption @this, string text)
        {
            byte[] input = Encoding.UTF8.GetBytes(text);
            byte[] output = @this.Encrypt(input);
            return Convert.ToBase64String(output);
        }

        public static string Decrypt(this Cryption @this, string text)
        {
            byte[] input = Convert.FromBase64String(text);
            byte[] output = @this.Decrypt(input);
            return Encoding.UTF8.GetString(output);
        }

        
        public static string SerializeObject<T>(this Cryption @this, T o)
        {
            if (!typeof(T).IsSerializable) return null;

            using (MemoryStream stream = new MemoryStream())
            {
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, o);
                return Convert.ToBase64String( @this.Encrypt(stream.ToArray()));
            }
        }

        public static T DeserializeObject<T>(this Cryption @this, string str)
        {
            try
            {
                byte[] bytes = @this.Decrypt(Convert.FromBase64String(str));
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    return (T) new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Deserialize: " + str, ex);
            }
        }
        
        /*
<PropertyGroup>
  <TargetFramework>net5.0</TargetFramework>
  <!-- Warning: Setting the following switch is *NOT* recommended in web apps. -->
  <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
</PropertyGroup>
         * 
         */
    }
}
