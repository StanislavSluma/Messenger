using Serializator__Deserializator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessengerClientMaui.Domain.Entities;
using System.Text.Json;
using System.Security.AccessControl;


namespace MessengerClientMaui
{
    public class Client
    {
        string host = "192.168.238.205"; // "127.0.0.1" "192.168.238.205"
        int RR_port = 8888;
        int N_port = 8080;
        public int ID = 0;
        public string nickname = "";
        public string? acces_token = null;
        public object? value = null;
        TcpClient RR_client;
        TcpClient N_client;
        StreamReader RR_reader = null!;
        StreamReader N_reader = null!;
        StreamWriter RR_writer = null!;
        StreamWriter N_writer = null!;

        public delegate void MessageReceive(Message? mess);
        public event MessageReceive? MessageReceiveHandler;
        public delegate void MessageDelete(int chat_id, int mess_id);
        public event MessageDelete? MessageDeleteHandler;
        public delegate void MessageEdit(Message? mess);
        public event MessageEdit? MessageEditHandler;

        public delegate void SetReaction(Message? mess);
        public event SetReaction? SetReactionHandler;
        public delegate void UnsetReaction(Message? mess);
        public event UnsetReaction? UnsetReactionHandler;

        public delegate void UpdateChat(Chat chat);
        public event UpdateChat? UpdateChatHandler;
        public delegate void DeleteChat(int chat_id);
        public event DeleteChat? DeleteChatHandler;
        public delegate void UserAddedToChat(Chat chat, string added_user);
        public event UserAddedToChat? UserAddedToChatHandler;
        public delegate void UserDeletedFromChat(Chat chat, string deleted_user, int deleted_user_id);
        public event UserDeletedFromChat? UserDeletedFromChatHandler;
        public delegate void UserLeaveChat(int user_id, string user_name, int chat_id);
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
            string request;
            if (tag == "SignIn?" || tag == "SignUp?")
            {
                request = RequestSerializer.Serialize(tag, objs);
            }
            else
            {
                object[] ob = new object[objs.Length + 1];
                Array.Copy(objs, 0, ob, 1, objs.Length);
                ob[0] = acces_token;
                request = RequestSerializer.Serialize(tag, ob);
            }
            await RR_writer.WriteLineAsync(request);
            await RR_writer.FlushAsync();
            string? response = await RR_reader.ReadLineAsync();
            Trace.WriteLine(response);
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
                                MessageReceiveHandler?.Invoke(mess);
                                break;
                            }
                        case "DeleteMessage?":
                            {
                                int chat_id = JsonSerializer.Deserialize<int>(parse_response[1]);
                                int mess_id = JsonSerializer.Deserialize<int>(parse_response[2]);
                                MessageDeleteHandler?.Invoke(chat_id, mess_id);
                                break;
                            }
                        case "EditMessage?":
                            {
                                Message? mess = JsonSerializer.Deserialize<Message>(parse_response[1]);
                                MessageEditHandler?.Invoke(mess);
                                break;
                            }
                        case "SetReaction?":
                            {
                                Message? mess = JsonSerializer.Deserialize<Message>(parse_response[1]);
                                SetReactionHandler?.Invoke(mess);
                                break;
                            }
                        case "UnsetReaction?":
                            {
                                Message? mess = JsonSerializer.Deserialize<Message>(parse_response[1]);
                                UnsetReactionHandler?.Invoke(mess);
                                break;
                            }
                        case "UpdateChat?":
                            {
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_response[1]);
                                UpdateChatHandler?.Invoke(chat);
                                break;
                            }
                        case "DeleteChat?":
                            {
                                int chat_id = int.Parse(parse_response[1]);
                                DeleteChatHandler?.Invoke(chat_id);
                                break;
                            }
                        case "UserAddedToChat?":
                            {
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_response[1]);
                                string added_user = parse_response[2];
                                UserAddedToChatHandler?.Invoke(chat, added_user);
                                break;
                            }
                        case "UserDeletedFromChat?":
                            {
                                Chat chat = JsonSerializer.Deserialize<Chat>(parse_response[1]);
                                string deleted_user = parse_response[2];
                                int deleted_user_id = int.Parse(parse_response[3]);
                                UserDeletedFromChatHandler?.Invoke(chat, deleted_user, deleted_user_id);
                                break;
                            }
                        case "UserLeaveChat?":
                            {
                                int user_id = int.Parse(parse_response[1]);
                                string user_name = parse_response[2];
                                int chat_id = int.Parse(parse_response[3]);
                                UserLeaveChatHandler?.Invoke(user_id, user_name, chat_id);
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
