using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Pages;
using MessengerClientMaui.Popups;
using MessengerClientMaui.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClientMaui.ViewModels
{
    public partial class UserChatsViewModel : ObservableObject
    {
        Client client;

        public UserChatsViewModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
        }

        public ObservableCollection<Chat> Chats { get; set; } = new ObservableCollection<Chat>();

        [RelayCommand]
        public async Task UpdateChats()
        {
            AddHandlers();
            string? response = await client.Request("GetAllChats?");
            List<Chat>? new_chats = JsonSerializer.Deserialize<List<Chat>>(response);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Chats.Clear();
                foreach (var chat in new_chats)
                    Chats.Add(chat);
            });
        }

        [RelayCommand]
        public async Task GotoChatPage(Chat selected_chat)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>() { { "Selected_chat", selected_chat } };
            await Shell.Current.GoToAsync(nameof(ChatPage), parameters);
        }

        [RelayCommand]
        public async Task GoToChangeUserPage()
        {
            await Shell.Current.GoToAsync(nameof(ChangeUserPage));
        }

        [RelayCommand]
        public async Task CreateChat()
        {
            string? response = await client.Request("CreateChat?", "DefaultChat");
            if (response == null) return;
            Chat new_chat = JsonSerializer.Deserialize<Chat>(response);
            Chats.Add(new_chat);
            IDictionary<string, object> parameters = new Dictionary<string, object>() { { "Selected_chat", new_chat } };
            await Shell.Current.GoToAsync(nameof(ChatPage), parameters);
        }

        public void AddHandlers()
        {
            UnloadedEventHandler();
            client.UserAddedToChatHandler += this.UserAddedToChat;
            client.UserDeletedFromChatHandler += this.UserDeletedFromChat;
            client.UpdateChatHandler += this.UpdateChat;
            client.DeleteChatHandler += this.DeleteChat;
            client.UserLeaveChatHandler += this.UserLeaveChat;
        }

        [RelayCommand]
        public void UnloadedEventHandler()
        {
            client.UserAddedToChatHandler -= this.UserAddedToChat;
            client.UserDeletedFromChatHandler -= this.UserDeletedFromChat;
            client.UpdateChatHandler -= this.UpdateChat;
            client.DeleteChatHandler -= this.DeleteChat;
            client.UserLeaveChatHandler -= this.UserLeaveChat;
        }

        public void UpdateChat(Chat chat)
        {
            for (int i = 0; i < Chats.Count; i++)
            {
                if(Chats[i].Id == chat.Id)
                {
                    Chats.RemoveAt(i);
                    Chats.Insert(i, chat);
                }    
            }
        }

        public void DeleteChat(int chat_id)
        {
            for(int i = 0; i < Chats.Count; i++)
            {
                if (Chats[i].Id == chat_id)
                {
                    Chats.RemoveAt(i);
                }
            }
        }

        public void UserAddedToChat(Chat chat, string added_user)
        {
            bool new_chat = true;
            for (int i = 0; i < Chats.Count; i++)
            {
                new_chat &= Chats[i].Id != chat.Id;
                if(!new_chat)
                {
                    Chats[i].usersId = chat.usersId;
                    break;
                }
            }
            if (new_chat)
            {
                Chats.Add(chat);
            }
        }

        public void UserDeletedFromChat(Chat chat, string deleted_user, int deleted_user_id)
        {

            for (int i = 0; i < Chats.Count; i++)
            {
                if (Chats[i].Id == chat.Id)
                {
                    if (client.ID == deleted_user_id)
                    {
                        Chats.RemoveAt(i);
                    }
                    else
                    {
                        Chats[i].usersId = chat.usersId;
                    }
                    break;
                }
            }
        }

        public void UserLeaveChat(int user_id, string user_name, int chat_id)
        {
            for (int i = 0; i < Chats.Count; i++)
            {
                if (Chats[i].Id == chat_id)
                {
                    Chats[i].usersId.Remove(user_id);
                }
            }
        }
    }
}
