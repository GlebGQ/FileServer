namespace FileServer.Models
{
    public class GenerateSessionKeyResponse
    {
        public byte[] PublicEcdfKey { get; set; }
        public byte[] IV { get; set; }
    }
}
