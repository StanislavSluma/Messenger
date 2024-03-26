using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Domain.Abstractions
{
    public interface IRepository<T> where T : Entity
    {
        Task<T> GetByIdAsync(int id);

        Task<T> CreateAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task DeleteAsync(int entityId);

        Task<IReadOnlyList<T>> ListAllAsync();

        Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> filter);

        Task<T> FirstOrDefaultAsync(Func<T, bool> filter);

    }
}
