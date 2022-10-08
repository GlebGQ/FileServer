namespace FileServer.Models
{
    public class GenerateSessionKeyResponse
    {
        public byte[] EncryptedSessionKey { get; set; }
        public byte[] IV { get; set; }
    }
}
