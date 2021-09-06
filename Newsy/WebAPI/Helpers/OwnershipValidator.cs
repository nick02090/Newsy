using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace WebAPI.Helpers
{
    public class OwnershipValidator
    {
        public static bool ValidateOwnership(HttpContext httpContext, Guid ownerID)
        {
            if (httpContext.User.Identity is ClaimsIdentity identity)
            {
                var userID = Guid.Parse(identity.Claims.First(x => x.Type == "id").Value);
                return userID == ownerID;
            }
            return false;
        }
    }
}
