using MessengerClient.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClient.Client
{
    public class Client
    {
        string host = "127.0.0.1"; // "127.0.0.1"
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
            Chat curr_chat_save = curr_chat;
            while (true)
            {
                Console.WriteLine("1 - Add user\n2 - Write in chat\n3 - Leave chat\n4 - Exit");
                string ch = Console.ReadLine();
                if (ch == "1")
                {
                    Console.WriteLine("Enter user name: ");
                    string user_name = Console.ReadLine();
                    string request = "AddUserToChat?" + $"{user_name}!" + $"{JsonSerializer.Serialize(curr_chat_save)}!";
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
                    curr_chat = curr_chat_save;
                    await WriteInChat();
                }
                else if(ch == "3")
                {
                    string request = "LeaveChat?";
                    request += $"{curr_chat_save.Id}";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                    user.chatsId.Remove(curr_chat_save.Id);
                    chats.Remove(curr_chat_save);
                    messages.Remove(curr_chat_save.Id);
                    curr_chat = new();
                    break;
                }
                else
                {
                    curr_chat = new();
                    break;
                }
            }
        }

        public async Task WriteInChat()
        {
            Console.Clear();
            Console.WriteLine("Write ExitChat to Exit chat");
            Console.WriteLine("Write DeleteMess to delete message");
            Console.WriteLine("Write EditMess to edit message");
            Console.WriteLine("Write SetReactMess to set reaction on message");
            Console.WriteLine("Write UnsetReactMess to unset reaction on message\n");
            messages[curr_chat.Id].Sort((x, y) => DateTime.Compare(x.date, y.date));
            foreach (var item in messages[curr_chat.Id])
            {
                if (item.Text.Substring(0, item.Text.IndexOf(':')) == $"{user.Name}")
                {
                    item.Text = item.Text.Remove(0, item.Text.IndexOf(':'));
                    item.Text = "me" + item.Text;
                }
                Console.WriteLine($"{item.date.ToString("hh:mm:ss")} {item.Text}");
                PrintReactions(item);
            }
            while (true)
            {
                string mess = Console.ReadLine();
                if (mess == "ExitChat")
                {
                    curr_chat = new();
                    break;
                }
                if (mess == "DeleteMess")
                {
                    Console.WriteLine("Enter a date of send message (Format hh:mm:ss) :");
                    string date_of_mess = Console.ReadLine();
                    int? messId = null;
                    int index = 0;
                    foreach (var item in messages[curr_chat.Id])
                    {
                        if(item.date.ToString("hh:mm:ss") == date_of_mess)
                        {
                            if (item.userId == user.Id)
                            {
                                messId = item.Id;
                                break;
                            }
                        }
                        ++index;
                    }
                    if(messId == null)
                    {
                        Console.WriteLine("This message not exist");
                        continue;
                    }
                    //messages[curr_chat.Id].RemoveAt(index);
                    string request = "DeleteMessage?" + $"{messId}!" + JsonSerializer.Serialize(curr_chat) + "!";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                }
                else if (mess == "EditMess")
                {
                    Console.WriteLine("Enter a date of send message (Format hh:mm:ss) :");
                    string date_of_mess = Console.ReadLine();
                    Message? message = null;
                    int index = 0;
                    foreach (var item in messages[curr_chat.Id])
                    {
                        if (item.date.ToString("hh:mm:ss") == date_of_mess)
                        {
                            if (item.userId == user.Id)
                            {
                                message = item;
                                break;
                            }
                        }
                        ++index;
                    }
                    if (message == null)
                    {
                        Console.WriteLine("This message not exist");
                        continue;
                    }
                    Console.WriteLine("Write new mess:");
                    string new_text_mess = Console.ReadLine();
                    message.Text = $"{user.Name}: " + new_text_mess;
                    string request = "EditMessage?" + JsonSerializer.Serialize(message) + "!" + JsonSerializer.Serialize(curr_chat.usersId) + "!";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                }
                else if(mess == "SetReactMess")
                {
                    Console.WriteLine("Enter a date of send message (Format hh:mm:ss) :");
                    string date_of_mess = Console.ReadLine();
                    Message? message = null;
                    int index = 0;
                    foreach (var item in messages[curr_chat.Id])
                    {
                        if (item.date.ToString("hh:mm:ss") == date_of_mess)
                        {
                            message = item;
                            break;
                        }
                        ++index;
                    }
                    if (message == null)
                    {
                        Console.WriteLine("This message not exist");
                        continue;
                    }
                    Console.WriteLine("Choose reaction:");
                    Console.WriteLine("1 - :), 2 - :(, 3 - ;), 4 - ;(, 5 - XD, 6 - exit");
                    string smile = Console.ReadLine();
                    if (smile != "1" && smile != "2" && smile != "3" && smile != "4" && smile != "5")
                        continue;
                    if (smile=="1")
                        smile = ":)";
                    if (smile == "2")
                        smile = ":(";
                    if (smile == "3")
                        smile = ";)";
                    if (smile == "4")
                        smile = ";(";
                    if (smile == "5")
                        smile = "XD";
                    string request = "SetReaction?" + $"{message.Id}!" + $"{smile}!";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                }
                else if (mess == "UnsetReactMess")
                {
                    Console.WriteLine("Enter a date of send message (Format hh:mm:ss) :");
                    string date_of_mess = Console.ReadLine();
                    Message? message = null;
                    int index = 0;
                    foreach (var item in messages[curr_chat.Id])
                    {
                        if (item.date.ToString("hh:mm:ss") == date_of_mess)
                        {
                            message = item;
                            break;
                        }
                        ++index;
                    }
                    if (message == null)
                    {
                        Console.WriteLine("This message not exist");
                        continue;
                    }
                    string request = "UnsetReaction?" + $"{message.Id}";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                }
                else
                {
                    string mess_text = mess;
                    mess = $"{user.Name}: " + mess;
                    Message message = new Message() { Text = mess, chatId = curr_chat.Id, date = DateTime.Now, userId = user.Id };
                    Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
                    Console.WriteLine($"{message.date.ToString("hh:mm:ss")} me: {mess_text}");
                    string request = "AddMessage?";
                    request += JsonSerializer.Serialize(message) + "!" + JsonSerializer.Serialize(curr_chat.usersId) + "!";
                    await RR_writer.WriteLineAsync(request);
                    await RR_writer.FlushAsync();
                    string response = await RR_reader.ReadLineAsync();
                    message = JsonSerializer.Deserialize<Message>(response);
                    messages[curr_chat.Id].Add(message);
                }
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
                                    old_chat.usersId = chat.usersId;
                                    if (curr_chat.Id == old_chat.Id)
                                    {
                                        curr_chat = old_chat;
                                    }
                                }
                                break;
                            }
                        case "DeleteMessage?":
                            {
                                int chatId = int.Parse(response.Substring(0, response.IndexOf('!')));
                                response = response.Remove(0, response.IndexOf('!') + 1);
                                int messId = int.Parse(response.Substring(0, response.IndexOf('!')));
                                if(messages.ContainsKey(chatId))
                                {
                                    Message? mess = null;
                                    foreach (var message in messages[chatId])
                                    {
                                        if (message.Id == messId)
                                            mess = message;
                                    }
                                    if (mess != null)
                                    {
                                        messages[chatId].Remove(mess);
                                        if (chatId == curr_chat.Id)
                                        {
                                            Console.Clear();
                                            Console.WriteLine("Write ExitChat to exit chat");
                                            Console.WriteLine("Write DeleteMess to delete message");
                                            Console.WriteLine("Write EditMess to edit message");
                                            Console.WriteLine("Write SetReactMess to set reaction on message");
                                            Console.WriteLine("Write UnsetReactMess to unset reaction on message\n");
                                            foreach (var item in messages[curr_chat.Id])
                                            {
                                                if (item.Text.Substring(0, item.Text.IndexOf(':')) == $"{user.Name}")
                                                {
                                                    item.Text = item.Text.Remove(0, item.Text.IndexOf(':'));
                                                    item.Text = "me" + item.Text;
                                                }
                                                Console.WriteLine($"{item.date.ToString("hh:mm:ss")} {item.Text}");
                                                PrintReactions(item);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case "EditMessage?":
                            {
                                Message message = JsonSerializer.Deserialize<Message>(response);
                                if (messages.ContainsKey(message.chatId))
                                {
                                    int i = 0;
                                    foreach (var item in messages[message.chatId])
                                    {
                                        if (item.Id == message.Id)
                                            break;
                                        ++i;
                                    }
                                    if(i != messages[message.chatId].Count)
                                    {
                                        messages[message.chatId][i] = message;
                                        if (message.chatId == curr_chat.Id)
                                        {
                                            Console.Clear();
                                            Console.WriteLine("Write ExitChat to exit chat");
                                            Console.WriteLine("Write DeleteMess to delete message");
                                            Console.WriteLine("Write EditMess to edit message");
                                            Console.WriteLine("Write SetReactMess to set reaction on message");
                                            Console.WriteLine("Write UnsetReactMess to unset reaction on message\n");
                                            foreach (var item in messages[curr_chat.Id])
                                            {
                                                if (item.Text.Substring(0, item.Text.IndexOf(':')) == $"{user.Name}")
                                                {
                                                    item.Text = item.Text.Remove(0, item.Text.IndexOf(':'));
                                                    item.Text = "me" + item.Text;
                                                }
                                                Console.WriteLine($"{item.date.ToString("hh:mm:ss")} {item.Text}");
                                                PrintReactions(item);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case "SetReaction?":
                            {
                                int chatId = int.Parse(response.Substring(0, response.IndexOf("!")));
                                response = response.Remove(0, response.IndexOf("!") + 1);
                                Message mess = JsonSerializer.Deserialize<Message>(response.Substring(0, response.IndexOf("!")));
                                foreach (var itemm in messages[chatId])
                                {
                                    if(itemm.Id == mess.Id)
                                    {
                                        itemm.userReactions = mess.userReactions;
                                        if(curr_chat?.Id == chatId)
                                        {
                                            Console.Clear();
                                            Console.WriteLine("Write ExitChat to exit chat");
                                            Console.WriteLine("Write DeleteMess to delete message");
                                            Console.WriteLine("Write EditMess to edit message");
                                            Console.WriteLine("Write SetReactMess to set reaction on message");
                                            Console.WriteLine("Write UnsetReactMess to unset reaction on message\n");
                                            foreach (var item in messages[curr_chat.Id])
                                            {
                                                if (item.Text.Substring(0, item.Text.IndexOf(':')) == $"{user.Name}")
                                                {
                                                    item.Text = item.Text.Remove(0, item.Text.IndexOf(':'));
                                                    item.Text = "me" + item.Text;
                                                }
                                                Console.WriteLine($"{item.date.ToString("hh:mm:ss")} {item.Text}");
                                                PrintReactions(item);
                                            }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        case "UnsetReaction?":
                            {
                                int chatId = int.Parse(response.Substring(0, response.IndexOf("!")));
                                response = response.Remove(0, response.IndexOf("!") + 1);
                                Message mess = JsonSerializer.Deserialize<Message>(response.Substring(0, response.IndexOf("!")));
                                foreach (var itemm in messages[chatId])
                                {
                                    if (itemm.Id == mess.Id)
                                    {
                                        itemm.userReactions = mess.userReactions;
                                        if (curr_chat?.Id == chatId)
                                        {
                                            Console.Clear();
                                            Console.WriteLine("Write ExitChat to exit chat");
                                            Console.WriteLine("Write DeleteMess to delete message");
                                            Console.WriteLine("Write EditMess to edit message");
                                            Console.WriteLine("Write SetReactMess to set reaction on message");
                                            Console.WriteLine("Write UnsetReactMess to unset reaction on message\n");
                                            foreach (var item in messages[curr_chat.Id])
                                            {
                                                if (item.Text.Substring(0, item.Text.IndexOf(':')) == $"{user.Name}")
                                                {
                                                    item.Text = item.Text.Remove(0, item.Text.IndexOf(':'));
                                                    item.Text = "me" + item.Text;
                                                }
                                                Console.WriteLine($"{item.date.ToString("hh:mm:ss")} {item.Text}");
                                                PrintReactions(item);
                                            }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        case "DeleteReaction?":
                            {
                                int chatId = int.Parse(response.Substring(0, response.IndexOf("!")));
                                response = response.Remove(0, response.IndexOf("!") + 1);
                                int messId = int.Parse(response.Substring(0, response.IndexOf("!")));
                                response = response.Remove(0, response.IndexOf("!") + 1);
                                string react = response.Substring(0, response.IndexOf("!"));
                                foreach (var itemm in messages[chatId])
                                {
                                    if (itemm.Id == messId)
                                    {
                                        if (itemm.userReactions.ContainsKey(react))
                                            itemm.userReactions[react].Add(1);
                                        else
                                            itemm.userReactions[react] = new List<int>() { 1 };
                                        if (curr_chat?.Id == chatId)
                                        {
                                            Console.Clear();
                                            Console.WriteLine("Write ExitChat to exit chat");
                                            Console.WriteLine("Write DeleteMess to delete message");
                                            Console.WriteLine("Write EditMess to edit message");
                                            Console.WriteLine("Write SetReactMess to set reaction on message");
                                            Console.WriteLine("Write UnsetReactMess to unset reaction on message\n");
                                            foreach (var item in messages[curr_chat.Id])
                                            {
                                                if (item.Text.Substring(0, item.Text.IndexOf(':')) == $"{user.Name}")
                                                {
                                                    item.Text = item.Text.Remove(0, item.Text.IndexOf(':'));
                                                    item.Text = "me" + item.Text;
                                                }
                                                Console.WriteLine($"{item.date.ToString("hh:mm:ss")} {item.Text}");
                                                PrintReactions(item);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case "UserLeaveChat?":
                            {
                                int userId = int.Parse(response.Substring(0, response.IndexOf("!")));
                                response = response.Remove(0, response.IndexOf("!") + 1);
                                int chatId = int.Parse(response.Substring(0, response.IndexOf("!")));
                                foreach (var item in chats)
                                {
                                    if(item.Id == chatId)
                                        item.usersId.Remove(userId);
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
                Console.WriteLine($"{message.date.ToString("hh:mm:ss")} {message.Text}");
                // переносим курсор на следующую строку
                // и пользователь продолжает ввод уже на следующей строке
                Console.SetCursorPosition(left, top + 1);
            }
            else Console.WriteLine(message);
        }

        public void PrintReactions(Message mess)
        {
            List<string> emots = mess.userReactions.Keys.ToList();
            foreach (var emot in emots)
            {
                Console.Write($"{emot}-{mess.userReactions[emot].Count}  ");
            }
            if(emots.Count != 0)
            { Console.WriteLine(); }
        }
    }

}
