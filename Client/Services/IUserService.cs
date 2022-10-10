using System;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Services;

internal interface IUserService
{
    public Task<LoginResponse> LogIn(string userEmail, string userPassword);

    public Task<CreateConnectionResponse> CreateConnection(Guid appIdentifier);
}