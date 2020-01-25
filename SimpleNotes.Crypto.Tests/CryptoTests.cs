using System;
using SimpleNotes.Crypto;
using Xunit;

namespace SimpleNotes.Crypto.Tests {
    public class CryptoTests {

        [Fact]
        public void DeriveKeyForPassowrd() {
            string password = "Password_1";
            string key = "aLYGMYOcgI5WK/X8PAp8L6K/xcH3tj5vbIYcvdhoOtw=";
            string output = Crypto.DeriveKey(password);
            Assert.Equal(output, key);

        }

        [Fact]
        public void EncryptDecrypt() {
            string key = "aLYGMYOcgI5WK/X8PAp8L6K/xcH3tj5vbIYcvdhoOtw=";
            string plainText = "Encrypt this string, please";
            string cipherText = Crypto.Encrypt(key,plainText);
            string output = Crypto.Decrypt(key, cipherText);
            Assert.Equal(output, plainText);
        }

    }
}
