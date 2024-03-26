using MessengerServer.Domain.Abstractions;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Persistence
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        AppDbContext _dbcontext;

        public Repository(AppDbContext _context)
        {
            _dbcontext = _context;
        }

        public Task<T> CreateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int entityId)
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstOrDefaultAsync(Func<T, bool> filter)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> ListAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
