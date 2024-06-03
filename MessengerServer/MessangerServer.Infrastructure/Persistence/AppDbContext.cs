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
                if (fs.Length != 0)
                    users = await JsonSerializer.DeserializeAsync<List<User>>(fs, new JsonSerializerOptions { WriteIndented=true });
            }
            using (FileStream fs = new FileStream("chats.json", FileMode.OpenOrCreate))
            {
                if (fs.Length != 0)
                    chats = await JsonSerializer.DeserializeAsync<List<Chat>>(fs, new JsonSerializerOptions { WriteIndented = true });
            }
            using (FileStream fs = new FileStream("messages.json", FileMode.OpenOrCreate))
            {
                if (fs.Length != 0)
                    messages = await JsonSerializer.DeserializeAsync<List<Message>>(fs, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        public async Task EnsureUpdateAsync()
        {
            using (FileStream fs = new FileStream("users.json", FileMode.Truncate))
            {
                await JsonSerializer.SerializeAsync(fs, users, new JsonSerializerOptions { WriteIndented = true });
            }
            using (FileStream fs = new FileStream("chats.json", FileMode.Truncate))
            {
                await JsonSerializer.SerializeAsync(fs, chats, new JsonSerializerOptions { WriteIndented = true });
            }
            using (FileStream fs = new FileStream("messages.json", FileMode.Truncate))
            {
                await JsonSerializer.SerializeAsync(fs, messages, new JsonSerializerOptions { WriteIndented = true });
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
