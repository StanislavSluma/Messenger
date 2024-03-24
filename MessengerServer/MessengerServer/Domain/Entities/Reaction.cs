using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Domain.Entities
{
    public class Reaction : Entity
    {
        public string? Name { get; set; }
        public int userId { get; set; }
        public int messageId { get; set; }
    }
}
