using System.Collections.Concurrent;
using FileServer.Entities;

namespace FileServer.KeyStore
{
    public class KeyStore
    {
        private ConcurrentDictionary<Guid, SessionKeyWrapper> SessionKeys { get; }

        public KeyStore()
        {
            SessionKeys = new ConcurrentDictionary<Guid, SessionKeyWrapper>();
        }

        public SessionKeyWrapper GetSessionKey(Guid clientId)
        {

            var exists = SessionKeys.TryGetValue(clientId, out var sessionKeyWrapper);
            if (exists)
            {
                if (sessionKeyWrapper.ExpirationDateTime > DateTime.Now)
                {
                    var isRemoved = SessionKeys.TryRemove(clientId, out sessionKeyWrapper);
                    return null;
                }

                return sessionKeyWrapper;
            }
            return null;
        }

        public void AddSessionKey(Guid clientId, byte[] sessionKey)
        {
            var sessionKeyWrapper = new SessionKeyWrapper
            {
                ExpirationDateTime = DateTime.Now.AddMinutes(1),
                SessionKey = sessionKey
            };
            SessionKeys.TryAdd(clientId, sessionKeyWrapper);
        }
    }
}
