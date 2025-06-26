namespace SecureMessenger.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public byte[] IdentityPublicKey { get; set; } // Renamed from PublicKey
        public byte[] EncryptedIdentityKey { get; set; } // Renamed from EncryptedPrivateKey
        public byte[] PrivateKeyNonce { get; set; }
        public byte[] PrivateKeyAuthTag { get; set; }
    }
}