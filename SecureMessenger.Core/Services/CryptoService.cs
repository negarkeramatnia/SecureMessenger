using System;
using System.Security.Cryptography;

namespace SecureMessenger.Core.Services
{
    public class CryptoService
    {
        //32 byte = 256 bits
        private const int SaltSize = 32;
        private const int HashSize = 32;
        //sets the number of times to run the password hashing algorithm
        private const int Pbkdf2Iterations = 350000;
        //symmetric encryption key
        private const int AesKeySize = 32;
        private const int AesNonceSize = 12;
        private const int AesTagSize = 16;

        public byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(SaltSize);

        public string HashPassword(string password, byte[] salt)
        {
            //it creates a new instance of the PBKDF2 algorithm
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                //can be stored in the database
                return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            }
        }

        public bool VerifyPassword(string password, byte[] salt, string storedPasswordHash)
        {
            byte[] hashToVerify = Convert.FromBase64String(storedPasswordHash);
            //it creates a new hash from the password the user just typed
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);
                //it returns true if they match, false if not
                return CryptographicOperations.FixedTimeEquals(computedHash, hashToVerify);
            }
        }

        //its main purpose is to create the key that will encrypt the user's private RSA key
        public byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(AesKeySize);
            }
        }
        //RSA - this is used for securely sharing keys

        public (byte[] publicKey, byte[] privateKey) GenerateRsaKeyPair(int keySizeInBits = 4096)
        {
            //creates a new RSA object with a very strong key size of 4096 bits.
            using (var rsa = RSA.Create(keySizeInBits))
            {
                //export to be stored
                return (rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());
            }
        }

        //encrypts the message text
        public (byte[] ciphertext, byte[] nonce, byte[] tag) EncryptWithAesGcm(byte[] key, byte[] plaintext)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(AesNonceSize);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[AesTagSize];
            //creates an instance of the AES with key
            using (var aesGcm = new AesGcm(key))
            {
                //encrypted text and the authentication tag
                aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
            }
            return (ciphertext, nonce, tag);
        }
        //decrypts the message text
        public byte[] DecryptWithAesGcm(byte[] key, byte[] nonce, byte[] ciphertext, byte[] tag)
        {
            byte[] plaintext = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(key))
            {
                //first check tag...
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }
            return plaintext;
        }

        public byte[] RsaEncrypt(byte[] rsaPublicKeyBytes, byte[] dataToEncrypt)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(rsaPublicKeyBytes, out _);
                //encrypts the data using OAEP padding
                return rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA256);
            }
        }
        //decrypts data using a user's PRIVATE key
        public byte[] RsaDecrypt(byte[] rsaPrivateKeyBytes, byte[] dataToDecrypt)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(rsaPrivateKeyBytes, out _);
                return rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.OaepSHA256);
            }
        }
        //for encrypting a single message
        public byte[] GenerateSymmetricKey() => RandomNumberGenerator.GetBytes(AesKeySize);
    }
}