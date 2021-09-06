using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Repositories.Interfaces
{
    public interface IArticleRepository : IBaseRepository<Article>
    {
        Task<ICollection<Article>> GetByAuthorID(Guid authorID);
        Task<ICollection<Article>> GetByAuthorLastName(string authorLastName);
        Task<ICollection<Article>> GetByCreatedOn(DateTime createdOn);
        Task<ICollection<Article>> GetByTitlePart(string titlePart);
    }
}
