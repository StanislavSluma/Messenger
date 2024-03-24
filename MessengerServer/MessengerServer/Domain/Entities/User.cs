using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Domain.Entities
{
    public class User : Entity
    {
        public List<int> chatsId { get; set; } = new List<int>();
        public string? Name { get; set; }
        public string? PasswordHash { get; set; }
        public string? Login { get; set; }

        public void AddChatById(int chatId)
        {
            chatsId.Add(chatId);
        }

        public void RemoveChatById(int chatId)
        {
            chatsId.Remove(chatId);
        }
    }
}
