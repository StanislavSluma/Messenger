using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Domain.Entities
{
    public class Chat : Entity
    {
        public string? Name { get; set; }
        public List<int> usersId { get; set; } = new List<int>();

        public void AddUserById(int userId)
        {
            usersId.Add(userId);
        }

        public void RemoveUserById(int userId)
        {
            usersId.Remove(userId);
        }
    }
}
