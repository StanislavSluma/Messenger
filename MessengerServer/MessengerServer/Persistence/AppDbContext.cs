using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MessengerServer.Persistence
{
    public class AppDbContext
    {
        public List<User>? users;
        public List<Chat>? chats;
        public List<Message>? messages;

        public AppDbContext()
        {
            users = new List<User>();
            chats = new List<Chat>();
            messages = new List<Message>();
        }

        public async Task EnsureCreateAsync()
        {
            using (FileStream fs = new FileStream("users.json", FileMode.OpenOrCreate))
            {
                if(fs.Length != 0)
                    users = await JsonSerializer.DeserializeAsync<List<User>>(fs);
            }
            using (FileStream fs = new FileStream("chats.json", FileMode.OpenOrCreate))
            {
                if(fs.Length != 0)
                    chats = await JsonSerializer.DeserializeAsync<List<Chat>>(fs);
            }
            using (FileStream fs = new FileStream("messages.json", FileMode.OpenOrCreate))
            {
                if(fs.Length != 0)
                    messages = await JsonSerializer.DeserializeAsync<List<Message>>(fs);
            }
        }

        public async Task EnsureUpdateAsync()
        {
            using (FileStream fs = new FileStream("users.json", FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync(fs, users);
            }
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
            new FileInfo("users.json").Delete();
            new FileInfo("chats.json").Delete();
            new FileInfo("messages.json").Delete();
        }
    }
}
