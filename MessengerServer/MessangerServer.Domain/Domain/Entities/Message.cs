using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Domain.Entities
{
    public class Message : Entity
    {
        public int userId { get; set; }
        public int chatId { get; set; }
        public Dictionary<string, List<int>> userReactions { get; set; } = new Dictionary<string, List<int>>(); // ReactionName - usersId
        public string? Text { get; set; }
        public DateTime date { get; set; }
    }
}
