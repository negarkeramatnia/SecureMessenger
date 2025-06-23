// In SecureMessenger.Core/Services/MessageService.cs
using SecureMessenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace SecureMessenger.Core.Services
{
    public class MessageService
    {
        private readonly CryptoService _cryptoService;
        private readonly UserDataService _userDataService;
        private readonly MessageDataService _messageDataService;

        // Pass the AuthService instance in the constructor
        public MessageService()
        {
            _cryptoService = new CryptoService();
            _userDataService = new UserDataService();
            _messageDataService = new MessageDataService();
        }

        // --- MODIFIED: SendMessage no longer needs the private key passed to it ---
        public bool SendMessage(string senderUsername, string recipientUsername, string plaintextMessage, AuthService authService)
        {
            var senderUser = _userDataService.GetUserByUsername(senderUsername);
            var recipientUser = _userDataService.GetUserByUsername(recipientUsername);

            if (senderUser == null || recipientUser == null) return false;

            try
            {
                FileLogger.Log($"--- SENDING from {senderUsername} to {recipientUsername} ---");
                FileLogger.Log($"[SEND] Encrypting for RECIPIENT ({recipientUser.Username}) using PubKey: {Convert.ToBase64String(recipientUser.PublicKey).Substring(0, 10)}...");
                FileLogger.Log($"[SEND] Encrypting for SENDER ({senderUser.Username}) using PubKey: {Convert.ToBase64String(senderUser.PublicKey).Substring(0, 10)}...");

                byte[] messageKey = _cryptoService.GenerateSymmetricKey();
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintextMessage);
                var (ciphertext, nonce, tag) = _cryptoService.EncryptWithAesGcm(messageKey, plaintextBytes);

                byte[] encryptedKeyForRecipient = _cryptoService.RsaEncrypt(recipientUser.PublicKey, messageKey);
                byte[] encryptedKeyForSender = _cryptoService.RsaEncrypt(senderUser.PublicKey, messageKey);

                Array.Clear(messageKey, 0, messageKey.Length);

                var messageToStore = new Message
                {
                    SenderUsername = senderUsername,
                    RecipientUsername = recipientUsername,
                    Ciphertext = ciphertext,
                    Nonce = nonce,
                    AuthTag = tag,
                    EncryptedMessageKeyForSender = encryptedKeyForSender,
                    EncryptedMessageKeyForRecipient = encryptedKeyForRecipient,
                    Timestamp = DateTime.UtcNow
                };

                return _messageDataService.CreateMessage(messageToStore);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"[SEND] FAILED with exception: {ex.Message}");
                return false;
            }
        }

        // --- MODIFIED: DecryptMessage no longer needs the private key passed to it ---
        public string DecryptMessage(Message message, string currentUsername, AuthService authService)
        {
            FileLogger.Log($"--- DECRYPTING message for {currentUsername} ---");
            var currentUser = _userDataService.GetUserByUsername(currentUsername);
            if (currentUser != null)
            {
                FileLogger.Log($"[DECRYPT] Using private key that corresponds to PubKey: {Convert.ToBase64String(currentUser.PublicKey).Substring(0, 10)}...");
            }

            byte[] currentUserPrivateKey = authService.GetCurrentUserPrivateKey();
            if (currentUserPrivateKey == null)
            {
                FileLogger.Log("[DECRYPT] FAILED: Current user's private key is not available in AuthService.");
                return "[Decryption Error: User session invalid]";
            }

            byte[] encryptedMessageKeyToUse;
            if (message.SenderUsername == currentUsername)
            {
                encryptedMessageKeyToUse = message.EncryptedMessageKeyForSender;
            }
            else if (message.RecipientUsername == currentUsername)
            {
                encryptedMessageKeyToUse = message.EncryptedMessageKeyForRecipient;
            }
            else
            {
                throw new SecurityException("User is not authorized to decrypt this message.");
            }

            try
            {
                byte[] messageKey = _cryptoService.RsaDecrypt(currentUserPrivateKey, encryptedMessageKeyToUse);
                byte[] plaintextBytes = _cryptoService.DecryptWithAesGcm(messageKey, message.Nonce, message.Ciphertext, message.AuthTag);

                Array.Clear(messageKey, 0, messageKey.Length);
                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch (CryptographicException ex)
            {
                FileLogger.Log($"[DECRYPT] FAILED with CryptographicException: {ex.Message}");
                return "[Unable to decrypt message - data may be corrupt]";
            }
        }

        public List<Message> GetConversation(string user1, string user2)
        {
            return _messageDataService.GetConversation(user1, user2);
        }
    }
}