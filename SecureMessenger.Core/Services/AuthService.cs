// In SecureMessenger.Core/Services/AuthService.cs
using SecureMessenger.Core.Models;
using System;
using System.Security.Cryptography;

namespace SecureMessenger.Core.Services
{
    public class AuthService
    {
        private readonly CryptoService _cryptoService;
        private readonly UserDataService _userDataService; // Add this line

        public AuthService(CryptoService cryptoService)
        {
            _cryptoService = cryptoService;
            _userDataService = new UserDataService(); // Add this line
        }

        // The method signature changes to not return the User object, just a success/fail boolean
        public bool RegisterUser(string username, string password)
        {
            // REAL DATABASE CHECK: Check if the username already exists.
            if (_userDataService.UserExists(username))
            {
                return false; // Indicate that registration failed because the user exists
            }

            byte[] salt = _cryptoService.GenerateSalt();
            string passwordHash = _cryptoService.HashPassword(password, salt);
            var (publicKey, privateKeyRaw) = _cryptoService.GenerateRsaKeyPair();

            byte[] privateKeyEncryptionKey = _cryptoService.DeriveKeyFromPassword(password, salt);
            var (encryptedPkBytes, pkNonce, pkTag) = _cryptoService.EncryptWithAesGcm(privateKeyEncryptionKey, privateKeyRaw);

            Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            Array.Clear(privateKeyRaw, 0, privateKeyRaw.Length);

            // Create the user object to save to the database
            var userToSave = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Salt = salt,
                PublicKey = publicKey,
                EncryptedPrivateKey = encryptedPkBytes,
                PrivateKeyNonce = pkNonce,
                PrivateKeyAuthTag = pkTag
            };

            // Save the new user to the database
            return _userDataService.CreateUser(userToSave);
        }

        // The Login method now needs to fetch the user from the database first
        public byte[] Login(string username, string password)
        {
            // Fetch the user from the database
            var storedUser = _userDataService.GetUserByUsername(username);
            if (storedUser == null)
            {
                return null; // User not found
            }

            // The rest of the login logic remains the same!
            if (!_cryptoService.VerifyPassword(password, storedUser.Salt, storedUser.PasswordHash))
            {
                return null;
            }

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
                return null;
            }
            finally
            {
                Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            }
        }
    }
}