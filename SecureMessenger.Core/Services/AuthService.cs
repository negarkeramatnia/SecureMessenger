// In SecureMessenger.Core/Services/AuthService.cs
using SecureMessenger.Core.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureMessenger.Core.Services
{
    public class AuthService
    {
        private readonly CryptoService _cryptoService;

        public AuthService(CryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        // This method simulates registering a user. It returns the full User object
        // so you can see all the generated data. In a real app, you would save this to a database.
        public User RegisterUser(string username, string password)
        {
            // In a real app, you would first check if the username already exists in your database.

            byte[] salt = _cryptoService.GenerateSalt();
            string passwordHash = _cryptoService.HashPassword(password, salt);
            var (publicKey, privateKeyRaw) = _cryptoService.GenerateRsaKeyPair();

            // Encrypt the private key using a key derived from the user's password
            byte[] privateKeyEncryptionKey = _cryptoService.DeriveKeyFromPassword(password, salt);
            var (encryptedPkBytes, pkNonce, pkTag) = _cryptoService.EncryptWithAesGcm(privateKeyEncryptionKey, privateKeyRaw);

            // Clear sensitive keys from memory immediately after use
            Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            Array.Clear(privateKeyRaw, 0, privateKeyRaw.Length);

            return new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Salt = salt,
                PublicKey = publicKey,
                EncryptedPrivateKey = encryptedPkBytes,
                PrivateKeyNonce = pkNonce,
                PrivateKeyAuthTag = pkTag
            };
        }

        // This method simulates logging in a user. It takes the stored User object and the password.
        // It returns the decrypted private key on success, or null on failure.
        public byte[] Login(string password, User storedUser)
        {
            if (!_cryptoService.VerifyPassword(password, storedUser.Salt, storedUser.PasswordHash))
            {
                // Password does not match
                return null;
            }

            // Password is correct, now try to decrypt the private key
            byte[] privateKeyEncryptionKey = _cryptoService.DeriveKeyFromPassword(password, storedUser.Salt);
            try
            {
                byte[] decryptedPrivateKey = _cryptoService.DecryptWithAesGcm(
                    privateKeyEncryptionKey,
                    storedUser.PrivateKeyNonce,
                    storedUser.EncryptedPrivateKey,
                    storedUser.PrivateKeyAuthTag
                );
                return decryptedPrivateKey;
            }
            catch (CryptographicException)
            {
                // Decryption failed. This should not happen if the password was correct
                // and the data wasn't tampered with. It's a critical security failure.
                return null;
            }
            finally
            {
                // Always clear the key derivation key from memory
                Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            }
        }
    }
}