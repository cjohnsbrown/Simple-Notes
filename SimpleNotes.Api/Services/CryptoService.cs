using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleNotes.Api.Services {
    public interface ICryptoService {

        /// <summary>
        /// Key used for retrieving user's derived key stored in the session
        /// </summary>
        public const string UserKey = "UserKey";

        /// <summary>
        /// Decrypt a stirng using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cypherText"></param>
        /// <returns></returns>
        string Decrypt(string key, string cypherText);

        /// <summary>
        /// Generate the key for encrypting/decrpyting data
        /// </summary>
        /// <param name="password">The password to derive the key from</param>
        /// <returns>Decrpyted plain text</returns>
        string DeriveKey(string password);

        /// <summary>
        /// Encrypt a string using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plainText"></param>
        /// <returns>Ciphertext</returns>
        string Encrypt(string key, string plainText);

        /// <summary>
        /// Generate a random 256-bit string
        /// </summary>
        string RandomString();
    }

    public class CryptoService : ICryptoService {

        private const int IV_Length = 16;

        public string DeriveKey(string password) {
            byte[] hash = Encoding.UTF8.GetBytes(password);

            using (var hasher = SHA256.Create()) {
                for (int i = 0; i < 1000; i++) {
                    hash = hasher.ComputeHash(hash);
                }
            }

            return Convert.ToBase64String(hash);
        }


        public string RandomString() {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        public string Encrypt(string key, string plainText) {
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


        public string Decrypt(string key, string cypherText) {
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
