using System.Linq;
using System.Threading.Tasks;

namespace Eve_Skynet.Bot.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task AddOrUpdateAsync(T item);
        void Remove(T item);
        IQueryable<T> Get(); 
        Task CommitAsync();
    }
}
