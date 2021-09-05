using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Repositories.Interfaces;

namespace WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public IUserRepository UserRepository { get; }

        public UsersController(IUserRepository userRepository)
        {
            UserRepository = userRepository;
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

            user.Password = null;

            return CreatedAtAction("GetUser", new { id = user.ID }, user);
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await UserRepository.DeleteAsync(id);

            return Ok();
        }
    }
}
