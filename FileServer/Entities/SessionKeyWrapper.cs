namespace FileServer.Entities
{
    public class SessionKeyWrapper
    {
        public byte[] SessionKey { get; set; }
        private byte[] IV { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
