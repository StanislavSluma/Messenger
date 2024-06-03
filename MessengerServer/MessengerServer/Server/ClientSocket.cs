using MessengerServer.Application;
using MessengerServer.Domain.Entities;
using Serializator__Deserializator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerServer.Server
{
    public class ClientSocket
    {
        Server server;
        public TcpClient RR_socket;
        public TcpClient N_socket;
        public User user;
        public StreamWriter RR_writer;
        public StreamReader RR_reader;
        public StreamWriter N_writer;
        public StreamReader N_reader;
        private ApplicationService app;

        public ClientSocket(Server server, TcpClient RR_socket, TcpClient N_socket, ApplicationService app)
        {
            this.app = app;
            this.server = server;
            this.RR_socket = RR_socket;
            this.N_socket = N_socket;
            user = null!;
            var RR_stream = this.RR_socket.GetStream();
            var N_stream = this.N_socket.GetStream();
            RR_writer = new StreamWriter(RR_stream);
            RR_reader = new StreamReader(RR_stream);
            N_writer = new StreamWriter(N_stream);
            N_reader = new StreamReader(N_stream);
        }

        public async Task ListenAsync()
        {
            try
            {
                while (true)
                {
                    string? request = await RR_reader.ReadLineAsync();
                    List<string> parse_request = RequestSerializer.Deserializer(request);
                    string tag = parse_request[0];
                    switch(tag)
                    {
                        case "SignUp?":
                            {
                                string response = "";
                                User IsUser = JsonSerializer.Deserialize<User>(parse_request[1]);
                                IsUser = await app.userService.SignUp(IsUser);
                                if (IsUser == null)
                                    response = "Error";
                                else
                                {
                                    response = JsonSerializer.Serialize(IsUser);
                                    this.user = IsUser;
                                }
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "SignIn?":
                            {
                                string response = "";
                                User IsUser = JsonSerializer.Deserialize<User>(parse_request[1]);
                                IsUser = await app.userService.SignIn(IsUser);
                                if (IsUser == null)
                                    response = "Error";
                                else
                                {
                                    response = JsonSerializer.Serialize(IsUser);
                                    this.user = IsUser;
                                }
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "GetAllChats?":
                            {
                                user = await app.userService.GetUserById(user.Id);
                                List<Chat> chats = await app.chatService.GetChats(this.user);
                                string response = JsonSerializer.Serialize(chats);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "GetChatMessages?":
                            {
                                int chatId = int.Parse(parse_request[1]);
                                List<Message> messages = await app.messageService.GetMessages(chatId);
                                string response = JsonSerializer.Serialize(messages);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "GetChatUsers?":
                            {
                                string response = "Error";
                                Chat? chat = JsonSerializer.Deserialize<Chat>(parse_request[1]);
                                if (chat != null)
                                {
                                    List<User> users = new List<User>();
                                    foreach (int userId in chat.usersId)
                                    {
                                        User? user = await app.userService.GetUserById(userId);
                                        if (user != null)
                                            users.Add(user);
                                    }
                                    response = JsonSerializer.Serialize(users);
                                }
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "GetUsersByName?":
                            {
                                string user_name = parse_request[1];
                                List<User> users = await app.userService.GetUsersByName(user_name);
                                string response = JsonSerializer.Serialize(users);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "CreateChat?":
                            {
                                string chat_name = parse_request[1];
                                Chat chat = new Chat() { Name = chat_name};
                                chat.usersId.Add(this.user.Id);
                                chat.user_id = this.user.Id;
                                chat = await app.chatService.AddChat(chat);
                                string response = JsonSerializer.Serialize(chat);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "UpdateChat?":
                            {
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_request[1]);
                                chat = await app.chatService.UpdateChat(chat);
                                server.UpdateChat(user.Id, chat);
                                await RR_writer.WriteLineAsync("Success");
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "DeleteChat?":
                            {        
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_request[1]);
                                chat = await app.chatService.DeleteChat(chat);
                                server.DeleteChat(user.Id, chat);
                                await RR_writer.WriteLineAsync("Success");
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "AddUserToChat?":
                            {
                                string response = "";
                                int user_id = int.Parse(parse_request[1]);
                                Chat? chat = JsonSerializer.Deserialize<Chat>(parse_request[2]);
                                User add_user = await app.userService.GetUserById(user_id);
                                chat = await app.chatService.AddUser(chat.Id, user_id);
                                server.AddUserToChat(user.Id, chat, add_user.Name);
                                response = JsonSerializer.Serialize(chat);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "DeleteUserFromChat?":
                            {
                                string response = "";
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_request[1]);
                                User deleted_user = JsonSerializer.Deserialize<User>(parse_request[2]);

                                await app.chatService.UpdateChat(chat);
                                await app.userService.UpdateUser(deleted_user);
                                server.DeleteUserFromChat(user.Id, chat, deleted_user.Name, deleted_user.Id);
                                response = JsonSerializer.Serialize(chat);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "AddMessage?":
                            {
                                string mess = parse_request[1];
                                string usersId_str = parse_request[2];
                                List<int> usersId = JsonSerializer.Deserialize<List<int>>(usersId_str);
                                Message message = JsonSerializer.Deserialize<Message>(mess);
                                // добавляем в базу данных
                                message = await app.messageService.AddMessage(message);
                                // отправляем всем пользователям
                                server.SendMessageToClients(message, usersId);
                                string response = JsonSerializer.Serialize(message);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "DeleteMessage?":
                            {
                                int messId = int.Parse(parse_request[1]);
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_request[2]);
                                await app.messageService.RemoveMessage(messId);
                                server.DeleteMessageFromClients(user.Id, messId, chat);
                                await RR_writer.WriteLineAsync("Success");
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "EditMessage?":
                            {
                                Message mess = JsonSerializer.Deserialize<Message>(parse_request[1]);
                                List<int> usersId = JsonSerializer.Deserialize<List<int>>(parse_request[2]);
                                server.SendEditMessageToClients(user.Id, mess, usersId);
                                await app.messageService.EditMessage(mess);
                                await RR_writer.WriteLineAsync("Success");
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "SetReaction?":
                            {
                                int messId = int.Parse(parse_request[1]);
                                string react = parse_request[2];
                                Message mess = await app.messageService.SetReaction(messId, user.Id, react);
                                Chat chat = await app.chatService.GetChat(mess.chatId);
                                server.UserSetReaction(user.Id, chat, mess);
                                await RR_writer.WriteLineAsync(JsonSerializer.Serialize(mess));
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "UnsetReaction?":
                            {
                                int messId = int.Parse(parse_request[1]);
                                Message mess = await app.messageService.UnsetReaction(messId, user.Id);
                                Chat chat = await app.chatService.GetChat(mess.chatId);
                                server.UserUnsetReaction(user.Id, chat, mess);
                                await RR_writer.WriteLineAsync(JsonSerializer.Serialize(mess));
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "LeaveChat?":
                            {
                                int chatId = JsonSerializer.Deserialize<int>(parse_request[1]);
                                Chat chat = await app.chatService.DeleteUser(chatId, user.Id);
                                server.UserLeaveChat(chat, user.Id, user.Name);
                                await RR_writer.WriteLineAsync("Success");
                                await RR_writer.FlushAsync();
                                break;
                            }
                        default:
                            {
                                await RR_writer.WriteLineAsync("Error this request not parse -> SOS");
                                await RR_writer.FlushAsync();
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(user.Id);
            }
        }

        public void Close()
        {
            RR_writer.Close();
            RR_reader.Close();
            N_writer.Close();
            N_reader.Close();
            RR_socket.Close();
            N_socket.Close();
        }
    }
}
