using MessengerClient.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClient.Client
{
    public class Client
    {
        string host = "192.168.222.205"; // "127.0.0.1"
        int RR_port = 8888;
        int N_port = 8080;
        TcpClient RR_client;
        TcpClient N_client;
        User user = null!;
        List<Chat> chats = new List<Chat>();
        Dictionary<int, List<Message>> messages = new Dictionary<int, List<Message>>(); //(chatId, message)
        Chat? curr_chat;
        StreamReader RR_reader = null!;
        StreamReader N_reader = null!;
        StreamWriter RR_writer = null!;
        StreamWriter N_writer = null!;

        public Client()
        {
            RR_client = new TcpClient();
            N_client = new TcpClient();
        }

        public void Disconnect()
        {
            RR_client.Close();
            N_client.Close();
            RR_reader?.Close();
            RR_writer?.Close();
            N_reader?.Close();
            N_writer?.Close();
        }

        public async Task ClientStart()
        {
            RR_client.Connect(host, RR_port);
            N_client.Connect(host, N_port);
            RR_reader = new StreamReader(RR_client.GetStream());
            N_reader = new StreamReader(N_client.GetStream());
            RR_writer = new StreamWriter(RR_client.GetStream());
            N_writer = new StreamWriter(N_client.GetStream());
            ServerNotify();
            await ServerInteractive();
            Disconnect();
        }

        public async Task ServerInteractive()
        {
            await EnterPage();
            await MenuPage();
        }

        public async Task EnterPage()
        {
            while (true)
            {
                Console.WriteLine("1 - Sign Up\n2 - Sign in");
                string sh = Console.ReadLine();
                string request = "";
                if (sh == "1")
                {
                    Console.Write("Name: ");
                    string? user_name = Console.ReadLine();
                    Console.Write("Login: ");
                    string? login = Console.ReadLine();
                    Console.Write("Password: ");
                    string password = Console.ReadLine();
                    user = new User() { Name = user_name, Login = login, PasswordHash = password };
                    request = JsonSerializer.Serialize(user);
                    request = "SignUp?" + request;
                }
                else
                {
                    Console.Write("Login: ");
                    string? login = Console.ReadLine();
                    Console.Write("Password: ");
                    string? password = Console.ReadLine();
                    user = new User() { Login = login, PasswordHash = password };
                    request = JsonSerializer.Serialize(user);
                    request = "SignIn?" + request;
                }
                await RR_writer.WriteLineAsync(request);
                await RR_writer.FlushAsync();
                string response = await RR_reader.ReadLineAsync();
                if (response == "Error")
                {
                    Console.WriteLine("Something error. Try again");
                    continue;
                }
                user = JsonSerializer.Deserialize<User>(response);
                request = "GetAllChat?";
                await RR_writer.WriteLineAsync(request);
                await RR_writer.FlushAsync();
                response = await RR_reader.ReadLineAsync();
                chats = JsonSerializer.Deserialize<List<Chat>>(response);
                string tag = "GetChatMessages?";
                foreach (var chat in chats)
                {
                    request = tag + $"{chat.Id}!";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                    string respons = await RR_reader.ReadLineAsync();
                    if (respons == "Error")
                        continue;
                    messages[chat.Id] = JsonSerializer.Deserialize<List<Message>>(respons);
                }
                break;
            }
        }

        public async Task MenuPage()
        {
            while (true)
            {
                Console.WriteLine("1 - Add Chat\n2 - Select Chat\n3 - Exit");
                string ch = Console.ReadLine();
                if (ch == "1")
                {
                    Console.WriteLine("Enter chat name: ");
                    string? chat_name = Console.ReadLine();
                    List<string> users_name = new List<string>();
                    while (true)
                    {
                        Console.WriteLine("1 - Add user to chat\n2 - Exit");
                        string chos = Console.ReadLine();
                        if (chos == "2")
                            break;
                        Console.WriteLine("Enter user name: ");
                        users_name.Add(Console.ReadLine());
                        Console.Clear();
                    }
                    string request = "CreateChat?" + $"{chat_name}!" + JsonSerializer.Serialize(users_name);
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                    string response = await RR_reader.ReadLineAsync();
                    if (response == "Error")
                    {
                        Console.WriteLine("Something error. Try again");
                        continue;
                    }
                    //N_reader получит сообщение о создании чата
                    //Chat res_chat = JsonSerializer.Deserialize<Chat>(response);
                    //chats.Add(res_chat);
                    //messages[res_chat.Id] = new List<Message>();
                    Console.WriteLine("Succes Added!");
                }
                else if (ch == "2")
                {
                    if (chats.Count == 0)
                    {
                        Console.WriteLine("No chats");
                        continue;
                    }
                    Console.WriteLine("Enter Exit to exit");
                    for (int i = 0; i < chats.Count; i++)
                    {
                        Console.WriteLine($"{i + 1} - {chats[i].Name}");
                    }
                    Console.Write("Enter a number: ");
                    string chh = Console.ReadLine();
                    if (chh == "Exit")
                        continue;
                    int chat_num = int.Parse(chh);
                    curr_chat = chats[chat_num - 1];
                    await ChatMenu();
                }
                else
                {
                    break;
                }
            }
        }

        public async Task ChatMenu()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("1 - Add user\n2 - Write in chat\n3 - Exit");
                string ch = Console.ReadLine();
                if (ch == "1")
                {
                    Console.WriteLine("Enter user name: ");
                    string user_name = Console.ReadLine();
                    string request = "AddUserToChat?" + $"{user_name}!" + $"{JsonSerializer.Serialize(curr_chat)}!";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                    string response = await RR_reader.ReadLineAsync();
                    if (response == "Error")
                    {
                        Console.WriteLine("Something error. Try again");
                    }
                    else
                    {
                        Console.WriteLine("Succes!");
                    }
                }
                else if(ch == "2")
                {
                    await WriteInChat();
                }
                else
                {
                    curr_chat = null;
                    break;
                }
            }
        }

        public async Task WriteInChat()
        {
            Console.WriteLine("Write ExitChat to Exit chat");
            List<Message> list_messages = messages[curr_chat.Id].ToList();
            list_messages.Sort((x, y) => DateTime.Compare(x.date, y.date));
            foreach (var item in list_messages)
            {
                Console.WriteLine(item.Text);
            }
            while (true)
            {
                string mess = Console.ReadLine();
                if (mess == "ExitChat")
                    break;
                mess = $"{user.Name}: " + mess;
                Message message = new Message() {Text = mess, chatId = curr_chat.Id, date = DateTime.Now, userId = user.Id };
                string request = "AddMessage?";
                request += JsonSerializer.Serialize(message) + "!" + JsonSerializer.Serialize(curr_chat.usersId) + "!";
                await RR_writer.WriteLineAsync(request);
                await RR_writer.FlushAsync();
                string response = await RR_reader.ReadLineAsync();
                message = JsonSerializer.Deserialize<Message>(response);
                messages[curr_chat.Id].Add(message);
            }
        }

        public async Task ServerNotify()
        {
            try
            {
                while (true)
                {
                    string response = await N_reader.ReadLineAsync();
                    string tag = response.Substring(0, response.IndexOf('?') + 1);
                    response = response.Remove(0, response.IndexOf('?') + 1);
                    switch (tag)
                    {
                        case "Message?":
                            {
                                Message message = JsonSerializer.Deserialize<Message>(response);
                                if (messages.ContainsKey(message.chatId))
                                    messages[message.chatId].Add(message);
                                if (curr_chat?.Id == message.chatId)
                                    Print(message);
                                break;
                            }
                        case "Chat?":
                            {
                                Chat chat = JsonSerializer.Deserialize<Chat>(response);
                                Chat? old_chat = chats.FirstOrDefault(x => x.Id == chat.Id);
                                if (old_chat == null)
                                {
                                    chats.Add(chat);
                                    messages[chat.Id] = new List<Message>();
                                }
                                else
                                {
                                    old_chat = chat;
                                    if (curr_chat.Id == old_chat.Id)
                                    {
                                        curr_chat = old_chat;
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally 
            {
                Disconnect(); 
            }
        }

        public void Print(Message message)
        {
            if (OperatingSystem.IsWindows())    // если ОС Windows
            {
                var position = Console.GetCursorPosition(); // получаем текущую позицию курсора
                int left = position.Left;   // смещение в символах относительно левого края
                int top = position.Top;     // смещение в строках относительно верха
                                            // копируем ранее введенные символы в строке на следующую строку
                Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
                // устанавливаем курсор в начало текущей строки
                Console.SetCursorPosition(0, top);
                // в текущей строке выводит полученное сообщение
                Console.WriteLine($"{message.date.TimeOfDay} {message.Text}");
                // переносим курсор на следующую строку
                // и пользователь продолжает ввод уже на следующей строке
                Console.SetCursorPosition(left, top + 1);
            }
            else Console.WriteLine(message);
        }
    }

}
