using MessengerClient.Domain.Entities;
using MessengerServer.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerClient.Domain.Abstraction
{
    public interface IUnitOfWork
    {
        IRepository<Message> Message_Repository { get; }
        IRepository<Chat> Chat_Repository { get; }
        Task SaveAllAsync();
        Task DeleteDataBaseAsync();
        Task CreateDataBaseAsync();
    }
}
