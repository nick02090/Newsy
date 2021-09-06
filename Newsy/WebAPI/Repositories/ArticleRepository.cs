using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Context;
using WebAPI.Repositories.Interfaces;

namespace WebAPI.Repositories
{
    public class ArticleRepository : BaseRepository, IArticleRepository
    {
        public ArticleRepository(NewsyContext context) : base(context)
        {
        }

        public async Task<Article> CreateAsync(Article entity)
        {
            // Set initial date
            entity.CreatedOn = DateTime.UtcNow;
            entity.LastEditedOn = DateTime.UtcNow;
            NewsyContext.Articles.Add(entity);
            await NewsyContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var article = await NewsyContext.Articles.FindAsync(id);
            if (article == null)
                return;

            NewsyContext.Articles.Remove(article);
            await NewsyContext.SaveChangesAsync();
        }

        public async Task<ICollection<Article>> GetAsync()
        {
            return await NewsyContext.Articles.ToArrayAsync();
        }

        public async Task<Article> GetAsync(Guid id)
        {
            return await NewsyContext.Articles.FindAsync(id);
        }

        public async Task<ICollection<Article>> GetByAuthorID(Guid authorID)
        {
            return await NewsyContext.Articles.Where(article => article.Author.ID.Equals(authorID))
                .Select(x => new Article
                {
                    ID = x.ID,
                    Body = x.Body,
                    CreatedOn = x.CreatedOn,
                    Description = x.Description,
                    LastEditedOn = x.LastEditedOn,
                    Title = x.Title,
                    Author = new User
                    {
                        ID = x.Author.ID,
                        Email = x.Author.Email,
                        FirstName = x.Author.FirstName,
                        LastName = x.Author.LastName
                    }
                }).ToArrayAsync();
        }

        public async Task<ICollection<Article>> GetByAuthorLastName(string authorLastName)
        {
            return await NewsyContext.Articles.Where(article => article.Author.LastName.Equals(authorLastName))
                .Select(x => new Article
                {
                    ID = x.ID,
                    Body = x.Body,
                    CreatedOn = x.CreatedOn,
                    Description = x.Description,
                    LastEditedOn = x.LastEditedOn,
                    Title = x.Title,
                    Author = new User
                    {
                        ID = x.Author.ID,
                        Email = x.Author.Email,
                        FirstName = x.Author.FirstName,
                        LastName = x.Author.LastName
                    }
                }).ToArrayAsync();
        }

        public async Task<ICollection<Article>> GetByCreatedOn(DateTime createdOn)
        {
            return await NewsyContext.Articles.Where(article => article.CreatedOn.Date.Equals(createdOn.Date))
                .Select(x => new Article
                {
                    ID = x.ID,
                    Body = x.Body,
                    CreatedOn = x.CreatedOn,
                    Description = x.Description,
                    LastEditedOn = x.LastEditedOn,
                    Title = x.Title,
                    Author = x.Author
                }).ToArrayAsync();
        }

        public async Task<ICollection<Article>> GetByTitlePart(string titlePart)
        {
            return await NewsyContext.Articles.Where(article => article.Title.Contains(titlePart))
                .Select(x => new Article
                {
                    ID = x.ID,
                    Body = x.Body,
                    CreatedOn = x.CreatedOn,
                    Description = x.Description,
                    LastEditedOn = x.LastEditedOn,
                    Title = x.Title,
                    Author = x.Author
                }).ToArrayAsync();
        }

        public async Task<Article> UpdateAsync(Article entity)
        {
            var updatedArticle = await NewsyContext.Articles.FindAsync(entity.ID);
            if (updatedArticle == null)
                return null;

            // Assign new values to the article
            updatedArticle.Title = entity.Title;
            updatedArticle.Description = entity.Description;
            updatedArticle.Body = entity.Body;
            updatedArticle.LastEditedOn = DateTime.UtcNow;

            NewsyContext.Articles.Update(updatedArticle);
            await NewsyContext.SaveChangesAsync();

            return entity;
        }
    }
}
