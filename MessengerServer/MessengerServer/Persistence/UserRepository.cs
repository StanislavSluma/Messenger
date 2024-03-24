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
    public class UserRepository : IRepository<User>
    {
        AppDbContext _context;

        public UserRepository(AppDbContext _context)
        {
            this._context = _context;
        }

        public async Task<User> CreateAsync(User entity)
        {
            entity.Id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
            _context.users.Add(entity);
            return entity;
        }

        public async Task DeleteAsync(int entityId)
        {
            User user = _context.users.FirstOrDefault(c => c.Id == entityId);
            _context.users.Remove(user);
        }

        public async Task<User?> FirstOrDefaultAsync(Func<User, bool> filter)
        {
            User? user = _context.users.FirstOrDefault(filter);
            return user;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            User? user = _context.users.FirstOrDefault(x => x.Id == id);
            return user;
        }

        public async Task<IReadOnlyList<User>> ListAllAsync()
        {
            return _context.users.AsReadOnly();
        }

        public async Task<IReadOnlyList<User>> ListAsync(Expression<Func<User, bool>> filter)
        {
            var query = _context.users.AsQueryable();
            var users = query.Where(filter);
            return users.ToList();
        }

        public async Task<User> UpdateAsync(User entity)
        {
            var user = _context.users.FirstOrDefault(x => x.Id == entity.Id);
            user = entity;
            return user;
        }
    }
}
