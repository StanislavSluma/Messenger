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
        public List<(int, string)> userReactions { get; set; } = new List<(int, string)>(); // userId - ReactionName
        public string? Text { get; set; }
        public DateTime date { get; set; }
    }
}
