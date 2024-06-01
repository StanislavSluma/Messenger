using MessengerServer.Domain.Abstractions;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IRepository<User> User_Repository { get; } = null!;
        public IRepository<Chat> Chat_Repository { get; } = null!;
        public IRepository<Message> Message_Repository { get; } = null!;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            User_Repository = new UserRepository(_context);
            Chat_Repository = new ChatRepository(_context);
            Message_Repository = new MessageRepository(_context);
        }

        public async Task SaveAllAsync()
        {
            await _context.EnsureUpdateAsync();
        }
        public async Task DeleteDataBaseAsync()
        {
            await _context.EnsureDeleteAsync();
        }
        public async Task CreateDataBaseAsync()
        {
            await _context.EnsureCreateAsync();
        }
    }
}
