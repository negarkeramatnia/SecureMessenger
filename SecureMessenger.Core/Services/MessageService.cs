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

        public bool SendMessage(string senderUsername, string recipientUsername, string plaintextMessage, AuthService authService)
        {
            //needed to get their public key
            var senderUser = _userDataService.GetUserByUsername(senderUsername);
            var recipientUser = _userDataService.GetUserByUsername(recipientUsername);

            if (senderUser == null || recipientUser == null) return false;

            try
            {
                //AES
                byte[] messageKey = _cryptoService.GenerateSymmetricKey();
                //convert into an array of bytes      needed for encryption
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintextMessage);
                var (ciphertext, nonce, tag) = _cryptoService.EncryptWithAesGcm(messageKey, plaintextBytes);

                //encrypt the message key using the RECIPIENT's public RSA key
                byte[] encryptedKeyForRecipient = _cryptoService.RsaEncrypt(recipientUser.PublicKey, messageKey);
                //encrypt the message key again, this time using the SENDER's public RSA key
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
                return false;
            }
        }

        public string DecryptMessage(Message message, string currentUsername, AuthService authService)
        {
            var currentUser = _userDataService.GetUserByUsername(currentUsername);

            byte[] currentUserPrivateKey = authService.GetCurrentUserPrivateKey();
            if (currentUserPrivateKey == null)
            {
                return "[Decryption Error: User session invalid]";
            }

            //this logic decides which encrypted key to use for decryption
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
                //use the user's private RSA key for message key
                byte[] messageKey = _cryptoService.RsaDecrypt(currentUserPrivateKey, encryptedMessageKeyToUse);
                //use the decrypted message key to decrypt the actual message content
                byte[] plaintextBytes = _cryptoService.DecryptWithAesGcm(messageKey, message.Nonce, message.Ciphertext, message.AuthTag);

                Array.Clear(messageKey, 0, messageKey.Length);
                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch (CryptographicException ex)
            {
                return "[Unable to decrypt message - data may be corrupt]";
            }
        }
        //a simple helper method that just asks the data service to get the conversation history
        public List<Message> GetConversation(string user1, string user2)
        {
            return _messageDataService.GetConversation(user1, user2);
        }

        public bool EditMessage(int messageId, string newText, string currentUsername, AuthService authService)
        {
            var messageToEdit = _messageDataService.GetMessageById(messageId);

            if (messageToEdit == null || messageToEdit.SenderUsername != currentUsername)
            {
                return false;
            }

            //get the private key of the logged in user
            byte[] currentUserPrivateKey = authService.GetCurrentUserPrivateKey();
            if (currentUserPrivateKey == null) return false;

            try
            {
                //decrypt message key
                byte[] messageKey = _cryptoService.RsaDecrypt(currentUserPrivateKey, messageToEdit.EncryptedMessageKeyForSender);

                var newPlaintextBytes = Encoding.UTF8.GetBytes(newText);
                var (newCiphertext, newNonce, newAuthTag) = _cryptoService.EncryptWithAesGcm(messageKey, newPlaintextBytes);

                Array.Clear(messageKey, 0, messageKey.Length);

                return _messageDataService.UpdateMessage(messageId, newCiphertext, newNonce, newAuthTag);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public bool DeleteMessage(int messageId, string currentUsername)
        {
            var messageToDelete = _messageDataService.GetMessageById(messageId);

            if (messageToDelete != null && messageToDelete.SenderUsername == currentUsername)
            {
                return _messageDataService.DeleteMessage(messageId, currentUsername);
            }
            return false;
        }
    }
}