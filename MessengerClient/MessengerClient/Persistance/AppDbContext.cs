using MessengerClient.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClient.Persistance
{
    public class AppDbContext
    {
        public List<Chat>? chats;
        public List<Message>? messages;

        public AppDbContext()
        {
            chats = new List<Chat>();
            messages = new List<Message>();
        }

        public async Task EnsureCreateAsync()
        {
            using (FileStream fs = new FileStream("chats.json", FileMode.OpenOrCreate))
            {
                if (fs.Length != 0)
                    chats = await JsonSerializer.DeserializeAsync<List<Chat>>(fs);
            }
            using (FileStream fs = new FileStream("messages.json", FileMode.OpenOrCreate))
            {
                if (fs.Length != 0)
                    messages = await JsonSerializer.DeserializeAsync<List<Message>>(fs);
            }
        }

        public async Task EnsureUpdateAsync()
        {
            using (FileStream fs = new FileStream("chats.json", FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync(fs, chats);
            }
            using (FileStream fs = new FileStream("messages.json", FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync(fs, messages);
            }
        }

        public async Task EnsureDeleteAsync()
        {
            new FileInfo("chats.json").Delete();
            new FileInfo("messages.json").Delete();
        }
    }
}
