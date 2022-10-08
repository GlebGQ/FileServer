namespace FileServer.Models.Request
{
    public class GenerateSessionKeyRequest
    {
        public Guid ClientId { get; set; }
        public byte[] ClientPublicKey { get; set; }
    }
}
