using System;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Services;

internal interface IUserService
{
    public Guid ClientId { get; set; }
    public Task<string> LogInAsync(string userEmail, string userPassword);

    public Task<string> CreateConnectionAsync();
}