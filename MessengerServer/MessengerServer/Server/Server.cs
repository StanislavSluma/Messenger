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

namespace MessengerServer.Server
{
    public class Server
    {
        private string host = "127.0.0.1"; //"192.168.222.205" "127.0.0.1"
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
                    await socket.N_writer.WriteLineAsync("Message?" + JsonSerializer.Serialize(message));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task DeleteMessageFromClients(int messId, Chat chat)
        {
            foreach (var socket in sockets)
            {
                if(chat.usersId.Contains(socket.user.Id))
                {
                    await socket.N_writer.WriteLineAsync("DeleteMessage?" + $"{chat.Id}!" + $"{messId}!");
                    await socket.N_writer.FlushAsync();
                }
            }
        }
        public async Task SendEditMessageToClients(Message mess, List<int> usersId) // sender, recievers
        {
            foreach (var socket in sockets)
            {
                if (usersId.Contains(socket.user.Id))
                {
                    await socket.N_writer.WriteLineAsync("EditMessage?" + JsonSerializer.Serialize(mess));
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task UserSetReaction(Chat chat, Message mess)
        {
            foreach (var socket in sockets)
            {
                if (chat.usersId.Contains(socket.user.Id))
                {
                    await socket.N_writer.WriteLineAsync("SetReaction?" + $"{chat.Id}!" + $"{JsonSerializer.Serialize(mess)}!");
                    await socket.N_writer.FlushAsync();
                }
            }
        }

        public async Task UserUnsetReaction(Chat chat, Message mess)
        {
            foreach (var socket in sockets)
            {
                if (chat.usersId.Contains(socket.user.Id))
                {
                    await socket.N_writer.WriteLineAsync("UnsetReaction?" + $"{chat.Id}!" + $"{JsonSerializer.Serialize(mess)}!");
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
                    await socket.N_writer.WriteLineAsync("Chat?" + JsonSerializer.Serialize(chat));
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
                    await socket.N_writer.WriteLineAsync("UserLeaveChat?" + $"{userId}!" + $"{chat.Id}!");
                    await socket.N_writer.FlushAsync();
                }
            }
        }
    }
}
