using System.Security.Cryptography;
using System.Text;

namespace MyAuthApi.Models
{
    public class RsaHelper
    {
        private static readonly RSA _rsa;

        static RsaHelper()
        {
            _rsa = RSA.Create();

            // Try to load keys from file
            if (File.Exists("privateKey.xml") && File.Exists("publicKey.xml"))
            {
                // Load the keys from file
                var privateKeyXml = File.ReadAllText("privateKey.xml");
                var publicKeyXml = File.ReadAllText("publicKey.xml");
                _rsa.FromXmlString(privateKeyXml);
            }
            else
            {
                // Generate new keys and save them
                _rsa.KeySize = 2048;
                SaveKeys();
            }
        }

        // Method to save the keys to files
        private static void SaveKeys()
        {
            var privateKeyXml = _rsa.ToXmlString(true);  // Export private key
            var publicKeyXml = _rsa.ToXmlString(false);  // Export public key

            File.WriteAllText("privateKey.xml", privateKeyXml);  // Save private key to a file
            File.WriteAllText("publicKey.xml", publicKeyXml);    // Save public key to a file
        }

        // Encrypt data using the public key
        public static string Encrypt(string plainText)
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            var encryptedData = _rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedData);
        }

        // Decrypt data using the private key
        public static string Decrypt(string encryptedText)
        {
            var encryptedData = Convert.FromBase64String(encryptedText);
            var decryptedData = _rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedData);
        }

        // Get public key for encryption
        public static string GetPublicKey()
        {
            return Convert.ToBase64String(_rsa.ExportRSAPublicKey());
        }
    }
}
