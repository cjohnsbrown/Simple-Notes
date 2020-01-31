using System;
using SimpleNotes.Cryptography;
using Xunit;

namespace SimpleNotes.Cryptography.Tests {
    public class CryptoTests {

        [Fact]
        public void DeriveKeyForPassowrd() {
            string password = "Password_1";
            string key = "/aExOhfLY8QV11tghX5NpRXydJUyIu6kPWwO+mwyMnU=";
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
