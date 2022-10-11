using System.Threading.Tasks;

namespace Client.Services;

internal interface ITextService
{
    public string EncryptedText { get; set; }
    public string DecryptedText { get; set; }

    public Task<string> GetTextAsync(string textName);
    public Task<string> DeleteTextAsync(string textName);
    public Task<string> EditTextAsync(string textName, string editedText);
}