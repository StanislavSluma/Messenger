using MessengerClient.Domain.Entities;
using MessengerServer.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MessengerClient.Persistance
{
    public class ChatRepository : IRepository<Chat>
    {
        AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Chat> CreateAsync(Chat entity)
        {
            entity.Id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
            _context.chats.Add(entity);
            return entity;
        }

        public async Task DeleteAsync(int entityId)
        {
            Chat chat = _context.chats.FirstOrDefault(c => c.Id == entityId);
            _context.chats.Remove(chat);
        }

        public async Task<Chat?> FirstOrDefaultAsync(Func<Chat, bool> filter)
        {
            Chat? chat = _context.chats.FirstOrDefault(filter);
            return chat;
        }

        public async Task<Chat> GetByIdAsync(int id)
        {
            Chat chat = _context.chats.FirstOrDefault(x => x.Id == id);
            return chat;
        }

        public async Task<IReadOnlyList<Chat>> ListAllAsync()
        {
            return _context.chats.AsReadOnly();
        }

        public async Task<IReadOnlyList<Chat>> ListAsync(Expression<Func<Chat, bool>> filter)
        {
            var query = _context.chats.AsQueryable();
            return query.Where(filter).ToList();
        }

        public async Task<Chat> UpdateAsync(Chat entity)
        {
            Chat chat = _context.chats.FirstOrDefault(x => x.Id == entity.Id);
            chat = entity;
            return chat;
        }
    }
}
