// In SecureMessenger.Core/Services/CryptoService.cs
using System;
using System.Security.Cryptography;

namespace SecureMessenger.Core.Services
{
    public class CryptoService : IDisposable
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Pbkdf2Iterations = 350000;
        private const int AesKeySize = 32;
        private const int AesNonceSize = 12;
        private const int AesTagSize = 16;

        public byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(SaltSize);

        public string HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            }
        }

        public bool VerifyPassword(string password, byte[] salt, string storedPasswordHash)
        {
            byte[] hashToVerify = Convert.FromBase64String(storedPasswordHash);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);
                return CryptographicOperations.FixedTimeEquals(computedHash, hashToVerify);
            }
        }

        public byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(AesKeySize);
            }
        }

        public (byte[] publicKey, byte[] privateKey) GenerateRsaKeyPair(int keySizeInBits = 4096)
        {
            using (var rsa = RSA.Create(keySizeInBits))
            {
                return (rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());
            }
        }

        public (byte[] ciphertext, byte[] nonce, byte[] tag) EncryptWithAesGcm(byte[] key, byte[] plaintext)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(AesNonceSize);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[AesTagSize];
            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
            }
            return (ciphertext, nonce, tag);
        }

        public byte[] DecryptWithAesGcm(byte[] key, byte[] nonce, byte[] ciphertext, byte[] tag)
        {
            byte[] plaintext = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }
            return plaintext;
        }

        public byte[] RsaEncrypt(byte[] rsaPublicKeyBytes, byte[] dataToEncrypt)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(rsaPublicKeyBytes, out _);
                return rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA256);
            }
        }

        public byte[] RsaDecrypt(byte[] rsaPrivateKeyBytes, byte[] dataToDecrypt)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(rsaPrivateKeyBytes, out _);
                return rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.OaepSHA256);
            }
        }

        public byte[] GenerateSymmetricKey() => RandomNumberGenerator.GetBytes(AesKeySize);
        public void Dispose() { }
    }
}