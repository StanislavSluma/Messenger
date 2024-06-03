using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Domain.Entities;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MessengerClientMaui.ViewModels
{
    public partial class FindUsersPopupModel : ObservableObject
    {
        Client client;

        public ObservableCollection<User> users { get; set; } = new();

        public FindUsersPopupModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
        }

        [RelayCommand]
        public async Task SearchUsers(string user_name)
        {
            string? response = await client.Request("GetUsersByName?", user_name);
            if (response == null) return;

            List<User>? res_users = JsonSerializer.Deserialize<List<User>>(response);
            if (res_users == null) return;

            users.Clear();
            foreach (User user in res_users)
            {
                users.Add(user);
            }
        }
    }
}
