using System;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Services;

internal interface IUserService
{
    public Task<(string message, string token)> LogIn(string userEmail, string userPassword);

    public Task<(string message, GenerateSessionKeyResponse? sessionKeyResponse)> CreateConnection(Guid appIdentifier);
}