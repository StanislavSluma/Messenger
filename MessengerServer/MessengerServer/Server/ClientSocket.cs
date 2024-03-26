using MessengerServer.Application;
using MessengerServer.Domain.Entities;
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
            var RR_stream = RR_socket.GetStream();
            var N_stream = N_socket.GetStream();
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
                    string tag = request.Substring(0, request.IndexOf('?') + 1);
                    request = request.Remove(0, request.IndexOf('?') + 1);
                    switch(tag)
                    {
                        case "SignUp?":
                            {
                                string response = "";
                                User IsUser = JsonSerializer.Deserialize<User>(request);
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
                                User IsUser = JsonSerializer.Deserialize<User>(request);
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
                        case "GetAllChat?":
                            {
                                List<Chat> chats = await app.chatService.GetChats(this.user);
                                string response = JsonSerializer.Serialize(chats);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "GetChatMessages?":
                            {
                                int chatId = int.Parse(request.Substring(0, request.IndexOf("!")));
                                List<Message> messages = await app.messageService.GetMessages(chatId);
                                string response = JsonSerializer.Serialize(messages);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "CreateChat?": // это просто ужас отдельный запрос -> вернуть всех пользоваетелей с таким именем и выбрать на клиенте
                            {
                                string chat_name = request.Substring(0, request.IndexOf("!"));
                                request = request.Remove(0, request.IndexOf("!") + 1);
                                List<string> user_names = JsonSerializer.Deserialize<List<string>>(request);
                                Chat chat = new Chat() { Name = chat_name};
                                foreach (string user_name in user_names)
                                {
                                    int? userId = await app.userService.GetUserId(user_name);
                                    if(userId != null)
                                        chat.usersId.Add((int)userId);
                                }
                                chat.usersId.Add(this.user.Id);
                                chat = await app.chatService.AddChat(chat);
                                server.AddUsersToChat(chat);
                                string response = JsonSerializer.Serialize(chat);
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "AddUserToChat?":
                            {
                                string response = "";
                                string user_name = request.Substring(0, request.IndexOf("!"));
                                request = request.Remove(0, request.IndexOf("!") + 1);
                                Chat chat = JsonSerializer.Deserialize<Chat>(request.Substring(0, request.IndexOf("!")));
                                int? userId = await app.userService.GetUserId(user_name);
                                if (userId == null)
                                {
                                    response = "Error";
                                    await RR_writer.WriteLineAsync(response);
                                    await RR_writer.FlushAsync();
                                    break;
                                }                              
                                chat = await app.chatService.AddUser(chat.Id, (int)userId);
                                server.AddUsersToChat(chat);
                                response = "Succes";
                                await RR_writer.WriteLineAsync(response);
                                await RR_writer.FlushAsync();
                                break;
                            }
                        case "AddMessage?":
                            {
                                string mess = request.Substring(0, request.IndexOf("!"));
                                request = request.Remove(0, request.IndexOf("!") + 1);
                                string usersId_str = request.Substring(0, request.IndexOf('!'));
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
                                int messId = int.Parse(request.Substring(0, request.IndexOf("!")));
                                request = request.Remove(0, request.IndexOf("!") + 1);
                                Chat chat = JsonSerializer.Deserialize<Chat>(request.Substring(0, request.IndexOf('!')));
                                await app.messageService.RemoveMessage(messId);
                                server.DeleteMessageFromClients(messId, chat);
                                break;
                            }
                        case "EditMessage?":
                            {
                                Message mess = JsonSerializer.Deserialize<Message>(request.Substring(0, request.IndexOf('!')));
                                request = request.Remove(0, request.IndexOf('!') + 1);
                                List<int> usersId = JsonSerializer.Deserialize<List<int>>(request.Substring(0, request.IndexOf('!')));
                                server.SendEditMessageToClients(mess, usersId);
                                await app.messageService.EditMessage(mess);
                                break;
                            }
                        case "SetReaction?":
                            {
                                int messId = int.Parse(request.Substring(0, request.IndexOf("!")));
                                request = request.Remove(0, request.IndexOf("!") + 1);
                                string react = request.Substring(0, request.IndexOf("!"));
                                Message mess = await app.messageService.SetReaction(messId, user.Id, react);
                                Chat chat = await app.chatService.GetChat(mess.chatId);
                                server.UserSetReaction(chat, mess);
                                break;
                            }
                        case "UnsetReaction?":
                            {
                                int messId = int.Parse(request);
                                Message mess = await app.messageService.UnsetReaction(messId, user.Id);
                                Chat chat = await app.chatService.GetChat(mess.chatId);
                                server.UserSetReaction(chat, mess);
                                break;
                            }
                        case "LeaveChat?":
                            {
                                int chatId = JsonSerializer.Deserialize<int>(request);
                                Chat chat = await app.chatService.DeleteUser(chatId, user.Id);
                                server.UserLeaveChat(chat, user.Id);
                                break;
                            }
                        default:
                            break;
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
