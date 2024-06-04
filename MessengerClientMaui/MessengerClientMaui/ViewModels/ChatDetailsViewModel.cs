using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MessengerClientMaui.ViewModels
{
    [QueryProperty("Current_chat", "Current_chat")]
    public partial class ChatDetailsViewModel : ObservableObject
    {
        Client client;
        [ObservableProperty]
        Chat current_chat = null!;

        public ObservableCollection<User> chat_users { get; set; } = new();

        public ChatDetailsViewModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
        }

        [RelayCommand]
        public async Task UpdateUsers()
        {
            string? response = await client.Request("GetChatUsers?", Current_chat);
            if (response == null)
                return;

            List<User>? users = JsonSerializer.Deserialize<List<User>>(response);
            if (users == null)
                return;

            chat_users.Clear();
            foreach (User user in users)
            {
                if (user.Id != client.ID)
                    chat_users.Add(user);
            }
        }
    }
}
