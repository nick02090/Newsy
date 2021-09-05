using Domain;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Repositories.Interfaces;

namespace WebAPI.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private bool UserExists(Guid id) => NewsyContext.Users.Any(user => user.ID == id);

        public UserRepository(NewsyContext context) : base(context)
        {
        }

        public async Task<User> CreateAsync(User entity)
        {
            NewsyContext.Users.Add(entity);
            await NewsyContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await NewsyContext.Users.FindAsync(id);
            if (user == null)
            {
                return;
            }
            NewsyContext.Users.Remove(user);
            await NewsyContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAsync()
        {
            return await NewsyContext.Users.ToArrayAsync();
        }

        public async Task<User> GetAsync(Guid id)
        {
            return await NewsyContext.Users.FindAsync(id);
        }

        public async Task<User> UpdateAsync(User entity)
        {
            NewsyContext.Entry(entity).State = EntityState.Modified;

            try
            {
                await NewsyContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(entity.ID)) return null;
                else throw;
            }

            return entity;
        }
    }
}
