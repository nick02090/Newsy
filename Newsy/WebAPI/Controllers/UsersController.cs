using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public IUserRepository UserRepository { get; }
        public IUserService UserService { get; }

        public UsersController(IUserRepository userRepository, IUserService userService)
        {
            UserRepository = userRepository;
            UserService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            var result = await UserRepository.GetAsync();
            foreach (var r in result)
            {
                r.Password = null;
            }
            return result;
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await UserRepository.GetAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Password = null;

            return Ok(user);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] Guid id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.ID)
            {
                return BadRequest();
            }

            await UserRepository.UpdateAsync(user);

            return NoContent();
        }

        // POST: api/users
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            user = await UserRepository.CreateAsync(user);

            var authResponse = await UserService.AuthenticateAsync(new AuthenticateRequest 
            { 
                Email = user.Email,
                Password = user.Password
            });

            user.Password = null;

            return CreatedAtAction("GetUser", new { id = user.ID }, authResponse);
        }

        // DELETE: api/users/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var userID = Guid.Parse(identity.Claims.First(x => x.Type == "id").Value);
                if (userID != id)
                {
                    return Unauthorized(new JsonResult(new { message = "You cannot delete another user!" }) { StatusCode = StatusCodes.Status401Unauthorized });
                }
            }

            await UserRepository.DeleteAsync(id);

            return Ok();
        }
    }
}
