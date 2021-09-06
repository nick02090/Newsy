using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<ICollection<User>> GetByLastName(string lastName);
    }
}
