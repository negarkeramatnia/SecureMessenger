namespace SecureMessenger.Core.Models
{
    public class PreKey
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int KeyId { get; set; }
        public byte[] PublicKey { get; set; }
    }
}