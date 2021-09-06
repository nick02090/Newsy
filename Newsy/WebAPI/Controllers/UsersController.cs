using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Helpers;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
    [Authorize]
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
                r.HidePasswordRelatedData();
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

            user.HidePasswordRelatedData();

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

            // Don't allow other users to update each others data
            if (!OwnershipValidator.ValidateOwnership(HttpContext, id))
                return Unauthorized(new JsonResult(new { message = "You cannot update another user!" }) { StatusCode = StatusCodes.Status401Unauthorized });

            await UserRepository.UpdateAsync(user);

            user.HidePasswordRelatedData();

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

            user = await UserService.RegisterAsync(user);

            user.HidePasswordRelatedData();

            return CreatedAtAction("GetUser", new { id = user.ID }, user);
        }

        // POST: api/users/authenticate
        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        public async Task<IActionResult> AuthenticateAsync([FromBody] User user)
        {
            var entityAuth = await UserService.AuthenticateAsync(user);
            if (entityAuth == null)
                return Unauthorized(new JsonResult(new { message = "Invalid e-mail or password!" }) { StatusCode = StatusCodes.Status401Unauthorized });

            return Ok(entityAuth);
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Don't allow other users to delete each other
            if (!OwnershipValidator.ValidateOwnership(HttpContext, id))
                return Unauthorized(new JsonResult(new { message = "You cannot delete another user!" }) { StatusCode = StatusCodes.Status401Unauthorized });

            await UserRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
