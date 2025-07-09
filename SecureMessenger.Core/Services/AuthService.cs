using SecureMessenger.Core.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureMessenger.Core.Services
{
    public class AuthService
    {
        // holds the current user's decrypted private key in memory.
        private byte[] _currentUserDecryptedPrivateKey;

        //referenceS
        private readonly CryptoService _cryptoService;
        //handle saving and loading user data
        private readonly UserDataService _userDataService;

        //method that runs once when a new instance of AuthService is created
        public AuthService(CryptoService cryptoService)
        {
            _cryptoService = cryptoService;
            //creates a new instance of the UserDataService for talkin
            _userDataService = new UserDataService();
        }

        public User RegisterUser(string username, string password)
        {
            if (_userDataService.UserExists(username))
            {
                return null;
            }

            byte[] salt = _cryptoService.GenerateSalt();
            string passwordHash = _cryptoService.HashPassword(password, salt);

            //private key encryption
            //public and private RSA key pair for the user for encrypting messages
            var (publicKey, privateKeyRaw) = _cryptoService.GenerateRsaKeyPair();
            //temporary encryption key directly from the user's password and salt
            byte[] privateKeyEncryptionKey = _cryptoService.DeriveKeyFromPassword(password, salt);

            //encrypt the private key using AES with the derived key
            var (encryptedPkBytes, pkNonce, pkTag) = _cryptoService.EncryptWithAesGcm(privateKeyEncryptionKey, privateKeyRaw);

            Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            Array.Clear(privateKeyRaw, 0, privateKeyRaw.Length);
            //new User object
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
            //entered password =? hash stored in the database
            if (!_cryptoService.VerifyPassword(password, storedUser.Salt, storedUser.PasswordHash))
            {
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

                return true;
            }
            catch (CryptographicException ex)
            {
                return false;
            }
            finally
            {
                Array.Clear(privateKeyEncryptionKey, 0, privateKeyEncryptionKey.Length);
            }
        }

        //get the private key
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