using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClientMaui.ViewModels
{
    public partial class ChangeUserViewModel : ObservableObject
    {
        Client client;
        User curr_user;
        [ObservableProperty]
        string name = "";
        [ObservableProperty]
        string description = "";

        public ChangeUserViewModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
        }

        [RelayCommand]
        public async Task UpdateUser()
        {
            string? response = await client.Request("GetUser?", client.ID);
            curr_user = JsonSerializer.Deserialize<User>(response);

            Name = curr_user.Name;
            Description = curr_user.Description;
        }

        [RelayCommand]
        public async Task Save()
        {
            if (Name == "")
            {
                await Shell.Current.CurrentPage.DisplayAlert("Информация", "Имя не должно быть пустым", "ОК");
                return;
            }
            curr_user.Name = Name;
            curr_user.Description = Description;
            string? response = await client.Request("UpdateUser?", curr_user);
            await Shell.Current.GoToAsync("..");
        }
    }
}
