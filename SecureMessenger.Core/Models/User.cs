// In SecureMessenger.Core/Models/User.cs
namespace SecureMessenger.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] EncryptedPrivateKey { get; set; }
        public byte[] PrivateKeyNonce { get; set; }
        public byte[] PrivateKeyAuthTag { get; set; }
    }
}