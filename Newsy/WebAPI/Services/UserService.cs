using Domain;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Helpers;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Settings.Interfaces;

namespace WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IAppSettings AppSettings;
        private readonly IUserRepository UserRepository;

        public UserService(IAppSettings appSettings, IUserRepository userRepository)
        {
            AppSettings = appSettings;
            UserRepository = userRepository;
        }

        public async Task<dynamic> AuthenticateAsync(User entity)
        {
            // Verify email
            var userDb = await GetByEmailAsync(entity.Email);
            if (userDb == null)
                return null;

            // Verify password
            if (!PasswordHasher.VerifyPasswordHash(entity.Password, userDb.PasswordHash, userDb.PasswordSalt))
                return null;

            var token = GenerateJWTToken(userDb);

            userDb.HidePasswordRelatedData();

            return new { userDb, token };
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var users = await UserRepository.GetAsync();
            return users.SingleOrDefault(x => x.Email == email);
        }

        public async Task<User> RegisterAsync(User user)
        {
            PasswordHasher.CreatePasswordHash(user.Password,
                                              out byte[] passwordHash,
                                              out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            return await UserRepository.CreateAsync(user);
        }

        private string GenerateJWTToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(AppSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim("id", user.ID.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
