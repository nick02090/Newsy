using Domain;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
        Task<User> GetByEmail(string email);
    }
}
