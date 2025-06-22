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

        public MessageService()
        {
            _cryptoService = new CryptoService();
            _userDataService = new UserDataService();
            _messageDataService = new MessageDataService();
        }

        public bool SendMessage(string senderUsername, string recipientUsername, string plaintextMessage, byte[] senderPrivateKey, User senderUser)
        {
            var recipientUser = _userDataService.GetUserByUsername(recipientUsername);
            if (recipientUser == null) return false; // Recipient doesn't exist

            try
            {
                // 1. Generate a unique, one-time symmetric key for this message
                byte[] messageKey = _cryptoService.GenerateSymmetricKey();

                // 2. Encrypt the message content with this symmetric key
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintextMessage);
                var (ciphertext, nonce, tag) = _cryptoService.EncryptWithAesGcm(messageKey, plaintextBytes);

                // 3. Encrypt the symmetric key with both the sender's and recipient's public keys
                byte[] encryptedKeyForRecipient = _cryptoService.RsaEncrypt(recipientUser.PublicKey, messageKey);
                byte[] encryptedKeyForSender = _cryptoService.RsaEncrypt(senderUser.PublicKey, messageKey);

                // 4. Clear the plaintext symmetric key from memory immediately
                Array.Clear(messageKey, 0, messageKey.Length);

                // 5. Create the Message object to store in the database
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

                // 6. Save the encrypted message to the database
                return _messageDataService.CreateMessage(messageToStore);
            }
            catch (Exception)
            {
                // In a real app, log the exception
                return false;
            }
        }

        public string DecryptMessage(Message message, string currentUsername, byte[] currentUserPrivateKey)
        {
            byte[] encryptedMessageKeyToUse;

            // Determine which encrypted key to use based on who is viewing the message
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
                // 1. Decrypt the symmetric message key using the user's RSA private key
                byte[] messageKey = _cryptoService.RsaDecrypt(currentUserPrivateKey, encryptedMessageKeyToUse);

                // 2. Decrypt the actual message content using the symmetric key
                byte[] plaintextBytes = _cryptoService.DecryptWithAesGcm(messageKey, message.Nonce, message.Ciphertext, message.AuthTag);

                // 3. Clear the decrypted key from memory
                Array.Clear(messageKey, 0, messageKey.Length);

                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch (CryptographicException)
            {
                // Decryption failed (tampering, wrong key, etc.)
                return "[Unable to decrypt message - data may be corrupt]";
            }
        }

        public List<Message> GetConversation(string user1, string user2)
        {
            return _messageDataService.GetConversation(user1, user2);
        }
    }
}