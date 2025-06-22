// In SecureMessenger.Core/Models/Message.cs
using System;

namespace SecureMessenger.Core.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; }
        public string RecipientUsername { get; set; }
        public byte[] Ciphertext { get; set; }
        public byte[] Nonce { get; set; }
        public byte[] AuthTag { get; set; }
        public byte[] EncryptedMessageKeyForSender { get; set; }
        public byte[] EncryptedMessageKeyForRecipient { get; set; }
        public DateTime Timestamp { get; set; }
    }
}