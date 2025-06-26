using SecureMessenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SecureMessenger.Core.Services
{
    public class MessageService
    {
        private readonly CryptoService _cryptoService;
        private readonly UserDataService _userDataService;
        private readonly MessageDataService _messageDataService;

        public MessageService()
        {
            _cryptoService = new CryptoService();
            _userDataService = new UserDataService();
            _messageDataService = new MessageDataService();
        }

        public bool SendMessage(string senderUsername, string recipientUsername, string plaintextMessage, AuthService authService)
        {
            var senderUser = _userDataService.GetUserByUsername(senderUsername);
            var recipientUser = _userDataService.GetUserByUsername(recipientUsername);
            if (senderUser == null || recipientUser == null) return false;

            try
            {
                var (senderEphemeralPublic, senderEphemeralPrivate) = _cryptoService.GenerateEphemeralKeyPair();
                var sharedSecret = _cryptoService.DeriveSharedSecret(senderEphemeralPrivate, recipientUser.IdentityPublicKey);
                var ciphertext = _cryptoService.EncryptWithAesGcm(sharedSecret, Encoding.UTF8.GetBytes(plaintextMessage));

                Array.Clear(senderEphemeralPrivate, 0, senderEphemeralPrivate.Length);
                Array.Clear(sharedSecret, 0, sharedSecret.Length);

                var messageToStore = new Message
                {
                    RecipientUsername = recipientUsername,
                    SenderIdentityKey = senderUser.IdentityPublicKey,
                    SenderEphemeralKey = senderEphemeralPublic,
                    Ciphertext = ciphertext,
                    Timestamp = DateTime.UtcNow
                };

                return _messageDataService.CreateMessage(messageToStore);
            }
            catch (Exception) { return false; }
        }

        public string DecryptMessage(Message message, string currentUsername, AuthService authService)
        {
            var currentUser = _userDataService.GetUserByUsername(currentUsername);
            var currentUserPrivateKey = authService.GetCurrentUserPrivateKey();
            if (currentUser == null || currentUserPrivateKey == null) return "[Decryption Error: Invalid Session]";

            try
            {
                var sharedSecret = _cryptoService.DeriveSharedSecret(currentUserPrivateKey, message.SenderEphemeralKey);
                var plaintextBytes = _cryptoService.DecryptWithAesGcm(sharedSecret, message.Ciphertext);
                Array.Clear(sharedSecret, 0, sharedSecret.Length);
                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch (CryptographicException) { return "[Unable to decrypt message]"; }
        }

        public List<Message> GetMessagesForUser(string username)
        {
            return _messageDataService.GetMessagesForUser(username);
        }

        public bool EditMessage(int messageId, string newText, string currentUsername, AuthService authService)
        {
            return false; // Edit logic is disabled
        }

        public bool DeleteMessage(int messageId, string currentUsername)
        {
            var messageToDelete = _messageDataService.GetMessageById(messageId);
            var senderUsername = GetSenderUsernameFromMessage(messageToDelete);

            if (messageToDelete != null && senderUsername == currentUsername)
            {
                _messageDataService.DeleteMessage(messageId);
                return true;
            }
            return false;
        }

        public string GetSenderUsernameFromMessage(Message message)
        {
            if (message == null) return "Unknown";
            var allUsers = _userDataService.GetAllUsernames().Select(u => _userDataService.GetUserByUsername(u)).ToList();
            var sender = allUsers.FirstOrDefault(u => u.IdentityPublicKey.SequenceEqual(message.SenderIdentityKey));
            return sender?.Username ?? "Unknown";
        }
    }
}