using Microsoft.EntityFrameworkCore;

namespace WareHouse.Repositories
{
    public interface IUnitOfWork<T>
    {
        Task CompleteAsync();
    }

    public class UnitOfWork<T> : IUnitOfWork<T> where T : DbContext
    {
        private readonly T _dbContext;

        public UnitOfWork(T dbContext)
        {
            _dbContext = dbContext;
        }

        public Task CompleteAsync() => _dbContext.SaveChangesAsync();
    }
}