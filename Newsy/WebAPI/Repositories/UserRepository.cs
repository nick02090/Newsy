using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Context;
using WebAPI.Helpers;
using WebAPI.Repositories.Interfaces;

namespace WebAPI.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(NewsyContext context) : base(context)
        {
        }

        public async Task<User> CreateAsync(User entity)
        {
            // Never save password!!!
            entity.Password = null;

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

        public async Task<ICollection<User>> GetAsync()
        {
            return await NewsyContext.Users.ToArrayAsync();
        }

        public async Task<User> GetAsync(Guid id)
        {
            return await NewsyContext.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await NewsyContext.Users.Where(x => x.Email.Equals(email))
                .Select(x => new User
                {
                    ID = x.ID,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PasswordHash = x.PasswordHash,
                    PasswordSalt = x.PasswordSalt
                }).SingleOrDefaultAsync();
        }

        public async Task<ICollection<User>> GetByLastName(string lastName)
        {
            return await NewsyContext.Users.Where(x => x.LastName.Equals(lastName))
                .Select(x => new User
                {
                    ID = x.ID,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PasswordHash = x.PasswordHash,
                    PasswordSalt = x.PasswordSalt
                }).ToArrayAsync();
        }

        public async Task<User> UpdateAsync(User entity)
        {
            var updatedUser = await NewsyContext.Users.FindAsync(entity.ID);
            if (updatedUser == null)
                return null;

            // Assign new values to the user
            updatedUser.Email = entity.Email;
            updatedUser.FirstName = entity.FirstName;
            updatedUser.LastName = entity.LastName;

            // Calculate new password hash and salt
            PasswordHasher.CreatePasswordHash(entity.Password, out byte[] passwordHash, out byte[] passwordSalt);
            updatedUser.PasswordHash = passwordHash;
            updatedUser.PasswordSalt = passwordSalt;

            NewsyContext.Users.Update(updatedUser);
            await NewsyContext.SaveChangesAsync();

            return entity;
        }
    }
}
