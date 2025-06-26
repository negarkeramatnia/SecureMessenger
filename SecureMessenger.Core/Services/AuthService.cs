using SecureMessenger.Core.Models;
using System;
using System.Security.Cryptography;

namespace SecureMessenger.Core.Services
{
    public class AuthService
    {
        private byte[] _currentUserDecryptedIdentityKey;

        private readonly CryptoService _cryptoService;
        private readonly UserDataService _userDataService;

        public AuthService()
        {
            _cryptoService = new CryptoService();
            _userDataService = new UserDataService();
        }

        public bool RegisterUser(string username, string password)
        {
            if (_userDataService.UserExists(username))
            {
                return false; // User already exists
            }

            var salt = _cryptoService.GenerateSalt();
            var passwordHash = _cryptoService.HashPassword(password, salt);

            // Generate the new, more secure ECDsa identity keys
            var (publicKey, privateKeyRaw) = _cryptoService.GenerateIdentityKeyPair();

            var pkek = _cryptoService.DeriveKeyFromPassword(password, salt);
            byte[] encryptedPayload = _cryptoService.EncryptWithAesGcm(pkek, privateKeyRaw);

            // Manually split the payload into its parts for database storage
            const int AesNonceSize = 12;
            const int AesTagSize = 16;
            byte[] pkNonce = encryptedPayload.Take(AesNonceSize).ToArray();
            byte[] pkTag = encryptedPayload.Skip(AesNonceSize).Take(AesTagSize).ToArray();
            byte[] encryptedPkBytes = encryptedPayload.Skip(AesNonceSize + AesTagSize).ToArray();

            Array.Clear(pkek, 0, pkek.Length);
            Array.Clear(privateKeyRaw, 0, privateKeyRaw.Length);

            var newUser = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Salt = salt,
                IdentityPublicKey = publicKey,
                EncryptedIdentityKey = encryptedPkBytes,
                PrivateKeyNonce = pkNonce,
                PrivateKeyAuthTag = pkTag
            };

            return _userDataService.CreateUser(newUser);
        }

        public bool Login(string username, string password)
        {
            var storedUser = _userDataService.GetUserByUsername(username);
            if (storedUser == null) return false;

            if (!_cryptoService.VerifyPassword(password, storedUser.Salt, storedUser.PasswordHash))
            {
                return false;
            }

            var pkek = _cryptoService.DeriveKeyFromPassword(password, storedUser.Salt);
            try
            {
                byte[] encryptedPayload = new byte[storedUser.PrivateKeyNonce.Length + storedUser.PrivateKeyAuthTag.Length + storedUser.EncryptedIdentityKey.Length];
                Buffer.BlockCopy(storedUser.PrivateKeyNonce, 0, encryptedPayload, 0, storedUser.PrivateKeyNonce.Length);
                Buffer.BlockCopy(storedUser.PrivateKeyAuthTag, 0, encryptedPayload, storedUser.PrivateKeyNonce.Length, storedUser.PrivateKeyAuthTag.Length);
                Buffer.BlockCopy(storedUser.EncryptedIdentityKey, 0, encryptedPayload, storedUser.PrivateKeyNonce.Length + storedUser.PrivateKeyAuthTag.Length, storedUser.EncryptedIdentityKey.Length);

                // Now, decrypt the combined payload
                _currentUserDecryptedIdentityKey = _cryptoService.DecryptWithAesGcm(pkek, encryptedPayload);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            finally
            {
                Array.Clear(pkek, 0, pkek.Length);
            }
        }

        public byte[] GetCurrentUserPrivateKey() => _currentUserDecryptedIdentityKey;

        public void Logout()
        {
            if (_currentUserDecryptedIdentityKey != null)
            {
                Array.Clear(_currentUserDecryptedIdentityKey, 0, _currentUserDecryptedIdentityKey.Length);
                _currentUserDecryptedIdentityKey = null;
            }
        }
    }
}