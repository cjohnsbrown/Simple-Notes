using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace SimpleNotes.Cryptography {

    /// <summary>
    /// Class used for encypting/decrpying data stored in the database
    /// </summary>
    public static class Crypto {

        public const string UserKey = "UserKey";

        private const int IV_Length = 16;



        /// <summary>
        /// Generate the key for encrypting/decrpyting data
        /// </summary>
        /// <param name="password">The password to derive the key from</param>
        /// <returns></returns>
        public static string DeriveKey(string password) {
            byte[] hash;
            using (var hasher = SHA256.Create()) {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generate a random 256-bit string
        /// </summary>
        /// <returns></returns>
        public static string RandomString() {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Encrypt a string using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Encrypt(string key, string plainText) {
            byte[] cipherText;
            byte[] iv = new byte[IV_Length];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(iv);
            }

            using (var aes = Aes.Create()) {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    byte[] encrypted = msEncrypt.ToArray();
                    cipherText = new byte[iv.Length + encrypted.Length];
                    iv.CopyTo(cipherText, 0);
                    encrypted.CopyTo(cipherText, iv.Length);
                }
            }

            return Convert.ToBase64String(cipherText);
        }


        /// <summary>
        /// Decrypt a stirng using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cypherText"></param>
        /// <returns></returns>
        public static string Decrypt(string key, string cypherText) {
            string plainText;
            byte[] cypherBytes = Convert.FromBase64String(cypherText);
            byte[] iv = cypherBytes.Take(IV_Length).ToArray();
            byte[] encrypted = cypherBytes.Skip(IV_Length).ToArray();

            using (var aes = Aes.Create()) {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream msDecrypt = new MemoryStream(encrypted)) {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plainText;
        }
    }
}
