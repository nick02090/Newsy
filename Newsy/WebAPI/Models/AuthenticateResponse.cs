using Domain;
using System;

namespace WebAPI.Models
{
    public class AuthenticateResponse
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Token { get; set; }


        public AuthenticateResponse(User user, string token)
        {
            ID = user.ID;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Token = token;
        }
    }
}
