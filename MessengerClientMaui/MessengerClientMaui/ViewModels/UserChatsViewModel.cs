using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Pages;
using MessengerClientMaui.Popups;
using MessengerServer.Domain.Entities;
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
            string? response = await client.Request("GetAllChats");
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
            //await Shell.Current.CurrentPage.ShowPopupAsync(new MessagePopup());
            IDictionary<string, object> parameters = new Dictionary<string, object>() { { "Selected_chat", selected_chat } };
            await Shell.Current.GoToAsync(nameof(ChatPage), parameters);
        }
    }
}
