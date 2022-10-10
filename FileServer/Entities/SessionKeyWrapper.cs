namespace FileServer.Entities
{
    public class SessionKeyWrapper
    {
        public byte[] SessionKey { get; set; }
        public byte[] IV { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
