using Domain.Context;

namespace WebAPI.Repositories
{
    public abstract class BaseRepository
    {
        protected NewsyContext NewsyContext { get; }
        public BaseRepository(NewsyContext context)
        {
            NewsyContext = context;
        }
    }
}
