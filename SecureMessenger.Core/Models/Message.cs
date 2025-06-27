using System;

namespace SecureMessenger.Core.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string RecipientUsername { get; set; }
        public int TargetPreKeyId { get; set; }
        public byte[] SenderIdentityKey { get; set; }
        public byte[] SenderEphemeralKey { get; set; }
        public byte[] Ciphertext { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsEdited { get; set; }
    }
}