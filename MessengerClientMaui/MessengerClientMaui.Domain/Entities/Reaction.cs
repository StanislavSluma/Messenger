using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerClientMaui.Domain.Entities
{
    public class Reaction : Entity
    {
        public string Name { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public List<int> users_id { get; set; } = new List<int>();
        public int message_id { get; set; } = 0;
    }
}
