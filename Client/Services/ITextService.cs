using System.Threading.Tasks;

namespace Client.Services;

internal interface ITextService
{
    public Task<string> GetText(string textName);
    public Task<string> DeleteText(string textName);
    public Task<string> UpdateText(string textName, string editedText);
}