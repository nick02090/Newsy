using Domain;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Settings;
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

        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request)
        {
            // TODO: Verify password
            var user = await GetByEmail(request.Email);

            if (user == null) return null;

            var token = GenerateJWTToken(user);
            return new AuthenticateResponse(user, token);
        }

        public async Task<User> GetByEmail(string email)
        {
            var users = await UserRepository.GetAsync();
            return users.SingleOrDefault(x => x.Email == email);
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
