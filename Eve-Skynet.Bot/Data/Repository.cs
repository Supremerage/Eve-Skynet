using System.Linq;
using System.Threading.Tasks;
using Eve_Skynet.Bot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eve_Skynet.Bot.Data
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly BotDbContext _context;

        public Repository(BotDbContext context)
        {
            _context = context;
        }
        public virtual async Task AddOrUpdateAsync(TEntity item)
        {
            var set = _context.Set<TEntity>();
            var entityType = _context.Model.FindEntityType(typeof(TEntity).FullName);
            var primaryKey = entityType.FindPrimaryKey();

            var keyValues = new object[primaryKey.Properties.Count];
            for (int i = 0; i < keyValues.Length; i++)
            {
                keyValues[i] = primaryKey.Properties[i].GetGetter().GetClrValue(item);
            }

            var dbEntity = await set.FindAsync(keyValues);
            if (dbEntity == null)
            {
                await _context.AddAsync(item);
            }
            else
            {
                _context.Entry(dbEntity).CurrentValues.SetValues(item);  
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual IQueryable<TEntity> Get()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        public virtual void Remove(TEntity item)
        {
            _context.Remove(item);
        }
    }
}
