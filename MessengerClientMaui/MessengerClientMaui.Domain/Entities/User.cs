using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerClientMaui.Domain.Entities
{
    public class User : Entity
    {
        public List<int> chatsId { get; set; } = new List<int>();
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PasswordHash { get; set; }
        public string? Login { get; set; }
    }
}
