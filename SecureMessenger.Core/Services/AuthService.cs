using SecureMessenger.Core.Models;
using System;
using System.Security.Cryptography;

namespace SecureMessenger.Core.Services
{
    public class AuthService
    {
        private byte[] _currentUserDecryptedPrivateKey;

        private readonly CryptoService _cryptoService;
        private readonly UserDataService _userDataService;

        public AuthService(CryptoService cryptoService)
        {
            _cryptoService = cryptoService;
            _userDataService = new UserDataService();
        }

        public User RegisterUser(string username, string password)
        {
            if (_userDataService.UserExists(username))
            {
                FileLogger.Log($"[REGISTER] Warning: User '{username}' already exists.");
                return null;
            }

            byte[] salt = _cryptoService.GenerateSalt();
            string passwordHash = _cryptoService.HashPassword(password, salt);
            var (publicKey, privateKeyRaw) = _cryptoService.GenerateRsaKeyPair();

            FileLogger.Log($"[REGISTER] Generated Public Key for {username} starts with: {Convert.ToBase64String(publicKey).Substring(0, 10)}...");

            byte[] privateKeyEncryptionKey = _cryptoService.DeriveKeyFromPassword(password, salt);
            var (encryptedPkBytes, pkNonce, pkTag) = _cryptoService.EncryptWithAesGcm(privateKeyEncryptionKey, privateKeyRaw);

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

        public bool Login(string password, User storedUser)
        {
            if (!_cryptoService.VerifyPassword(password, storedUser.Salt, storedUser.PasswordHash))
            {
                FileLogger.Log($"[LOGIN] FAILED for user '{storedUser.Username}' due to wrong password.");
                return false;
            }

            byte[] privateKeyEncryptionKey = _cryptoService.DeriveKeyFromPassword(password, storedUser.Salt);
            try
            {
                _currentUserDecryptedPrivateKey = _cryptoService.DecryptWithAesGcm(
                    privateKeyEncryptionKey,
                    storedUser.PrivateKeyNonce,
                    storedUser.EncryptedPrivateKey,
                    storedUser.PrivateKeyAuthTag
                );

                FileLogger.Log($"[LOGIN] User '{storedUser.Username}' logged in. Their Public Key starts with: {Convert.ToBase64String(storedUser.PublicKey).Substring(0, 10)}...");
                return true;
            }
            catch (CryptographicException ex)
            {
                FileLogger.Log($"[LOGIN] FAILED for user '{storedUser.Username}'. Private key decryption failed: {ex.Message}");
                return false;
            }
            finally
            {
                Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            }
        }

        public byte[] GetCurrentUserPrivateKey()
        {
            return _currentUserDecryptedPrivateKey;
        }

        public void Logout()
        {
            if (_currentUserDecryptedPrivateKey != null)
            {
                Array.Clear(_currentUserDecryptedPrivateKey, 0, _currentUserDecryptedPrivateKey.Length);
                _currentUserDecryptedPrivateKey = null;
            }
        }
    }
}