using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        IRepository<User> User_Repository { get; }
        IRepository<Chat> Chat_Repository { get; }
        IRepository<Message> Message_Repository { get; }
        IRepository<Reaction> Reaction_Repository { get; }

        Task SaveAllAsync();
        Task DeleteDataBaseAsync();
        Task CreateDataBaseAsync();
    }
}
