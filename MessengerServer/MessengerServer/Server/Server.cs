using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MessengerServer.Application;
using MessengerServer.Domain.Entities;
using System.Text.Json;
using Serializator__Deserializator;

namespace MessengerServer.Server
{
    public class Server
    {
        private string host = "192.168.238.205"; //"192.168.238.205" "127.0.0.1"
        private int RR_port = 8888;
        private int N_port = 8080;
        private TcpListener RR_server; // server for get Request and gain Response
        private TcpListener N_server; // server for Notification client about changes (messages, chats and etc.)
        private List<ClientSocket> sockets;
        private ApplicationService app;

        public Server(ApplicationService app)
        {
            RR_server = new TcpListener(IPAddress.Parse(host), RR_port);
            N_server = new TcpListener(IPAddress.Parse(host), N_port);
            sockets = new List<ClientSocket>();
            this.app = app;
        }

        public void RemoveConnection(int userId)
        {
            ClientSocket? socket = sockets.FirstOrDefault(s => s.user.Id == userId);
            if (socket != null)
                sockets.Remove(socket);
            socket?.Close();
        }

        private void Disconnect()
        {
            foreach (var socket in sockets)
                socket?.Close();
            sockets.Clear();
            RR_server.Stop();
            N_server?.Stop();
        }

        public async Task ListenAsync()
        {
            try
            {
                RR_server.Start();
                N_server.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                while (true)
                {
                    TcpClient tcpClient_RR = await RR_server.AcceptTcpClientAsync();
                    TcpClient tcpClient_N = await N_server.AcceptTcpClientAsync();
                    // добавить проверку на совпадение RR и N соединений
                    Console.WriteLine($"Подключение: {tcpClient_RR.Client.RemoteEndPoint}");
                    ClientSocket socket_client = new ClientSocket(this, tcpClient_RR, tcpClient_N, app);
                    sockets.Add(socket_client);

                    // отдельный поток для взаимодействия с клиентом
                    socket_client.ListenAsync();
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

        public async Task SendMessageToClients(Message message, List<int> usersId) // sender, recievers
        {
            foreach (var socket in sockets)
            {
                if (usersId.Contains(socket.user.Id))
                {
                    if (socket.user.Id == message.userId)
                        continue;
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("Message?", message));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task DeleteMessageFromClients(int userId, int messId, Chat chat)
        {
            foreach (var socket in sockets)
            {
                if(chat.usersId.Contains(socket.user.Id))
                {
                    if (socket.user.Id == userId)
                        continue;
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("DeleteMessage?", chat.Id, messId));
                    await socket.N_writer.FlushAsync();
                }
            }
        }
        public async Task SendEditMessageToClients(int userId, Message mess, List<int> usersId) // sender, recievers
        {
            foreach (var socket in sockets)
            {
                if (usersId.Contains(socket.user.Id))
                {
                    if (socket.user.Id == userId)
                        continue;
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("EditMessage?", mess));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task UserSetReaction(int userId, Chat chat, Message mess)
        {
            foreach (var socket in sockets)
            {
                if (chat.usersId.Contains(socket.user.Id))
                {
                    if (socket.user.Id == userId)
                        continue;
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("SetReaction?", mess));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task UserUnsetReaction(int userId, Chat chat, Message mess)
        {
            foreach (var socket in sockets)
            {
                if (chat.usersId.Contains(socket.user.Id))
                {
                    if (socket.user.Id == userId)
                        continue;
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("UnsetReaction?", mess));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task AddUsersToChat(Chat chat)
        {
            foreach (var socket in sockets)
            {
                if (chat.usersId.Contains(socket.user.Id))
                {
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("Chat?", chat));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task UserLeaveChat(Chat chat, int userId)
        {
            foreach (var socket in sockets)
            {
                if (chat.usersId.Contains(socket.user.Id))
                {
                    await socket.N_writer.WriteLineAsync(RequestSerializer.Serialize("UserLeaveChat?", userId, chat.Id));
                    await socket.N_writer.FlushAsync();
                }
            }
        }
    }
}
