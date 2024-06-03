using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Domain.Entities;
using MessengerClientMaui.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClientMaui.ViewModels
{
    [QueryProperty("Current_chat", "Current_chat")]
    public partial class ChangeChatViewModel : ObservableObject
    {
        Client client;
        [ObservableProperty]
        Chat current_chat = null!;
        [ObservableProperty]
        string name = "";
        [ObservableProperty]
        string description = "";
        
        public ObservableCollection<User> chat_users { get; set; } = new();


        public ChangeChatViewModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
        }

        [RelayCommand]
        public async Task UpdateUsers()
        {
            Name = Current_chat.Name;
            Description = Current_chat.Description;
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

        [RelayCommand]
        public async Task AddUser()
        {
            var res = await MainThread.InvokeOnMainThreadAsync(async Task<string?> () =>
            {
                return await Shell.Current.CurrentPage.ShowPopupAsync(new FindUsersPopup(new FindUsersPopupModel())) as string;
            });

            if (res == null) return;

            User? added_user = JsonSerializer.Deserialize<User>(res);
            if (added_user == null) return;

            if (Current_chat.usersId.Contains(added_user.Id))
                return;

            string? response = await client.Request("AddUserToChat?", added_user.Id, Current_chat);
            if (response == null) return;

            Current_chat.usersId.Add(added_user.Id);
            chat_users.Add(added_user);
        }

        [RelayCommand]
        public async Task KickOut(User selected_user)
        {
            Current_chat.usersId.Remove(selected_user.Id);
            selected_user.chatsId.Remove(Current_chat.Id);
            chat_users.Remove(selected_user);
            string? response = await client.Request("DeleteUserFromChat?", Current_chat, selected_user); 
        }

        [RelayCommand]
        public async Task Save()
        {
            if (Name == "")
            {
                await Shell.Current.CurrentPage.DisplayAlert("Информация", "Имя чата не должно быть пустым", "ОК");
                return;
            }
            Current_chat.Name = Name;
            Current_chat.Description = Description;
            string? response = await client.Request("UpdateChat?", Current_chat); 
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task DeleteChat()
        {
            bool res = await Shell.Current.CurrentPage.DisplayAlert("Информация", "Ваш чат удалится безвозвратно(\nВы уверены что хотите удалить чат?", "Да", "Нет");
            if (res)
            {
                string? response = await client.Request("DeleteChat?", Current_chat);
                await Shell.Current.GoToAsync("../..");
            }
        }

    }
}
