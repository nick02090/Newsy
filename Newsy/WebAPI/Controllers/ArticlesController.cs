using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Helpers;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/articles")]
    [Authorize]
    [ApiController]
    public class ArticlesController : ControllerBase
    {

        public IArticleRepository ArticleRepository { get; }
        public IUserRepository UserRepository { get; }
        public IArticleService ArticleService { get; }

        public ArticlesController(IArticleRepository articleRepository, IUserRepository userRepository, IArticleService articleService)
        {
            ArticleRepository = articleRepository;
            UserRepository = userRepository;
            ArticleService = articleService;
        }

        // GET: api/articles
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<Article>))]
        public async Task<IActionResult> GetArticles([FromQuery] Guid authorID, [FromQuery] string authorLastName, [FromQuery] DateTime createdOn, [FromQuery] string titlePart)
        {
            var result = await ArticleRepository.GetAsync();

            if (authorID != null)
                result = result.Where(x => x.Author.ID.Equals(authorID)).ToList();
            if (authorLastName != null)
                result = result.Where(x => x.Author.LastName.Equals(authorLastName)).ToList();
            if (createdOn != null)
                result = result.Where(x => x.CreatedOn.Date.Equals(createdOn.Date)).ToList();
            if (titlePart != null)
                result = result.Where(x => x.Title.Contains(titlePart)).ToList();

            return Ok(result);
        }

        // GET: api/articles/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Article))]
        public async Task<IActionResult> GetArticle([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await ArticleRepository.GetAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            return Ok(article);
        }

        // PUT: api/articles/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutArticle([FromRoute] Guid id, [FromBody] Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != article.ID)
            {
                return BadRequest();
            }

            // Don't allow other users to update each others data
            if (!OwnershipValidator.ValidateOwnership(HttpContext, article.Author.ID))
                return Unauthorized(new JsonResult(new { message = "You cannot update article from another user!" }) { StatusCode = StatusCodes.Status401Unauthorized });

            await ArticleRepository.UpdateAsync(article);

            return NoContent();
        }

        // POST: api/articles
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> PostArticle([FromBody] Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the creator of the article is current user
            if (!OwnershipValidator.ValidateOwnership(HttpContext, article.Author.ID))
                return Unauthorized(new JsonResult(new { message = "You cannot create article as another user!" }) { StatusCode = StatusCodes.Status401Unauthorized });

            article.Author = await UserRepository.GetAsync(article.Author.ID);
            article = await ArticleRepository.CreateAsync(article);

            // To prevent cycle
            article.Author.Articles = null;
            article.Author.HidePasswordRelatedData();

            return CreatedAtAction("GetArticle", new { id = article.ID }, article);
        }

        // DELETE: api/articles/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteArticle([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await ArticleRepository.GetAsync(id);

            // Don't allow other users to delete each others articles
            if (!OwnershipValidator.ValidateOwnership(HttpContext, article.Author.ID))
                return Unauthorized(new JsonResult(new { message = "You cannot delete another users article!" }) { StatusCode = StatusCodes.Status401Unauthorized });

            await ArticleRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
