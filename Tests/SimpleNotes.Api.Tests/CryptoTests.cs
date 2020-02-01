using SimpleNotes.Api.Services;
using Xunit;

namespace SimpleNotes.Api.Tests {
    public class CryptoTests {

        [Fact]
        public void DeriveKeyForPassowrd() {
            CryptoService crypto = new CryptoService();
            string password = "Password_1";
            string key = "/aExOhfLY8QV11tghX5NpRXydJUyIu6kPWwO+mwyMnU=";
            string output = crypto.DeriveKey(password);
            Assert.Equal(output, key);

        }

        [Fact]
        public void EncryptDecrypt() {
            CryptoService crypto = new CryptoService();
            string key = "aLYGMYOcgI5WK/X8PAp8L6K/xcH3tj5vbIYcvdhoOtw=";
            string plainText = "Encrypt this string, please";
            string cipherText = crypto.Encrypt(key,plainText);
            string output = crypto.Decrypt(key, cipherText);
            Assert.Equal(output, plainText);
        }

    }
}
