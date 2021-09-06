using Domain;
using System.Threading.Tasks;

namespace WebAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<dynamic> AuthenticateAsync(User user);
        Task<User> RegisterAsync(User user);
    }
}
