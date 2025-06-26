using System;
using System.Security.Cryptography;

namespace SecureMessenger.Core.Services
{
    public class CryptoService : IDisposable
    {
        // Constants for password hashing and AES encryption
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Pbkdf2Iterations = 350000;
        private const int AesKeySize = 32;
        private const int AesNonceSize = 12;
        private const int AesTagSize = 16;

        // --- IDENTITY KEYS (ECDsa for signing) ---
        public (byte[] publicKey, byte[] privateKey) GenerateIdentityKeyPair()
        {
            // Use nistP384 for strong, widely supported identity signatures.
            using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                return (ecdsa.ExportSubjectPublicKeyInfo(), ecdsa.ExportECPrivateKey());
            }
        }

        public byte[] SignData(byte[] privateIdentityKey, byte[] data)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportECPrivateKey(privateIdentityKey, out _);
                return ecdsa.SignData(data, HashAlgorithmName.SHA384);
            }
        }

        public bool VerifySignature(byte[] publicIdentityKey, byte[] data, byte[] signature)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportSubjectPublicKeyInfo(publicIdentityKey, out _);
                return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA384);
            }
        }

        // --- EPHEMERAL KEYS (ECDH for key exchange / PFS) ---
        public (byte[] publicKey, byte[] privateKey) GenerateEphemeralKeyPair()
        {
            // Use the same curve for identity and ephemeral keys for simplicity.
            using (var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP384))
            {
                // Exporting the public key in a standard format is crucial for compatibility.
                return (ecdh.PublicKey.ExportSubjectPublicKeyInfo(), ecdh.ExportECPrivateKey());
            }
        }

        public byte[] DeriveSharedSecret(byte[] privateEphemeralKey, byte[] publicEphemeralKey)
        {
            using (var ecdh = ECDiffieHellman.Create())
            {
                ecdh.ImportECPrivateKey(privateEphemeralKey, out _);

                // Import the other party's public key
                using (var theirPublicKey = ECDiffieHellmanCng.Create()) // Use Cng for robust import
                {
                    theirPublicKey.ImportSubjectPublicKeyInfo(publicEphemeralKey, out _);
                    // Derive a shared secret and use a standard KDF (HKDF is built-in) to generate a stable AES key.
                    return ecdh.DeriveKeyFromHash(theirPublicKey.PublicKey, HashAlgorithmName.SHA256, null, null);
                }
            }
        }

        // --- PASSWORD HASHING ---
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

        // --- SYMMETRIC ENCRYPTION (AES-GCM) ---
        public byte[] EncryptWithAesGcm(byte[] key, byte[] plaintext)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(AesNonceSize);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[AesTagSize];
            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
            }

            // Combine nonce, tag, and ciphertext into a single payload for easier storage.
            // Format: [12-byte Nonce][16-byte AuthTag][Ciphertext]
            byte[] encryptedPayload = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, encryptedPayload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, encryptedPayload, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, encryptedPayload, nonce.Length + tag.Length, ciphertext.Length);
            return encryptedPayload;
        }

        public byte[] DecryptWithAesGcm(byte[] key, byte[] encryptedPayload)
        {
            if (encryptedPayload.Length < AesNonceSize + AesTagSize)
            {
                throw new CryptographicException("Invalid encrypted payload.");
            }

            // Extract nonce, tag, and ciphertext from the combined payload.
            byte[] nonce = new byte[AesNonceSize];
            byte[] tag = new byte[AesTagSize];
            byte[] ciphertext = new byte[encryptedPayload.Length - AesNonceSize - AesTagSize];

            Buffer.BlockCopy(encryptedPayload, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptedPayload, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(encryptedPayload, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

            byte[] plaintext = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }
            return plaintext;
        }

        public void Dispose() { }
    }
}