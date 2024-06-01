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
    public class MessageRepository : IRepository<Message>
    {
        AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Message> CreateAsync(Message entity)
        {
            entity.Id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
            _context.messages.Add(entity);
            return entity;
        }

        public async Task DeleteAsync(int entityId)
        {
            Message mess = _context.messages.FirstOrDefault(c => c.Id == entityId);
            _context.messages.Remove(mess);
        }

        public async Task<Message?> FirstOrDefaultAsync(Func<Message, bool> filter)
        {
            Message? mess = _context.messages.FirstOrDefault(filter);
            return mess;
        }

        public async Task<Message?> GetByIdAsync(int id)
        {
            Message? message = _context.messages.FirstOrDefault(x => x.Id == id);
            return message;
        }

        public async Task<IReadOnlyList<Message>> ListAllAsync()
        {
            return _context.messages.AsReadOnly();
        }

        public async Task<IReadOnlyList<Message>> ListAsync(Expression<Func<Message, bool>> filter)
        {
            var query = _context.messages.AsQueryable();
            return query.Where(filter).ToList().AsReadOnly();
        }

        public async Task<Message> UpdateAsync(Message entity)
        {
            Message message = _context.messages.FirstOrDefault(x => x.Id == entity.Id);
            message.Text = entity.Text;
            message.userReactions = entity.userReactions;
            return message;
        }
    }
}
