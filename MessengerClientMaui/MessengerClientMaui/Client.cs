using Serializator__Deserializator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessengerServer.Domain.Entities;
using System.Text.Json;


namespace MessengerClientMaui
{
    public class Client
    {
        string host = "192.168.250.205"; // "127.0.0.1" "192.168.250.205"
        int RR_port = 8888;
        int N_port = 8080;
        public int ID = 0;
        TcpClient RR_client;
        TcpClient N_client;
        StreamReader RR_reader = null!;
        StreamReader N_reader = null!;
        StreamWriter RR_writer = null!;
        StreamWriter N_writer = null!;

        public delegate void ChatReceive();
        public event ChatReceive? ChatReceiveHandler;
        public delegate void ChatDelete();
        public event ChatDelete? ChatDeleteHandler;
        public delegate void MessageReceive(Message? mess);
        public event MessageReceive? MessageReceiveHandler;
        public delegate void MessageDelete();
        public event MessageDelete? MessageDeleteHandler;
        public delegate void MessageEdit();
        public event MessageEdit? MessageEditHandler;
        public delegate void SetReaction();
        public event SetReaction? SetReactionHandler;
        public delegate void UnsetReaction();
        public event UnsetReaction? UnsetReactionHandler;
        public delegate void UserLeaveChat();
        public event UserLeaveChat? UserLeaveChatHandler;


        public Client()
        {
            RR_client = new TcpClient();
            N_client = new TcpClient();
        }

        public async Task Start()
        {
            RR_client.Connect(host, RR_port);
            N_client.Connect(host, N_port);
            RR_reader = new StreamReader(RR_client.GetStream());
            N_reader = new StreamReader(N_client.GetStream());
            RR_writer = new StreamWriter(RR_client.GetStream());
            N_writer = new StreamWriter(N_client.GetStream());
            await ServerNotify();
        }

        public async Task<string?> Request(string tag, params object[] objs)
        {
            string request = RequestSerializer.Serialize(tag, objs);
            await RR_writer.WriteLineAsync(request);
            await RR_writer.FlushAsync();
            string? response = await RR_reader.ReadLineAsync();
            return response;
        }

        private async Task ServerNotify()
        {
            try
            {
                while (true)
                {
                    string response = await N_reader.ReadLineAsync() ?? "";
                    List<string> parse_response = RequestSerializer.Deserializer(response);
                    string? tag = parse_response[0];
                    switch (tag)
                    {
                        case "Message?":
                            {
                                Message? mess = JsonSerializer.Deserialize<Message>(parse_response[1]);
                                Trace.WriteLine("New_mess");
                                Trace.WriteLine(mess?.Text);
                                MessageReceiveHandler?.Invoke(mess);
                                break;
                            }
                        case "MessageDelete?":
                            {
                                MessageDeleteHandler?.Invoke();
                                break;
                            }
                        case "MessageEdit?":
                            {
                                MessageEditHandler?.Invoke();
                                break;
                            }
                        case "Chat?":
                            {
                                ChatReceiveHandler?.Invoke();
                                break;
                            }
                        case "SetReaction?":
                            {
                                SetReactionHandler?.Invoke();
                                break;
                            }
                        case "UnsetReaction?":
                            {
                                UnsetReactionHandler?.Invoke();
                                break;
                            }
                        case "UserLeaveChat?":
                            {
                                UserLeaveChatHandler?.Invoke();
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
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
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
    }
}
