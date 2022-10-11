using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FileServer.Models.Request
{
    public class FileContentRequest
    {
        [BindRequired]
        public Guid ClientId { get; set; }
        [BindRequired]
        public string Base64EncryptedTextName { get; set; }

    }
}
